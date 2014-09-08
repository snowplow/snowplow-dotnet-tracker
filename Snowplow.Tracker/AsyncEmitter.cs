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
        private List<Thread> threads;

        public AsyncEmitter(string endpoint, string protocol = "http", int? port = null, string method = "get", int? bufferSize = null):
            base(endpoint, protocol, port, method, bufferSize) { threads = new List<Thread>(); }

        public override void flush(bool sync = false)
        {
            Thread flushingThread = new Thread(new ThreadStart(this.sendRequests));
            flushingThread.Start();
            threads = threads.Where(t => t.IsAlive).ToList();
            threads.Add(flushingThread);
            Thread.Yield();

            if (sync)
            {
                foreach (Thread thread in threads)
                {
                    thread.Join(TimeSpan.FromSeconds(1));
                    thread.Abort();
                }
            }
        }
    }
}
