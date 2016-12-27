/*
 * SnowplowHttpCollectorEndpoint.cs
 * 
 * Copyright (c) 2014-2016 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Ed Lewis, Joshua Beemster
 * Copyright: Copyright (c) 2014-2016 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Models;

namespace Snowplow.Tracker.Endpoints
{
    public class SnowplowHttpCollectorEndpoint : IEndpoint
    {
        public delegate int? PostDelegate(string uri, string postData);
        public delegate int? GetDelegate(string uri);

        private readonly PostDelegate DefaultPostMethod = new PostDelegate(SnowplowHttpCollectorEndpoint.HttpPost);
        private readonly GetDelegate DefaultGetMethod = new GetDelegate(SnowplowHttpCollectorEndpoint.HttpGet);


        private readonly string _collectorUri;
        private readonly Snowplow.Tracker.Endpoints.HttpMethod _method;

        private GetDelegate _getMethod;
        private PostDelegate _postMethod;

        private ILogger _logger;


        /// <summary>
        /// Create a connection to a Snowplow collector
        /// <param name="host">The hostname of the collector (not including the scheme, e.g. http://)</param>
        /// <param name="protocol">The protocol to use. HTTP or HTTPS</param>
        /// <param name="port">The port number the collector is listening on</param>
        /// <param name="method">The request method to use. GET or POST</param>
        /// <param name="postMethod">Internal use</param>
        /// <param name="getMethod">Internal use</param>
        /// <param name="l">Send log messages using this logger</param>
        /// </summary>
        public SnowplowHttpCollectorEndpoint(string host,
                                             HttpProtocol protocol = HttpProtocol.HTTP,
                                             int? port = null,
                                             Snowplow.Tracker.Endpoints.HttpMethod method = Snowplow.Tracker.Endpoints.HttpMethod.GET,
                                             PostDelegate postMethod = null,
                                             GetDelegate getMethod = null,
                                             ILogger l = null)
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
            _logger = l ?? new NoLogging();
        }

        /// <summary>
        /// Send a request to the endpoint using the current settings
        /// </summary>
        /// <param name="p">The paylaod to send</param>
        /// <returns>true if successful (200), otherwise false</returns>
        public bool Send(Payload p)
        {
            if (_method == Snowplow.Tracker.Endpoints.HttpMethod.GET)
            {
                var uri = _collectorUri + ToQueryString(p.Payload);
                _logger.Info(String.Format("Endpoint GET {0}", uri));
                var response = _getMethod(uri);
                var message = (response.HasValue) ? response.Value.ToString() : "(timed out)";
                _logger.Info(String.Format("Endpoint GET {0} responded with {1}", uri, message));
                return isGoodResponse(response);
            }
            else if (_method == Snowplow.Tracker.Endpoints.HttpMethod.POST)
            {
                var data = new Dictionary<string, object>()
                {
                    { Constants.SCHEMA, Constants.SCHEMA_PAYLOAD_DATA },
                    { Constants.DATA, new List<object> {  p.Payload } }
                };

                _logger.Info(String.Format("Endpoint POST {0}", _collectorUri));
                var response = _postMethod(_collectorUri, JsonConvert.SerializeObject(data));
                var message = (response.HasValue) ? response.Value.ToString() : "(timed out)";
                _logger.Info(String.Format("Endpoint POST {0} responded with {1}", _collectorUri, message));
                return isGoodResponse(response);
            }
            else
            {
                throw new NotSupportedException("Only post and get supported");
            }

        }

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

        private static string getCollectorUri(string endpoint, HttpProtocol protocol, int? port, Snowplow.Tracker.Endpoints.HttpMethod method)
        {
            string path;
            string requestProtocol = (protocol == HttpProtocol.HTTP) ? "http" : "https";
            if (method == Snowplow.Tracker.Endpoints.HttpMethod.GET)
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


        /// <summary>
        /// Converts an event from a dictionary to a querystring
        /// </summary>
        /// <param name="payload">The event to convert</param>
        /// <returns>Querystring of the form "?e=pv&tna=cf&..."</returns>
        private static string ToQueryString(Dictionary<string, object> payload)
        {
            var array = (from key in payload.Keys
                         select string.Format("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode((string)payload[key])))
                .ToArray();
            return String.Format("?{0}", String.Join("&", array));
        }


        /// <summary>
        /// Make a POST request with the given data. Content type application/json
        /// </summary>
        /// <param name="collectorUri">The URI to POST to</param>
        /// <param name="postData">JSON string of POST data</param>
        /// <returns>The HTTP return code, or null if couldn't connect</returns>
        public static int? HttpPost(string collectorUri, string postData)
        {
            try
            {
                using (HttpClient c = new HttpClient())
                {
                    var postContent = new StringContent(postData, Encoding.UTF8, Constants.POST_CONTENT_TYPE);
                    var response = c.PostAsync(collectorUri, postContent).Result;
                    return (int)response.StatusCode;
                }

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Make a GET request
        /// </summary>
        /// <param name="uri">The URI to GET</param>
        /// <returns>The HTTP return code, or null if couldn't connect</returns>
        public static int? HttpGet(string uri)
        {
            try
            {
                using (HttpClient c = new HttpClient())
                {
                    var result = c.GetAsync(uri).Result;
                    return (int)result.StatusCode;
                }
            }
            catch
            {
                return null;
            }

        }
    }
}
