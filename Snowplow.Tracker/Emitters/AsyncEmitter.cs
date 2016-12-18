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
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Emitters.Endpoints;

namespace Snowplow.Tracker
{
    public class AsyncEmitter : IEmitter
    {
        private readonly object _startStopLock = new object();
        private Thread _runner;
        private bool _keepRunning = false;
        private IPersistentBlockingQueue _queue;
        private IEndpoint _endpoint;
        private int _stopPollIntervalMs;

        public bool Running
        {
            get
            {
                lock (_startStopLock)
                {
                    return _runner != null;
                }
            }
        }

        public AsyncEmitter(IEndpoint endpoint,
                            IPersistentBlockingQueue queue,
                            int stopPollIntervalMs = 300) 
        {
            _queue = queue;
            _endpoint = endpoint;
            _stopPollIntervalMs = stopPollIntervalMs;
        }

        public void Start()
        {
            lock (_startStopLock)
            {
                if (_runner == null)
                {
                    _keepRunning = true;
                    _runner = new Thread(new ThreadStart(this.loop));
                    _runner.Start();
                } else
                {
                    throw new InvalidOperationException("Cannot start - already started");
                }
            }
        }

        private void loop()
        {
            for (;;)
            {
                lock(_startStopLock)
                {
                    if (_keepRunning)
                    {
                        // take item(s) off the queue and send it
                        var items = _queue.Dequeue(_stopPollIntervalMs);
                        foreach (var item in items)
                        {
                            _endpoint.Send(item);
                        }

                    } else
                    {
                        break;
                    }
                }
            }
        }

        public void Stop()
        {
           lock (_startStopLock)
            {
                if (_runner == null)
                {
                    throw new InvalidOperationException("Cannot stop - not started");
                } else
                {
                    _keepRunning = false;
                }
            }

            _runner.Join();

            lock(_startStopLock) { 
                _runner = null;
            }
        }

    

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void Input(Dictionary<string, string> payload)
        {
            throw new NotImplementedException();
        }
    }
}
