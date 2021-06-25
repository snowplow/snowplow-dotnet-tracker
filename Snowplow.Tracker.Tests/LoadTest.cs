/*
 * LoadTest.cs
 * 
 * Copyright (c) 2021 Snowplow Analytics Ltd. All rights reserved.
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
 * Copyright: Copyright (c) 2021 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Storage;
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Events;
using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Endpoints;
using Snowplow.Tracker.Emitters;

namespace Snowplow.Tracker.Tests
{
    [TestClass]
    public class LoadTest
    {

        private Tracker tracker = Tracker.Instance;
        private LiteDBStorage storage = null;

        private const string _collectorHostUri = @"snowplow-snowplow-pmz0wkw27skf-1566534576.eu-west-1.elb.amazonaws.com";

        private const string _testDbFilename = @"load_test.db";
        private const string _testDbJournalFilename = @"load_test-journal.db";
        private const string _testClientSessionFilename = @"load_test-session.xml";

        // --- Tests

        [TestMethod]
        [Ignore]
        public void testLoadPost()
        {
            storage = new LiteDBStorage(_testDbFilename);

            Assert.AreEqual(0, storage.TotalItems);

            var queue = new PersistentBlockingQueue(storage, new PayloadToJsonString());
            var endpoint = new SnowplowHttpCollectorEndpoint(host: _collectorHostUri, port: 8080, protocol: HttpProtocol.HTTP, method: HttpMethod.POST, byteLimitPost: 100000);
            var emitter = new AsyncEmitter(endpoint: endpoint, queue: queue, sendLimit: 1000);

            var clientSession = new ClientSession(_testClientSessionFilename);

            Assert.IsFalse(tracker.Started);
            tracker.Start(emitter: emitter, clientSession: clientSession, trackerNamespace: "testNamespace", appId: "testAppId", encodeBase64: false, synchronous: false);
            Assert.IsTrue(tracker.Started);

            for (int i = 0; i < 100; i++)
            {
                tracker.Track(new Structured()
                    .SetCategory("exampleCategory")
                    .SetAction("exampleAction")
                    .SetLabel("exampleLabel")
                    .SetProperty("exampleProperty")
                    .SetValue(17)
                    .Build()
                );
            }

            tracker.Flush();
            tracker.Stop();
            Assert.IsFalse(tracker.Started);
        }

        [TestMethod]
        [Ignore]
        public void testLoadGet()
        {
            storage = new LiteDBStorage(_testDbFilename);

            Assert.AreEqual(0, storage.TotalItems);

            var queue = new PersistentBlockingQueue(storage, new PayloadToJsonString());
            var endpoint = new SnowplowHttpCollectorEndpoint(host: _collectorHostUri, port: 8080, protocol: HttpProtocol.HTTP, method: HttpMethod.GET, byteLimitGet: 50000);
            var emitter = new AsyncEmitter(endpoint: endpoint, queue: queue, sendLimit: 25);

            var clientSession = new ClientSession(_testClientSessionFilename);

            Assert.IsFalse(tracker.Started);
            tracker.Start(emitter: emitter, clientSession: clientSession, trackerNamespace: "testNamespace", appId: "testAppId", encodeBase64: false, synchronous: false);
            Assert.IsTrue(tracker.Started);

            for (int i = 0; i < 100; i++)
            {
                tracker.Track(new Structured()
                    .SetCategory("exampleCategory")
                    .SetAction("exampleAction")
                    .SetLabel("exampleLabel")
                    .SetProperty("exampleProperty")
                    .SetValue(17)
                    .Build()
                );
            }

            tracker.Flush();
            tracker.Stop();
            Assert.IsFalse(tracker.Started);
        }

        [TestInitialize]
        public void cleanupDb()
        {
            if (File.Exists(_testDbFilename))
            {
                File.Delete(_testDbFilename);
            }

            if (File.Exists(_testDbJournalFilename))
            {
                File.Delete(_testDbJournalFilename);
            }

            if (File.Exists(_testClientSessionFilename))
            {
                File.Delete(_testClientSessionFilename);
            }
        }

        [TestCleanup]
        public void stopTracker()
        {
            storage.Dispose();
            storage = null;

            tracker.Stop();
            tracker = null;
        }
    }
}
