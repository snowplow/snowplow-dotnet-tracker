/*
 * Copyright (c) 2023 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Endpoints;
using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Emitters;
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Tests.Queues;

namespace Snowplow.Tracker.Tests.Emitters
{
    // Emitter that does no operations
    class MockEmitter : IEmitter
    {
        public void Close()
        {
        }

        public void Flush()
        {
        }

        public void Input(Payload payload)
        {
        }

        public void Start()
        {
        }
    }

    [TestClass]
    public class AsyncEmitterTest
    {
        class MockEndpoint : IEndpoint
        {
            public SendResult Result { get; private set; }
            public bool Response { get; set; } = true;
            public int CallCount { get; private set; } = 0;

            public SendResult Send(List<Tuple<string, Payload>> p)
            {
                CallCount += 1;

                var successIds = new List<string>();
                var failureIds = new List<string>();

                foreach (var tup in p)
                {
                    if (Response)
                    {
                        successIds.Add(tup.Item1);
                    }
                    else
                    {
                        failureIds.Add(tup.Item1);
                    }
                }

                Result = new SendResult()
                {
                    SuccessIds = successIds,
                    FailureIds = failureIds
                };
                return Result;
            }
        }

        private AsyncEmitter buildMockEmitter()
        {
            var q = new PersistentBlockingQueue(new MockStorage(), new PayloadToJsonString());
            AsyncEmitter e = new AsyncEmitter(new MockEndpoint(), q);

            return e;
        }

        [TestMethod]
        public void testEmitterStartStop()
        {
            var e = buildMockEmitter();

            e.Start();
            e.Stop();

            Assert.IsFalse(e.Running);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
                           @"Cannot start - already started")]
        public void testEmitterStartAlreadyStarted()
        {
            var e = buildMockEmitter();

            e.Start();

            try
            {
                e.Start();
            }
            finally
            {
                e.Stop();
            }
        }

        [TestMethod]
        public void testEmitterStopAlreadyStopped()
        {
            var e = buildMockEmitter();

            e.Start();
            e.Stop();
            Assert.IsFalse(e.Running);
            e.Stop();
            Assert.IsFalse(e.Running);
        }

        [TestMethod]
        public void testEmitterRestart()
        {
            var e = buildMockEmitter();

            e.Start();
            Assert.IsTrue(e.Running);
            e.Stop();
            Assert.IsFalse(e.Running);
            e.Start();
            Assert.IsTrue(e.Running);
            e.Stop();
            Assert.IsFalse(e.Running);
        }

        [TestMethod]
        public void testFailedItemsEnqueuedAgain()
        {
            var q = new PersistentBlockingQueue(new MockStorage(), new PayloadToJsonString());
            AsyncEmitter e = new AsyncEmitter(new MockEndpoint() { Response = false }, q);
            // no events will send, and so they should be at the start of the queue

            e.Start();

            var p = new Payload();
            p.AddDict(new Dictionary<string, string>() { { "foo", "bar" } });

            e.Input(p);
            Thread.Sleep(100); // this could be done better with triggers of some kind
            e.Stop();

            var inQueue = q.Peek(1);

            Assert.AreEqual(1, inQueue.Count);
        }

        [TestMethod]
        public void testBackoffInterval()
        {
            // because of the back off period (5sec +), this event should only be sent once
            var q = new PersistentBlockingQueue(new MockStorage(), new PayloadToJsonString());
            var mockEndpoint = new MockEndpoint() { Response = false };
            AsyncEmitter e = new AsyncEmitter(mockEndpoint, q);

            e.Start();
            var p = new Payload();
            p.AddDict(new Dictionary<string, string>() { { "foo", "bar" } });
            e.Input(p);
            Thread.Sleep(100);
            e.Stop();

            Assert.AreEqual(1, mockEndpoint.CallCount);
        }

        [TestMethod]
        public void testFlush()
        {
            var storage = new MockStorage();
            var queue = new PersistentBlockingQueue(storage, new PayloadToJsonString());
            var mockEndpoint = new MockEndpoint() { Response = true };
            AsyncEmitter e = new AsyncEmitter(mockEndpoint, queue, sendLimit: 1);

            for (int i = 0; i < 100; i++)
            {
                var p = new Payload();
                p.AddDict(new Dictionary<string, string>() { { "foo", "bar" } });
                e.Input(p);
            }

            Assert.IsFalse(e.Running);
            e.Flush();
            Assert.IsTrue(e.Running);
            e.Stop();

            Assert.AreEqual(100, mockEndpoint.CallCount);
            Assert.AreEqual(0, storage.TotalItems);
        }

        [TestMethod]
        public void testFlushStopsAfterFirstFailure()
        {
            var storage = new MockStorage();
            var queue = new PersistentBlockingQueue(storage, new PayloadToJsonString());
            var mockEndpoint = new MockEndpoint() { Response = false };
            AsyncEmitter e = new AsyncEmitter(mockEndpoint, queue, sendLimit: 1);

            for (int i = 0; i < 100; i++)
            {
                var p = new Payload();
                p.AddDict(new Dictionary<string, string>() { { "foo", "bar" } });
                e.Input(p);
            }

            Assert.AreEqual(100, storage.TotalItems);
            Assert.IsFalse(e.Running);

            e.Flush(true);

            Assert.IsFalse(e.Running);
            Assert.AreEqual(1, mockEndpoint.CallCount);
            Assert.AreEqual(0, mockEndpoint.Result.SuccessIds.Count);
            Assert.AreEqual(1, mockEndpoint.Result.FailureIds.Count);
            Assert.AreEqual(100, storage.TotalItems);
        }
    }
}
