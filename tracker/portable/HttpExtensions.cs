using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Snowplow.Tracker
{
	public static class HttpExtensions
	{
		public static Task<Stream> GetRequestStreamAsync (this HttpWebRequest request, TimeSpan? timeout = null)
		{
			var taskSource = new TaskCompletionSource<Stream> ();
			request.BeginGetRequestStream (r => {

				Stream stream = null;
				Exception error = null;
				try
				{
					stream = request.EndGetRequestStream (r);
				}
				catch (Exception e)
				{
					error = e;
				}

				if (error == null)
				{
					taskSource.SetResult (stream);	
				}
				else
				{
					taskSource.SetException (error);
				}


			}, null);

			var task = taskSource.Task;
			return timeout.HasValue
					? task
					: task.WithTimeout (timeout.Value);
		}

		public static Task<WebResponse> GetResponseAsync (this HttpWebRequest request, TimeSpan? timeout = null)
		{
			var taskSource = new TaskCompletionSource<WebResponse> ();
			request.BeginGetResponse (r => {

				WebResponse response = null;
				Exception error = null;
				try
				{
					response = request.EndGetResponse (r);
				}
				catch (Exception e)
				{
					error = e;
				}

				if (error == null)
				{
					taskSource.SetResult (response);	
				}
				else
				{
					taskSource.SetException (error);
				}


			}, null);

			var task = taskSource.Task;
			return timeout.HasValue
					? task
					: task.WithTimeout (timeout.Value);
		}

		public async static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan duration)
		{
			var retTask = await Task.WhenAny(task, Task.Delay(duration)).ConfigureAwait(false);

			if (retTask is Task<T>) return task.Result;
			throw new WebException ("Timeout");
		}
	}
}

