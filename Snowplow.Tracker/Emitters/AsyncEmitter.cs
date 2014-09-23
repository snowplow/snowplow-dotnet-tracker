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

        public AsyncEmitter(string endpoint, HttpProtocol protocol = HttpProtocol.HTTP, int? port = null, HttpMethod method = HttpMethod.GET, int? bufferSize = null, Action<int> onSuccess = null, Action<int, List<Dictionary<string, string>>> onFailure = null, bool offlineModeEnabled = true) :
            base(endpoint, protocol, port, method, bufferSize, onSuccess, onFailure, offlineModeEnabled) { tasks = new List<Task>(); }

        public override void Flush(bool sync = false)
        {
            Task flushingTask = Task.Factory.StartNew(SendRequests);
            tasks.Add(flushingTask);
            tasks = tasks.Where(t => !t.IsCompleted).ToList();
            if (sync)
            {
                logger.Info("Starting synchronous flush");
                Task.WaitAll(tasks.ToArray(), 10000);
            }
            else
            {
                Thread.Yield();
            }
        }

    }
}
