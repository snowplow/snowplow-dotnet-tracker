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
using System.Timers;
using System.Net;
using System.Net.NetworkInformation;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows;
using System.ServiceProcess;

namespace Snowplow.Tracker
{
    public class Emitter : IEmitter, IDisposable
    {
        private string collectorUri;
        private HttpMethod method;
        protected int bufferSize;
        volatile protected List<Dictionary<string, string>> buffer;
        private Action<int> onSuccess;
        private Action<int, List<Dictionary<string, string>>> onFailure = null;
        private static bool? offlineModePossible; // Whether MSMQ is available
        private bool offlineTrackingConfigured = false;
        private bool offlineModeEnabled;
        private MsmqEmitter backupEmitter;
        private Timer flushTimer;
        private bool disposed = false;
        private static JavaScriptSerializer jss = new JavaScriptSerializer();
        private static String noResponseMessage = "Unable to contact server";

        public bool OfflineModeEnabled
        {
            get { return offlineModeEnabled; }
            set
            {
                if (value)
                {
                    if (offlineModePossible == null)
                    {
                        var services = ServiceController.GetServices().ToList();
                        var msQue = services.Find(x => x.ServiceName == "MSMQ");
                        offlineModePossible = (msQue != null && msQue.Status == ServiceControllerStatus.Running);
                    }

                    if ((bool)offlineModePossible)
                    {
                        configureOfflineTracking();
                        offlineModeEnabled = true;
                    }
                    else if (! (bool)offlineModePossible)
                    {
                        Log.Logger.Warn("Offline mode cannot be enabled because MSMQ is not available");
                        offlineModeEnabled = false;
                    }
                }
                else
                {
                    offlineModeEnabled = false;
                }
            }
        }

        /// <summary>
        /// Basic emitter to send synchronous HTTP requests
        /// </summary>
        /// <param name="endpoint">Collector domain</param>
        /// <param name="protocol">HttpProtocol.HTTP or HttpProtocol.HTTPS</param>
        /// <param name="port">Port to connect to</param>
        /// <param name="method">HttpMethod.GET or HttpMethod.POST</param>
        /// <param name="bufferSize">Maximum number of events queued before the buffer is flushed automatically.
        /// Defaults to 10 for POST requests and 1 for GET requests.</param>
        /// <param name="onSuccess">Callback executed when every request in a flush has status code 200.
        /// Gets passed the number of events flushed.</param>
        /// <param name="onFailure">Callback executed when not every request in a flush has status code 200.
        /// Gets passed the number of events sent successfully and a list of unsuccessful events.</param>
        /// <param name="offlineModeEnabled">Whether to store unsent requests using MSMQ</param>
        public Emitter(string endpoint, HttpProtocol protocol = HttpProtocol.HTTP, int? port = null, HttpMethod method = HttpMethod.GET, int? bufferSize = null, Action<int> onSuccess = null, Action<int, List<Dictionary<string, string>>> onFailure = null, bool offlineModeEnabled = true)
        {
            collectorUri = GetCollectorUri(endpoint, protocol, port, method);
            this.method = method;
            this.buffer = new List<Dictionary<string, string>>();
            this.bufferSize = Math.Max(1, bufferSize ?? (method == HttpMethod.GET ? 1 : 10));
            this.onSuccess = onSuccess;
            this.onFailure = onFailure;
            this.OfflineModeEnabled = offlineModeEnabled;
            Log.Logger.Info(String.Format("{0} initialized with endpoint {1}", this.GetType(), collectorUri));
        }

