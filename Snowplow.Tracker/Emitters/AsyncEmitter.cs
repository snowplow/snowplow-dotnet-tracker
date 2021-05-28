/*
 * AsyncEmitter.cs
 * 
 * Copyright (c) 2014-2017 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Fred Blundun, Ed Lewis, Joshua Beemster
 * Copyright: Copyright (c) 2014-2017 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Endpoints;
using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Models;

namespace Snowplow.Tracker.Emitters
{
    public class AsyncEmitter : IEmitter, IDisposable
    {
        public delegate void SendSuccessDelegate(int successCount, int failureCount);
        public delegate bool DeviceOnlineDelegate();

        private readonly object _startStopLock = new object();
        private readonly object _backOffLock = new object();
        private bool _denyBackOff = false;

        private Thread _runner;
        private bool _keepRunning = false;
        private IPersistentBlockingQueue _queue;
        private IEndpoint _endpoint;
        private int _sendLimit;
        private int _stopPollIntervalMs;
        private ILogger _logger;
        private SendSuccessDelegate _sendSuccessMethod;
        private DeviceOnlineDelegate _deviceOnlineMethod;

        private readonly int _backOffIntervalMinMs = 5000;
        private readonly int _backOffIntervalMaxMs = 30000;

        /// <summary>
        /// A flag indicating that the emitter is processing events
        /// </summary>
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

        /// <summary>
        /// Creates a new Async Emitter
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="queue"></param>
        /// <param name="sendLimit"></param>
        /// <param name="stopPollIntervalMs"></param>
        /// <param name="sendSuccessMethod"></param>
        /// <param name="deviceOnlineMethod"></param>
        /// <param name="l"></param>
        public AsyncEmitter(IEndpoint endpoint, IPersistentBlockingQueue queue, int sendLimit = 100, int stopPollIntervalMs = 300, 
            SendSuccessDelegate sendSuccessMethod = null, DeviceOnlineDelegate deviceOnlineMethod = null, ILogger l = null)
        {
            _queue = queue;
            _endpoint = endpoint;
            _sendLimit = sendLimit;
            _stopPollIntervalMs = stopPollIntervalMs;
            _sendSuccessMethod = sendSuccessMethod;
            _deviceOnlineMethod = deviceOnlineMethod;
            _logger = l ?? new NoLogging();
        }

        /// <summary>
        /// Start the emitter processing events (in another thread)
        /// </summary>
        public void Start()
        {
            lock (_startStopLock)
            {
                if (_runner == null)
                {
                    _keepRunning = true;
                    _runner = new Thread(new ThreadStart(this.loop)) { IsBackground = false };
                    _runner.Start();
                    _logger.Info("Emitter started - waiting for events");
                }
                else
                {
                    throw new InvalidOperationException("Cannot start - already started");
                }
            }
        }

        /// <summary>
        /// Sends event forever until stopped.
        /// </summary>
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
                    // If device is offline wait and loop
                    if (_deviceOnlineMethod != null && !_deviceOnlineMethod.Invoke())
                    {
                        EmitterLoopBackoff();
                    }
                    else
                    {
                        var eventList = _queue.Peek(_sendLimit, _stopPollIntervalMs);
                        _logger.Info(String.Format("Emitter found {0} events", eventList.Count));

                        var sendResult = _endpoint.Send(eventList);
                        _queue.Remove(sendResult.SuccessIds);
                        var successCount = sendResult.SuccessIds.Count;
                        var failureCount = sendResult.FailureIds.Count;

                        _logger.Info(String.Format("Emitter sent and deleted {0} events", successCount));
                        _logger.Info(String.Format("Emitter failed to send {0}", failureCount));

                        // Send callback method
                        _sendSuccessMethod?.Invoke(successCount, failureCount);

                        // If all events failed to send slow down - back off 30 secs
                        // NB this can be interrupted by Flush
                        if (successCount == 0 && failureCount > 0)
                        {
                            EmitterLoopBackoff();
                        }
                    }
                }
                else
                {
                    _logger.Info("Emitter thread finished processing");
                    break;
                }
            }
        }

        /// <summary>
        /// Emitter backoff logic for handling:
        /// 1. offline mode
        /// 2. 100% event failure
        /// </summary>
        private void EmitterLoopBackoff()
        {
            lock (_backOffLock)
            {
                if (!_denyBackOff)
                {
                    var interval = new Random().Next(_backOffIntervalMinMs, _backOffIntervalMaxMs);
                    _logger.Info(String.Format("Emitter waiting {0}ms after error", interval));
                    Monitor.Wait(_backOffLock, interval);
                }
                else
                {
                    _logger.Info("Emitter not waiting for back-off period");
                }
            }
        }

        /// <summary>
        /// Stop the emitter processing
        /// </summary>
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

            _logger.Info("Emitter stopping");
            _runner.Join();
            _logger.Info("Emitter stopped");

            lock (_startStopLock)
            {
                _runner = null;
                _denyBackOff = false;
            }
        }

        /// <summary>
        /// Flush the events currently in the queue. If an event fails,
        /// re-queue all further events for processing later. 
        /// </summary>
        public void Flush()
        {
            Flush(false);
        }

        /// <summary>
        /// Flush the events currently in the queue. If an event fails,
        /// re-queue all further events for processing later. 
        /// <param name="disableRestart">
        ///  Restart the emitter after flushing if false (otherwise restart or start the emitter after flushing)
        /// </param>
        /// </summary>
        public void Flush(bool disableRestart = false)
        {
            Stop();

            _logger.Info("Emitter flushing queue");

            List<Tuple<long, Payload>> items;
            var totalCount = 0;
            var failureCount = 0;

            while ((items = _queue.Peek(_sendLimit, 0)).Count > 0)
            {
                totalCount += items.Count;
                var sendResult = _endpoint.Send(items);
                failureCount += sendResult.FailureIds.Count;
                _queue.Remove(sendResult.SuccessIds);

                // Exit eagerly on any failures
                if (failureCount > 0 )
                {
                    break;
                }
            }

            if (failureCount > 0)
            {
                _logger.Warn(String.Format("Emitter failed to flush {0}/{1} events", failureCount, totalCount));
            }
            else
            {
                _logger.Info(String.Format("Emitter flushed all ({0}) events successfully", totalCount));
            }
                       
            if (!disableRestart)
            {
                Start();
            }
            else
            {
                _logger.Info("Emitter skipping restart as requested");
            }
        }

        /// <summary>
        /// Add an event to the queue, return immediately
        /// </summary>
        public void Input(Payload payload)
        {
            _queue.Enqueue(new List<Payload>() { payload });
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup - stop the emitter thread
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }
        
        /// <summary>
        /// Cleanup - stop the emitter thread
        /// </summary>
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
