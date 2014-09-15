/*
 * Emitter.cs
 * 
 * Copyright (c) 2014 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Fred Blundun
 * Copyright: Copyright (c) 2014 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace Snowplow.Tracker
{
    public class Emitter : IEmitter
    {
        private string collectorUri;
        private HttpMethod method;
        private int bufferSize;
        volatile private List<Dictionary<string, string>> buffer;
        private Action<int> onSuccess;
        private Action<int, List<Dictionary<string, string>>> onFailure = null;

        protected static Logger logger = LogManager.GetLogger("Snowplow.Tracker");
        private static ColoredConsoleTarget logTarget = new ColoredConsoleTarget();
        private static LoggingRule loggingRule = new LoggingRule("*", LogLevel.Info, logTarget);
        private static bool loggingConfigured = false;
        private static String noResponseMessage = "Unable to contact server";

        public Emitter(string endpoint, HttpProtocol protocol = HttpProtocol.HTTP, int? port = null, HttpMethod method = HttpMethod.GET, int? bufferSize = null, Action<int> onSuccess = null, Action<int, List<Dictionary<string, string>>> onFailure = null)
        {
            collectorUri = GetCollectorUri(endpoint, protocol, port, method);
            this.method = method;
            this.buffer = new List<Dictionary<string, string>>();
            this.bufferSize = bufferSize ?? (method == HttpMethod.GET ? 0 : 10);
            this.onSuccess = onSuccess;
            this.onFailure = onFailure;
            if (!loggingConfigured)
            {
                logTarget.Layout = "${level}: ${logger}: ${message} ${exception:format=tostring}";
                LogManager.Configuration.LoggingRules.Add(loggingRule);
                loggingConfigured = true;
                SetLogLevel(Logging.Info);
            }
            logger.Info(String.Format("{0} initialized with endpoint {1}", this.GetType(), collectorUri));
        }

        public static void SetLogLevel(Logging newLevel)
        {
            foreach (int level in Enumerable.Range(0,6))
            {
                if (level < (int)newLevel)
                {
                    loggingRule.DisableLoggingForLevel(LogLevel.FromOrdinal(level));
                }
                else
                {
                    loggingRule.EnableLoggingForLevel(LogLevel.FromOrdinal(level));
                }
            }
            LogManager.ReconfigExistingLoggers();
        }

        private static string GetCollectorUri(string endpoint, HttpProtocol protocol, int? port, HttpMethod method)
        {
            string path;
            string requestProtocol = (protocol == HttpProtocol.HTTP) ? "http" : "https";
            if (method == HttpMethod.GET)
            {
                path = "/i";
            }
            else
            {
                path = "/com.snowplowanalytics.snowplow/tp2";
            }
            if (port == null)
            {
                return String.Format("{0}://{1}{2}", requestProtocol, endpoint, path);
            }
            else
            {
                return String.Format("{0}://{1}{2}{3}", requestProtocol, endpoint, port.ToString(), path);
            }
        }

        public void Input(Dictionary<string, string> payload)
        {
            buffer.Add(payload);
            if (buffer.Count >= bufferSize)
            {
                Flush();
            }
        }

        virtual public void Flush(bool sync = false)
        {
            SendRequests();
        }

        protected void SendRequests()
        {
            logger.Info(String.Format("Attempting to send {0} event{1}", buffer.Count, buffer.Count == 1 ? "" : "s"));
            if (method == HttpMethod.POST)
            {
                var tempBuffer = buffer;
                var data = new Dictionary<string, object>
                {
                    { "schema", "iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-0" },
                    { "data", tempBuffer }
                };
                buffer = new List<Dictionary<string, string>>();
                string statusCode = HttpPost(data);
                if (statusCode == "OK")
                {
                    logger.Info(String.Format("POST request to {0} finished with status '{1}'", collectorUri, statusCode));
                    if (onSuccess != null)
                    {
                        onSuccess(tempBuffer.Count);
                    }
                }
                else
                {
                    logger.Warn(String.Format("POST request to {0} finished with status: '{1}'", collectorUri, statusCode));
                    if (onFailure != null)
                    {
                        onFailure(0, tempBuffer);
                    }
                }
            }
            else
            {
                int successCount = 0;
                var unsentRequests = new List<Dictionary<string, string>>();
                while (buffer.Count > 0)
                {
                    var payload = buffer[0];
                    buffer.RemoveAt(0);
                    string statusCode = HttpGet(payload);
                    if (statusCode == "OK")
                    {
                        logger.Info(String.Format("GET request to {0} finished with status: '{1}'", collectorUri, statusCode));
                        successCount += 1;
                    }
                    else
                    {
                        logger.Warn(String.Format("GET request to {0} finished with status: '{1}'", collectorUri, statusCode));
                        unsentRequests.Add(payload);
                    }
                }
                if (unsentRequests.Count == 0)
                {

                    if (onSuccess != null)
                    {
                        onSuccess(successCount);
                    }
                }
                else
                {
                    if (onFailure != null)
                    {
                        onFailure(successCount, unsentRequests);
                    }
                }
            }
        }

        private static string ToQueryString(Dictionary<string, string> payload)
        {
            var array = (from key in payload.Keys
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(payload[key])))
                .ToArray();
            return String.Format("?{0}", String.Join("&", array));
        }

        // See http://stackoverflow.com/questions/9145667/how-to-post-json-to-the-server
        private string HttpPost(Dictionary<string, object> payload)
        {
            logger.Info(String.Format("Sending POST request to {0}", collectorUri));
            logger.Debug(() => String.Format("Payload: {0}", new JavaScriptSerializer(null).Serialize(payload)));
            string destination = collectorUri;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destination);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.UserAgent = "System.Net.HttpWebRequest";

            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer(null).Serialize(payload);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            catch (WebException we)
            {
                return noResponseMessage;
            }

            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                return httpResponse.StatusCode.ToString();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                {
                    return noResponseMessage;
                }
                return resp.StatusCode.ToString();
            }
        }

        private string HttpGet(Dictionary<string, string> payload)
        {
            logger.Info(String.Format("Sending GET request to {0}", collectorUri));
            logger.Debug(() => String.Format("Payload: {0}", new JavaScriptSerializer(null).Serialize(payload)));
            string destination = collectorUri + ToQueryString(payload);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destination);
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode.ToString();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                {
                    return noResponseMessage;
                }
                return resp.StatusCode.ToString();
            }
            
        }

    }
}
