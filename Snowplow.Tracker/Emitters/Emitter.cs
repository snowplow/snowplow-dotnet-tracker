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
using System.Net.NetworkInformation;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace Snowplow.Tracker
{
    public class Emitter : IEmitter, IDisposable
    {
        private string collectorUri;
        private HttpMethod method;
        private int bufferSize;
        volatile private List<Dictionary<string, string>> buffer;
        private Action<int> onSuccess;
        private Action<int, List<Dictionary<string, string>>> onFailure = null;
        private bool offlineModeEnabled;
        private MsmqEmitter backupEmitter;
        private bool disposed = false;

        private static JavaScriptSerializer jss = new JavaScriptSerializer();
        protected static Logger logger = LogManager.GetLogger("Snowplow.Tracker");
        private static ColoredConsoleTarget logTarget = new ColoredConsoleTarget();
        private static LoggingRule loggingRule = new LoggingRule("*", LogLevel.Info, logTarget);
        private static bool loggingConfigured = false;
        private static String noResponseMessage = "Unable to contact server";

        public bool OfflineModeEnabled
        {
            get { return offlineModeEnabled; }
            set { offlineModeEnabled = value;  }
        }

        public Emitter(string endpoint, HttpProtocol protocol = HttpProtocol.HTTP, int? port = null, HttpMethod method = HttpMethod.GET, int? bufferSize = null, Action<int> onSuccess = null, Action<int, List<Dictionary<string, string>>> onFailure = null, bool offlineModeEnabled = true)
        {
            collectorUri = GetCollectorUri(endpoint, protocol, port, method);
            this.method = method;
            this.buffer = new List<Dictionary<string, string>>();
            this.bufferSize = Math.Max(1, bufferSize ?? (method == HttpMethod.GET ? 1 : 10));
            this.onSuccess = onSuccess;
            this.onFailure = onFailure;
            this.offlineModeEnabled = offlineModeEnabled;
            if (!loggingConfigured)
            {
                logTarget.Layout = "${level}: ${logger}: ${message} ${exception:format=tostring}";
                LogManager.Configuration.LoggingRules.Add(loggingRule);
                loggingConfigured = true;
                SetLogLevel(Logging.Info);
            }
            logger.Info(String.Format("{0} initialized with endpoint {1}", this.GetType(), collectorUri));
            if (offlineModeEnabled)
            {
                backupEmitter = new MsmqEmitter(String.Format(".\\private$\\{0}", collectorUri));
                WeakEventManager<NetworkChange, NetworkAvailabilityEventArgs>.AddHandler(null, "NetworkAvailabilityChanged", NetworkAvailabilityChange);
            }
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
                return String.Format("{0}://{1}:{2}{3}", requestProtocol, endpoint, port.ToString(), path);
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
            if (buffer.Count == 0)
            {
                logger.Info("Buffer empty, returning");
                return;
            }

            // Move all requests from buffer into a tempBuffer for thread safety
            var tempBuffer = new List<Dictionary<string, string>>();
            while (buffer.Count > 0)
            {
                tempBuffer.Add(buffer[0]);
                buffer.RemoveAt(0);
            }

            logger.Info(String.Format("Attempting to send {0} event{1}", tempBuffer.Count, tempBuffer.Count == 1 ? "" : "s"));
            if (method == HttpMethod.POST)
            {
                var data = new Dictionary<string, object>
                {
                    { "schema", "iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-0" },
                    { "data", tempBuffer }
                };

                string statusCode = HttpPost(data, collectorUri);
                if (statusCode == "OK")
                {
                    logger.Info(String.Format("POST request to {0} finished with status '{1}'", collectorUri, statusCode));
                    if (onSuccess != null)
                    {
                        onSuccess(tempBuffer.Count);
                    }
                    ResendRequests();
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
                while (tempBuffer.Count > 0)
                {
                    var payload = tempBuffer[0];
                    tempBuffer.RemoveAt(0);
                    string statusCode = HttpGet(payload, collectorUri);
                    if (statusCode == "OK")
                    {
                        logger.Info(String.Format("GET request to {0} finished with status: '{1}'", collectorUri, statusCode));
                        successCount += 1;
                        ResendRequests();
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
        private string HttpPost(Dictionary<string, object> payload, string collectorUri)
        {
            logger.Info(String.Format("Sending POST request to {0}", collectorUri));
            logger.Debug(() => String.Format("Payload: {0}", jss.Serialize(payload)));
            string destination = collectorUri;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destination);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.UserAgent = "System.Net.HttpWebRequest";

            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = jss.Serialize(payload);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }
            }
            catch (WebException we)
            {
                OfflineHandle(payload);
                return noResponseMessage;
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();
                return response.StatusCode.ToString();
            }
            catch (WebException we)
            {
                var response = we.Response as HttpWebResponse;
                if (response == null)
                {
                    OfflineHandle(payload);
                    return noResponseMessage;
                }
                response.Close();
                return response.StatusCode.ToString();
            }
        }

        private string HttpGet(Dictionary<string, string> payload, string collectorUri)
        {
            logger.Info(String.Format("Sending GET request to {0}", collectorUri));
            logger.Debug(() => String.Format("Payload: {0}", jss.Serialize(payload)));
            string destination = collectorUri + ToQueryString(payload);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destination);
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();
                return response.StatusCode.ToString();
            }
            catch (WebException we)
            {
                var response = (HttpWebResponse)we.Response;
                if (response == null)
                {
                    OfflineHandle(payload);
                    return noResponseMessage;
                }
                response.Close();
                return response.StatusCode.ToString();
            }
            
        }

        public void OfflineHandle(Dictionary<string, string> evt)
        {
            if (offlineModeEnabled)
            {
                logger.Info("Could not connect to server, queueing event for later");
                backupEmitter.Input(evt);
            }
        }

        public void OfflineHandle(Dictionary<string, object> payload)
        {
            foreach (Dictionary<string, string> evt in (List<Dictionary<string, string>>)payload["data"])
            {
                OfflineHandle(evt);
            }
        }

        private void ResendRequests()
        {
            if (offlineModeEnabled)
            {
                var allSent = false;
                var messageEnumerator = backupEmitter.Queue.GetMessageEnumerator2();

                // Stop removing messages when there are none left to remove
                // or the buffer is full (as the buffer will then be flushed,
                // causing another call to ResendRequests)
                while (!allSent && (buffer.Count < bufferSize))
                {
                    allSent = true;

                    // The call to RemoveCurrent halts the MessageEnumerator2,
                    // so this loop only removes a single message
                    while (messageEnumerator.MoveNext())
                    {
                        allSent = false;
                        System.Messaging.Message evt = messageEnumerator.RemoveCurrent();
                        Input(jss.Deserialize<Dictionary<string, string>>(evt.Body.ToString()));
                    }
                }
            }
        }

        private void NetworkAvailabilityChange (object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                logger.Info("Network availability change detected, attempting to send stored requests");
                ResendRequests();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    WeakEventManager<NetworkChange, NetworkAvailabilityEventArgs>.RemoveHandler(null, "NetworkAvailabilityChanged", NetworkAvailabilityChange);
                    backupEmitter.Dispose();
                }
                disposed = true;
            }
        }
    }
}
