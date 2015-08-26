/*
 * AsyncEmitter.cs
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
using System.Threading;

namespace Snowplow.Tracker
{
    public class AsyncEmitter : Emitter
    {
        private List<Task> tasks;

        /// <summary>
        /// Asynchronous emitter to send non-blocking HTTP requests
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
        public AsyncEmitter(string endpoint, HttpProtocol protocol = HttpProtocol.HTTP, int? port = null, HttpMethod method = HttpMethod.GET, int? bufferSize = null, Action<int> onSuccess = null, Action<int, List<Dictionary<string, string>>> onFailure = null, bool offlineModeEnabled = true) :
            base(endpoint, protocol, port, method, bufferSize, onSuccess, onFailure, offlineModeEnabled) { tasks = new List<Task>(); }

        private readonly object Locker = new object();

        /// <summary>
        /// Create a new Task to send all requests in the buffer
        /// </summary>
        /// <param name="sync">If set to true, flush synchronously</param>
        /// <param name="forced">If set to true, flush no matter how many events are in the buffer</param>
        protected override void Flush(bool sync, bool forced)
        {
            lock (Locker)
            {
                Task flushingTask = Task.Factory.StartNew(() =>
                {
                    if (forced || this.buffer.Count >= this.bufferSize)
                    {
                        SendRequests();
                    }
                });
                tasks.Add(flushingTask);
                tasks = tasks.Where(t => !t.IsCompleted).ToList();

                if (sync)
                {
                    Log.Logger.Info("Starting synchronous flush");
                    Task.WaitAll(tasks.ToArray(), 10000);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

    }
}
