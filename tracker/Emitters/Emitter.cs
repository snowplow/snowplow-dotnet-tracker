using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

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
		private static String noResponseMessage = "Unable to contact server";

		private readonly static TimeSpan Timeout = TimeSpan.FromSeconds (10);
		private HttpClient httpClient;

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
		public Emitter(string endpoint, HttpProtocol protocol = HttpProtocol.HTTP, int? port = null, HttpMethod method = HttpMethod.GET, int? bufferSize = null, Action<int> onSuccess = null, Action<int, List<Dictionary<string, string>>> onFailure = null, HttpClient httpClient = null)
		{
			collectorUri = GetCollectorUri(endpoint, protocol, port, method);
			this.method = method;
			this.buffer = new List<Dictionary<string, string>>();
			this.bufferSize = Math.Max(1, bufferSize ?? (method == HttpMethod.GET ? 1 : 10));
			this.onSuccess = onSuccess;
			this.onFailure = onFailure;

			this.httpClient = httpClient ?? new HttpClient ();
			this.httpClient.DefaultRequestHeaders.Add ("User-Agent", "System.Net.HttpWebRequest");
			this.httpClient.Timeout = Timeout;

			LogLoggerInfo(String.Format("{0} initialized with endpoint {1}", this.GetType(), collectorUri));
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
		virtual protected Task Flush(bool sync, bool forced)
		{
			return SendRequests();
		}

		/// <summary>
		/// Send all requests in the buffer
		/// </summary>
		protected async Task SendRequests()
		{
			if (buffer.Count == 0)
			{
				LogLoggerInfo("Buffer empty, returning");
				return;
			}

			// Move all requests from buffer into a tempBuffer for thread safety
			var tempBuffer = new List<Dictionary<string, string>>();
			while (buffer.Count > 0)
			{
				tempBuffer.Add(buffer[0]);
				buffer.RemoveAt(0);
			}

			LogLoggerInfo(String.Format("Attempting to send {0} event{1}", tempBuffer.Count, tempBuffer.Count == 1 ? "" : "s"));
			if (method == HttpMethod.POST)
			{
				var data = new Dictionary<string, object>
				{
					{ "schema", "iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-0" },
					{ "data", tempBuffer }
				};

				string statusCode = await HttpPost(data, collectorUri).ConfigureAwait (false);
				if (statusCode == "OK")
				{
					LogLoggerInfo(String.Format("POST request to {0} finished with status '{1}'", collectorUri, statusCode));
					if (onSuccess != null)
					{
						onSuccess(tempBuffer.Count);
					}
					ResendRequests();
				}
				else
				{
					LogLoggerWarn(String.Format("POST request to {0} finished with status: '{1}'", collectorUri, statusCode));
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
					string statusCode = await HttpGet(payload, collectorUri).ConfigureAwait (false);
					if (statusCode == "OK")
					{
						LogLoggerInfo(String.Format("GET request to {0} finished with status: '{1}'", collectorUri, statusCode));
						successCount += 1;
						ResendRequests();
					}
					else
					{
						LogLoggerWarn(String.Format("GET request to {0} finished with status: '{1}'", collectorUri, statusCode));
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
				select string.Format("{0}={1}", Uri.EscapeUriString(key), Uri.EscapeUriString(payload[key])))
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
		private async Task<string> HttpPost(Dictionary<string, object> payload, string collectorUri)
		{
			LogLoggerInfo(String.Format("Sending POST request to {0}", collectorUri));
			//LogLoggerDebug(() => String.Format("Payload: {0}", jss.Serialize(payload)));

			var content = new StringContent (JsonConvert.SerializeObject (payload), Encoding.UTF8, "application/json");
			try
			{
				var response = await httpClient.PostAsync (collectorUri, content).ConfigureAwait (false);
				return response.StatusCode.ToString ();
			}
			catch (WebException we)
			{
				var response = (HttpWebResponse)we.Response;
				if (response == null)
				{
					OfflineHandle(payload);
					return noResponseMessage;
				}
				return response.StatusCode.ToString();
			}
		}

		/// <summary>
		/// Make a GET request to a collector
		/// </summary>
		/// <param name="payload">The event to be sent</param>
		/// <param name="collectorUri">The collector URI</param>
		/// <returns>String representing the status of the request, e.g. "OK" or "Forbidden"</returns>
		private async Task<string> HttpGet(Dictionary<string, string> payload, string collectorUri)
		{
			LogLoggerInfo(String.Format("Sending GET request to {0}", collectorUri));
			//Log.Logger.Debug(() => String.Format("Payload: {0}", jss.Serialize(payload)));
			string destination = collectorUri + ToQueryString(payload);
			try
			{
				var response = await httpClient.GetAsync (destination).ConfigureAwait (false);
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
				return response.StatusCode.ToString();
			}

		}

		/// <summary>
		/// If offline mode is enabled, store unsent requests using MSMQ
		/// </summary>
		/// <param name="evt">The event to store</param>
		public void OfflineHandle(Dictionary<string, string> evt)
		{
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
		}

		/// <summary>
		/// Periodically flush the buffer at a chosen interval
		/// </summary>
		/// <param name="timeout">The interval in milliseconds</param>
		public void SetFlushTimer(int timeout = 10000)
		{
		}

		/// <summary>
		/// Stop periodically flushing the buffer
		/// </summary>
		public void DisableFlushTimer()
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		private void LogLoggerInfo (string message)
		{
		}

		private void LogLoggerWarn (string message)
		{
		}
	}
}