        private void configureOfflineTracking()
        {
            if (! offlineTrackingConfigured)
            {
                backupEmitter = new MsmqEmitter(String.Format(".\\private$\\{0}", collectorUri));
                WeakEventManager<NetworkChange, NetworkAvailabilityEventArgs>.AddHandler(null, "NetworkAvailabilityChanged", NetworkAvailabilityChange);
            }
            offlineTrackingConfigured = true;
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

        /// <summary>
        /// Add an event to the buffer and flush if the buffer is full
        /// </summary>
        /// <param name="payload">The event to add</param>
        public void Input(Dictionary<string, string> payload)
        {
            buffer.Add(payload);
            if (buffer.Count >= bufferSize)
            {
                Flush(false, false);
            }
        }

        /// <summary>
        /// Send all events in the buffer
        /// </summary>
        /// <param name="sync">Only relevant in the case of the AsyncEmitter</param>
        public void Flush(bool sync = false)
        {
            Flush(sync, true);
        }

        /// <summary>
        /// Send all events in the buffer
        /// Exists to prevent AsyncEmitter from flushing the buffer when it isn't full
        /// </summary>
        /// <param name="sync">Only relevant in the case of the AsyncEmitter</param>
        /// <param name="forced">Only relevant in the case of the AsyncEmitter</param>
        virtual protected void Flush(bool sync, bool forced)
        {
            SendRequests();
        }

        /// <summary>
        /// Send all requests in the buffer
        /// </summary>
        protected void SendRequests()
        {
            if (buffer.Count == 0)
            {
                Log.Logger.Info("Buffer empty, returning");
                return;
            }

            // Move all requests from buffer into a tempBuffer for thread safety
            var tempBuffer = new List<Dictionary<string, string>>();
            while (buffer.Count > 0)
            {
                tempBuffer.Add(buffer[0]);
                buffer.RemoveAt(0);
            }

            Log.Logger.Info(String.Format("Attempting to send {0} event{1}", tempBuffer.Count, tempBuffer.Count == 1 ? "" : "s"));
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
                    Log.Logger.Info(String.Format("POST request to {0} finished with status '{1}'", collectorUri, statusCode));
                    if (onSuccess != null)
                    {
                        onSuccess(tempBuffer.Count);
                    }
                    ResendRequests();
                }
                else
                {
                    OfflineHandle(data);

                    Log.Logger.Warn(String.Format("POST request to {0} finished with status: '{1}'. Sent to backup emitter.", collectorUri, statusCode));
                    
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
                        Log.Logger.Info(String.Format("GET request to {0} finished with status: '{1}'", collectorUri, statusCode));
                        successCount += 1;
                        ResendRequests();
                    }
                    else
                    {
                        OfflineHandle(payload);
                        Log.Logger.Warn(String.Format("GET request to {0} finished with status: '{1}'. Sent to backup emitter.", collectorUri, statusCode));
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

        /// <summary>
        /// Converts an event from a dictionary to a querystring
        /// </summary>
        /// <param name="payload">The event to convert</param>
        /// <returns>Querystring of the form "?e=pv&tna=cf&..."</returns>
        private static string ToQueryString(Dictionary<string, string> payload)
        {
            var array = (from key in payload.Keys
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(payload[key])))
                .ToArray();
            return String.Format("?{0}", String.Join("&", array));
        }

        /// <summary>
        /// Make a POST request to a collector
        /// See http://stackoverflow.com/questions/9145667/how-to-post-json-to-the-server
        /// </summary>
        /// <param name="payload">The body of the request</param>
        /// <param name="collectorUri">The collector URI</param>
        /// <returns>String representing the status of the request, e.g. "OK" or "Forbidden"</returns>
        private string HttpPost(Dictionary<string, object> payload, string collectorUri)
        {
            Log.Logger.Info(String.Format("Sending POST request to {0}", collectorUri));
            Log.Logger.Debug(() => String.Format("Payload: {0}", jss.Serialize(payload)));
            string destination = collectorUri;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destination);
            request.Timeout = 10000;
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

        /// <summary>
        /// Make a GET request to a collector
        /// </summary>
        /// <param name="payload">The event to be sent</param>
        /// <param name="collectorUri">The collector URI</param>
        /// <returns>String representing the status of the request, e.g. "OK" or "Forbidden"</returns>
        private string HttpGet(Dictionary<string, string> payload, string collectorUri)
        {
            Log.Logger.Info(String.Format("Sending GET request to {0}", collectorUri));
            Log.Logger.Debug(() => String.Format("Payload: {0}", jss.Serialize(payload)));
            string destination = collectorUri + ToQueryString(payload);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destination);
            request.Timeout = 10000;
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
                    return noResponseMessage;
                }
                response.Close();
                return response.StatusCode.ToString();
            }
            
        }

        /// <summary>
        /// If offline mode is enabled, store unsent requests using MSMQ
        /// </summary>
        /// <param name="evt">The event to store</param>
        public void OfflineHandle(Dictionary<string, string> evt)
        {
            if (offlineModeEnabled)
            {
                Log.Logger.Info("Could not connect to server, queueing event for later");
                backupEmitter.Input(evt);
            }
        }

        /// <summary>
        /// Call offlineHandle on all events in a failed POST request
        /// </summary>
        /// <param name="payload">The body of the POST request</param>
        public void OfflineHandle(Dictionary<string, object> payload)
        {
            foreach (Dictionary<string, string> evt in (List<Dictionary<string, string>>)payload["data"])
            {
                OfflineHandle(evt);
            }
        }

        /// <summary>
        /// Attempt to resend events stored in a Message Queue
        /// </summary>
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

        /// <summary>
        /// Resend requests if the network becomes available
        /// </summary>
        /// <param name="sender">Unused parameter</param>
        /// <param name="e">Unused parameter</param>
        private void NetworkAvailabilityChange (object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Log.Logger.Info("Network availability change detected, attempting to send stored requests");
                ResendRequests();
            }
        }

        /// <summary>
        /// Periodically flush the buffer at a chosen interval
        /// </summary>
        /// <param name="timeout">The interval in milliseconds</param>
        public void SetFlushTimer(int timeout = 10000)
        {
            if (flushTimer == null)
            {
                flushTimer = new Timer();
                flushTimer.Elapsed += (source, elapsedEventArgs) =>
                {
                    Log.Logger.Info("flushTimer elapsed, flushing buffer");
                    Flush();
                };
            }
            flushTimer.Enabled = true;
            flushTimer.Interval = timeout;
        }

        /// <summary>
        /// Stop periodically flushing the buffer
        /// </summary>
        public void DisableFlushTimer()
        {
            if (flushTimer != null)
            {
                flushTimer.Enabled = false;
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
                    if (flushTimer != null)
                    {
                        flushTimer.Dispose();
                    }
                }
                disposed = true;
            }
        }
    }
}
