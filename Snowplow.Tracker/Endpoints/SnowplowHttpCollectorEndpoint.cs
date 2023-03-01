/*
 * Copyright (c) 2023 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Models;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Endpoints
{
    public class SnowplowHttpCollectorEndpoint : IEndpoint
    {
        public delegate RequestResult PostDelegate(string uri, string postData, bool oversize, List<string> itemIds);
        public delegate RequestResult GetDelegate(string uri, bool oversize, List<string> itemIds);

        private readonly PostDelegate DefaultPostMethod = new PostDelegate(SnowplowHttpCollectorEndpoint.HttpPost);
        private readonly GetDelegate DefaultGetMethod = new GetDelegate(SnowplowHttpCollectorEndpoint.HttpGet);

        private readonly int POST_WRAPPER_BYTES = 88; // "schema":"iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-4","data":[]
        private readonly int POST_STM_BYTES = 22;     // "stm":"1443452851000",

        private readonly string _collectorUri;
        private readonly HttpMethod _method;
        private GetDelegate _getMethod;
        private PostDelegate _postMethod;
        private int _byteLimitGet;
        private int _byteLimitPost;
        private ILogger _logger;

        /// <summary>
        /// Create a connection to a Snowplow collector
        /// <param name="host">The hostname of the collector (not including the scheme, e.g. http://)</param>
        /// <param name="protocol">The protocol to use. HTTP or HTTPS</param>
        /// <param name="port">The port number the collector is listening on</param>
        /// <param name="method">The request method to use. GET or POST</param>
        /// <param name="postMethod">Internal use</param>
        /// <param name="getMethod">Internal use</param>
        /// <param name="byteLimitPost">The maximum amount of bytes we can expect to work</param>
        /// <param name="byteLimitGet">The maximum amount of bytes we can expect to work</param>
        /// <param name="l">Send log messages using this logger</param>
        /// </summary>
        public SnowplowHttpCollectorEndpoint(string host, HttpProtocol protocol = HttpProtocol.HTTP, int? port = null, HttpMethod method = HttpMethod.GET, 
            PostDelegate postMethod = null, GetDelegate getMethod = null, int byteLimitPost = 40000, int byteLimitGet = 40000, ILogger l = null)
        {
            if (Uri.IsWellFormedUriString(host, UriKind.Absolute))
            {
                var uri = new Uri(host);
                var endpointWithoutScheme = uri.Host;
                _collectorUri = getCollectorUri(endpointWithoutScheme, protocol, port, method);
            }
            else
            {
                _collectorUri = getCollectorUri(host, protocol, port, method);
            }

            _method = method;
            _postMethod = postMethod ?? DefaultPostMethod;
            _getMethod = getMethod ?? DefaultGetMethod;
            _byteLimitPost = byteLimitPost;
            _byteLimitGet = byteLimitGet;
            _logger = l ?? new NoLogging();
        }

        /// <summary>
        /// Send a request to the endpoint using the current settings
        /// </summary>
        /// <param name="p">The paylaod to send</param>
        /// <returns>a list of successfully sent and failed events, success is determined by a 200 response code</returns>
        public SendResult Send(List<Tuple<string, Payload>> itemList)
        {
            List<RequestResult> requestResultList;

            if (_method == HttpMethod.GET)
            {
                requestResultList = SendGetAsync(itemList);
            }
            else if (_method == HttpMethod.POST)
            {
                requestResultList = SendPostAsync(itemList);
            }
            else
            {
                throw new NotSupportedException("Only POST and GET supported");
            }

            var successIds = new List<string>();
            var failureIds = new List<string>();

            foreach (var requestResult in requestResultList)
            {
                int statusCode;
                try
                {
                    statusCode = requestResult.StatusCodeTask.Result;
                }
                catch
                {
                    statusCode = -1;
                }

                if (requestResult.IsOversize)
                {
                    successIds.AddRange(requestResult.ItemIds);
                }
                else if (isGoodResponse(statusCode))
                {
                    successIds.AddRange(requestResult.ItemIds);
                }
                else
                {
                    failureIds.AddRange(requestResult.ItemIds);
                }
            }

            return new SendResult()
            {
                SuccessIds = successIds,
                FailureIds = failureIds
            };
        }

        /// <summary>
        /// Sends each item as an individual GET request
        /// 
        /// Note: If an individual event is greater than the byteLimit we assume success
        /// </summary>
        /// <param name="itemList">The list of items to send</param>
        /// <returns>The list of send results</returns>
        private List<RequestResult> SendGetAsync(List<Tuple<string, Payload>> itemList)
        {
            var requestResultList = new List<RequestResult>();

            foreach (var item in itemList)
            {
                var payload = item.Item2;
                payload.Add(Constants.SENT_TIMESTAMP, Utils.GetTimestamp().ToString());

                var uri = _collectorUri + ToQueryString(payload.Payload);
                var byteSize = Utils.GetUTF8Length(uri);

                _logger.Info(String.Format("Endpoint GET {0}", uri));

                requestResultList.Add(_getMethod(uri, byteSize > _byteLimitGet, new List<string> { item.Item1 }));
            }

            return requestResultList;
        }

        /// <summary>
        /// Batches and sends POST requests containing many events
        /// 
        /// Note: If an individual event is greater than the byteLimit we assume success
        /// </summary>
        /// <param name="itemList">The list of items to send</param>
        /// <returns>The list of send results</returns>
        private List<RequestResult> SendPostAsync(List<Tuple<string, Payload>> itemList)
        {
            var requestResultList = new List<RequestResult>();

            var itemIds = new List<string>();
            var itemPayloads = new List<Dictionary<string, object>>();
            long totalByteSize = 0;

            foreach (var item in itemList)
            {
                var payload = item.Item2;
                var byteSize = payload.GetByteSize() + POST_STM_BYTES;

                if ((byteSize + POST_WRAPPER_BYTES) > _byteLimitPost)
                {
                    var singleEventPost = AddSendTimestamp(new List<Dictionary<string, object>> { payload.Payload });
                    var singleEventIds = new List<string> { item.Item1 };
                    requestResultList.Add(_postMethod(_collectorUri, ToPostDataString(singleEventPost), true, singleEventIds));
                }
                else if ((totalByteSize + byteSize + POST_WRAPPER_BYTES + (itemPayloads.Count - 1)) > _byteLimitPost)
                {
                    itemPayloads = AddSendTimestamp(itemPayloads);
                    requestResultList.Add(_postMethod(_collectorUri, ToPostDataString(itemPayloads), false, itemIds));

                    itemPayloads = new List<Dictionary<string, object>> { payload.Payload };
                    itemIds = new List<string> { item.Item1 };
                    totalByteSize = byteSize;
                }
                else
                {
                    itemPayloads.Add(payload.Payload);
                    itemIds.Add(item.Item1);
                    totalByteSize += byteSize;
                }
            }

            if (itemPayloads.Count > 0)
            {
                itemPayloads = AddSendTimestamp(itemPayloads);
                requestResultList.Add(_postMethod(_collectorUri, ToPostDataString(itemPayloads), false, itemIds));
            }

            return requestResultList;
        }

        // --- Helpers

        /// <summary>
        /// Checks whether the response is a 200
        /// </summary>
        /// <param name="response">The response to parse</param>
        /// <returns></returns>
        private bool isGoodResponse(int? response)
        {
            if (response != null)
            {
                return response == 200;
            }
            else
            {
                _logger.Warn("Endpoint returned non 200");
                return false;
            }
        }

        /// <summary>
        /// Builds and returns the collector URI string
        /// </summary>
        /// <param name="endpoint">The endpoint</param>
        /// <param name="protocol">The protocol</param>
        /// <param name="port">The port</param>
        /// <param name="method">The method to use</param>
        /// <returns></returns>
        private static string getCollectorUri(string endpoint, HttpProtocol protocol, int? port, Snowplow.Tracker.Endpoints.HttpMethod method)
        {
            string path;
            string requestProtocol = (protocol == HttpProtocol.HTTP) ? "http" : "https";
            if (method == HttpMethod.GET)
            {
                path = Constants.GET_URI_SUFFIX;
            }
            else
            {
                path = Constants.POST_URI_SUFFIX;
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

        // --- Event builders

        /// <summary>
        /// Appends the sent timestamp to a list of outbound events.
        /// </summary>
        /// <param name="payloadList">The list of events to append</param>
        /// <returns>The updated event list</returns>
        private List<Dictionary<string, object>> AddSendTimestamp(List<Dictionary<string, object>> payloadList)
        {
            var timestamp = Utils.GetTimestamp().ToString();
            var newPayloadList = new List<Dictionary<string, object>>();
            foreach(var payload in payloadList)
            {
                payload.Add(Constants.SENT_TIMESTAMP, timestamp);
                newPayloadList.Add(payload);
            }
            return newPayloadList;
        }

        /// <summary>
        /// Converts an event from a dictionary to a querystring
        /// </summary>
        /// <param name="payload">The event to convert</param>
        /// <returns>Querystring of the form "?e=pv&tna=cf&..."</returns>
        private string ToQueryString(Dictionary<string, object> payload)
        {
            var array = (from key in payload.Keys
                         select string.Format("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode((string)payload[key])))
                .ToArray();
            return String.Format("?{0}", String.Join("&", array));
        }

        /// <summary>
        /// Converts a list of events to a post-data string
        /// </summary>
        /// <param name="payloadList">The events to convert</param>
        /// <returns>PayloadData SelfDescribingJson string</returns>
        private string ToPostDataString(List<Dictionary<string, object>> payloadList)
        {
            return new SelfDescribingJson(Constants.SCHEMA_PAYLOAD_DATA, payloadList).ToString();
        }

        // --- Default Event Senders

        /// <summary>
        /// Make a POST request with the given data. Content type application/json
        /// </summary>
        /// <param name="collectorUri">The URI to POST to</param>
        /// <param name="postData">JSON string of POST data</param>
        /// <param name="oversize">If the request is oversized</param>
        /// <param name="itemIds">The ids of the events being sent</param>
        /// <returns>The HTTP return code, or null if couldn't connect</returns>
        public static RequestResult HttpPost(string collectorUri, string postData, bool oversize, List<string> itemIds)
        {
            var postContent = new StringContent(postData, Encoding.UTF8, Constants.POST_CONTENT_TYPE);
            var statusCodeTask = Task.Factory.StartNew(() => {
                using (HttpClient c = new HttpClient())
                {
                    return ProcessRequestTask(c.PostAsync(collectorUri, postContent));
                }
            });

            return new RequestResult()
            {
                IsOversize = oversize,
                StatusCodeTask = statusCodeTask,
                ItemIds = itemIds
            };
        }

        /// <summary>
        /// Make a GET request
        /// </summary>
        /// <param name="uri">The URI to GET</param>
        /// <param name="oversize">If the request is oversized</param>
        /// <param name="itemIds">The ids of the events being sent</param>
        /// <returns>The HTTP return code, or null if couldn't connect</returns>
        public static RequestResult HttpGet(string uri, bool oversize, List<string> itemIds)
        {
            var statusCodeTask = Task.Factory.StartNew(() => {
                using (HttpClient c = new HttpClient())
                {
                    return ProcessRequestTask(c.GetAsync(uri));
                } 
            });

            return new RequestResult()
            {
                IsOversize = oversize,
                StatusCodeTask = statusCodeTask,
                ItemIds = itemIds
            };
        }

        /// <summary>
        /// Wraps the request task and returns either the status code
        /// or -1 if it cannot get the code
        /// </summary>
        /// <param name="requestTask"></param>
        /// <returns></returns>
        private static int ProcessRequestTask(Task<HttpResponseMessage> requestTask)
        {
            try
            {
                return (int) requestTask.Result.StatusCode;
            }
            catch
            {
                return -1;
            }
        }
    }
}
