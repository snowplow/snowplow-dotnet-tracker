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
using System.Diagnostics;

namespace Snowplow.Tracker
{
    public class AsyncEmitter : IEmitter, IDisposable
    {
        private readonly object _startStopLock = new object();
        private readonly object _backOffLock = new object();
        private bool _denyBackOff = false;

        private Thread _runner;
        private bool _keepRunning = false;
        private IPersistentBlockingQueue _queue;
        private IEndpoint _endpoint;
        private int _stopPollIntervalMs;

        private readonly int _backOffIntervalMinMs = 5000;
        private readonly int _backOffIntervalMaxMs = 30000;

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
                    _runner = new Thread(new ThreadStart(this.loop)) { IsBackground = false };
                    _runner.Start();
                }
                else
                {
                    throw new InvalidOperationException("Cannot start - already started");
                }
            }
        }

        private void loop()
        {
            for (;;)
            {
                var run = false;
                lock (_startStopLock)
                {
                    run = _keepRunning;
                }

                if (run)
                {
                    // take item(s) off the queue and send it
                    var items = _queue.Dequeue(_stopPollIntervalMs);
                    foreach (var item in items)
                    {
                        if (!_endpoint.Send(item))
                        {
                            _queue.Enqueue(new List<Payload>() { item });
                            // slow down - back off 30 secs
                            // NB this can be interrupted by Flush
                            lock (_backOffLock)
                            {
                                if (!_denyBackOff)
                                {
                                    var interval = new Random().Next(_backOffIntervalMinMs, _backOffIntervalMaxMs);
                                    Monitor.Wait(_backOffLock, interval);
                                }
                            }
                        }
                    }

                }
                else
                {
                    break;
                }

            }
        }

        public void Stop()
        {
            lock (_startStopLock)
            {
                if (_runner == null)
                {
                    return;
                }
                else
                {
                    _keepRunning = false;
                }
            }

            lock (_backOffLock)
            {
                _denyBackOff = true;
                Monitor.Pulse(_backOffLock);
            }
            
            _runner.Join();

            lock (_startStopLock)
            {
                _runner = null;
                _denyBackOff = false;
            }
        }

        public void Flush()
        {
            Flush(false);
        }

        public void Flush(bool disableRestart = false)
        {
            Stop();

            var items = new List<Payload>();
            List<Payload> newItems; 

            while ((newItems = _queue.Dequeue(0)).Count > 0)
            {
                items.AddRange(newItems);
            }

            var failed = new List<Payload>();
            bool stopSending = false;

            foreach (var item in items)
            {
                if (stopSending || !_endpoint.Send(item))
                {
                    failed.Add(item);
                    stopSending = true;
                }
            }

            _queue.Enqueue(failed);

            if (!disableRestart)
            {
                Start();
            }
        }

        public void Input(Payload payload)
        {
            _queue.Enqueue(new List<Payload>() { payload });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }

        public void Close()
        {
            Dispose();
        }

        ~AsyncEmitter()
        {
            Close();
        }
    }
}
