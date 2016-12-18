using Snowplow.Tracker.Emitters.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Snowplow.Tracker.Emitters.Endpoints
{

    public class SnowplowHttpCollectorEndpoint : IEndpoint
    {
        public delegate int? PostDelegate(string uri, string postData);
        public delegate int? GetDelegate(string uri);

        private readonly PostDelegate DefaultPostMethod = new PostDelegate(SnowplowHttpCollectorEndpoint.HttpPost);
        private readonly GetDelegate DefaultGetMethod = new GetDelegate(SnowplowHttpCollectorEndpoint.HttpGet);


        private readonly string _collectorUri;
        private readonly HttpMethod _method;

        private GetDelegate _getMethod;
        private PostDelegate _postMethod;

        public SnowplowHttpCollectorEndpoint(string host,
                                             HttpProtocol protocol = HttpProtocol.HTTP,
                                             int? port = null,
                                             HttpMethod method = HttpMethod.GET,
                                             PostDelegate postMethod = null,
                                             GetDelegate getMethod = null)
        {

            if (Uri.IsWellFormedUriString(host, UriKind.Absolute)) { 
                var uri = new Uri(host);
                var endpointWithoutScheme = uri.Host;
                _collectorUri = getCollectorUri(endpointWithoutScheme, protocol, port, method);
            } else
            {
                _collectorUri = getCollectorUri(host, protocol, port, method);
            }
            
            _method = method;
            _postMethod = postMethod ?? DefaultPostMethod;
            _getMethod = getMethod ?? DefaultGetMethod;
        }

        public bool Send(Payload p)
        {
            if (_method == HttpMethod.GET)
            {
                return isGoodResponse(_getMethod(_collectorUri + ToQueryString(p.NvPairs)));
            } else if ( _method == HttpMethod.POST)
            {
                var data = new Dictionary<string, object>()
                {
                    { "schema", "iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-0" },
                    { "data", new List<object> {  p.NvPairs } }
                };
                return isGoodResponse(_postMethod(_collectorUri, JsonConvert.SerializeObject(data)));
            } else
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
                return false;
            }
        }

        private static string getCollectorUri(string endpoint, HttpProtocol protocol, int? port, HttpMethod method)
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
        /// Converts an event from a dictionary to a querystring
        /// </summary>
        /// <param name="payload">The event to convert</param>
        /// <returns>Querystring of the form "?e=pv&tna=cf&..."</returns>
        private static string ToQueryString(Dictionary<string, string> payload)
        {
            var array = (from key in payload.Keys
                         select string.Format("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode(payload[key])))
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
        public static int? HttpPost(string collectorUri, string postData)
        {
            //Log.Logger.Info(String.Format("Sending POST request to {0}", collectorUri));
            //Log.Logger.Debug(() => String.Format("Payload: {0}", jss.Serialize(payload)));

            try
            {
                using (HttpClient c = new HttpClient())
                {
                    var postContent = new StringContent(postData, Encoding.UTF8, "application/json");
                    var response = c.PostAsync(collectorUri, postContent).Result;
                    return (int)response.StatusCode;
                }

            } catch (Exception e)
            {
                // logger.log e
                return null;
            }
        }

        /// <summary>
        /// Make a GET request to a collector
        /// </summary>
        /// <param name="payload">The event to be sent</param>
        /// <param name="collectorUri">The collector URI</param>
        /// <returns>String representing the status of the request, e.g. "OK" or "Forbidden"</returns>
        public static int? HttpGet(string uri)
        {
            //Log.Logger.Info(String.Format("Sending GET request to {0}", collectorUri));
            //Log.Logger.Debug(() => String.Format("Payload: {0}", jss.Serialize(payload)));
            
            try
            {
                using (HttpClient c = new HttpClient())
                {
                    var result = c.GetAsync(uri).Result;
                    return (int)result.StatusCode;
                }                    
            } catch (Exception e)
            {
                // log e
                return null;
            }

        }
    }
}
