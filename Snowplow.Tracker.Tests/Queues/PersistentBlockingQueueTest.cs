﻿/*
 * PersistentBlockingQueueTest.cs
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
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Snowplow.Tracker.Tests.Queues
{
    class MockStorage : IStorage
    {
        private List<StorageRecord> _items = new List<StorageRecord>();

        public int TotalItems
        {
            get
            {
                return _items.Count;
            }
        }

        public void Put(string item)
        {
            _items.Insert(0, new StorageRecord()
            {
                Id = 0,
                Item = item
            });
        }

        public List<StorageRecord> TakeLast(int n)
        {
            if (TotalItems - n < 0)
            {
                return _items.GetRange(0, TotalItems);
            }
            else
            {
                return _items.GetRange(TotalItems - n, n);
            }
        }

        public bool Delete(List<long> idList)
        {
            _items.RemoveRange(TotalItems - idList.Count, idList.Count);
            return true;
        }
    }

    [TestClass]
    public class PersistentBlockingQueueTest
    {
        [TestMethod]
        public void testMockStorageOne()
        {
            var s = new MockStorage();

            Assert.AreEqual(s.TotalItems, 0);

            s.Put("hello world");

            Assert.AreEqual(1, s.TotalItems);

            var i = s.TakeLast(1);

            Assert.AreEqual(1, i.Count);
            Assert.AreEqual("hello world", i[0].Item);
            Assert.AreEqual(1, s.TotalItems);

            var del = s.Delete(new List<long> { 0 });

            Assert.IsTrue(del);
            Assert.AreEqual(0, s.TotalItems);
        }

        [TestMethod]
        public void testMockStorageMany()
        {
            var s = new MockStorage();

            Assert.AreEqual(s.TotalItems, 0);

            s.Put("hello world");
            s.Put("hello world 2");
            s.Put("hello world 3");

            Assert.AreEqual(3, s.TotalItems);

            var i = s.TakeLast(3);

            Assert.AreEqual(3, i.Count);
            Assert.AreEqual("hello world", i[2].Item);
            Assert.AreEqual("hello world 2", i[1].Item);
            Assert.AreEqual("hello world 3", i[0].Item);
            Assert.AreEqual(3, s.TotalItems);

            var del = s.Delete(new List<long> { 0, 1, 2 });

            Assert.IsTrue(del);
            Assert.AreEqual(0, s.TotalItems);
        }

        [TestMethod]
        public void testAddRemoveOne()
        {
            var storage = new MockStorage();
            var queue = new PersistentBlockingQueue(storage, new PayloadToJsonString());

            var dict = new Dictionary<string, string>();
            dict.Add("hello", "world");
            var samplePayload = new Payload();
            samplePayload.AddDict(dict);

            var payload = new List<Payload>();
            payload.Add(samplePayload);

            queue.Enqueue(payload);
            var actualPayload = queue.Peek(1);

            Assert.AreEqual(1, actualPayload.Count);
            CollectionAssert.AreEqual(samplePayload.Payload, actualPayload[0].Item2.Payload);
        }

        class MockConsumer
        {
            public List<Payload> Consumed
            {
                get;
                private set;
            }

            private IPersistentBlockingQueue _q;
            private int _count;
            private int _timeout;

            public MockConsumer(int count, IPersistentBlockingQueue q, int timeout = 1000)
            {
                _count = count;
                _q = q;
                _timeout = timeout;
            }

            public void Consume()
            {
                Consumed = new List<Payload>();

                for (int i = 0; i < _count; i++)
                {
                    var items = _q.Peek(1, _timeout);
                    foreach (var item in items)
                    {
                        Consumed.Add(item.Item2);
                    }
                }
            }
        }

        class MockProducer
        {
            private IPersistentBlockingQueue _q;
            private int _count;

            public MockProducer(int count, IPersistentBlockingQueue q)
            {
                _count = count;
                _q = q;
            }

            public void Produce()
            {
                for (int i = 0; i < _count; i++)
                {
                    var dict = new Dictionary<string, string>();
                    dict.Add("hello", "world");
                    var samplePayload = new Payload();
                    samplePayload.AddDict(dict);

                    var payload = new List<Payload>();
                    payload.Add(samplePayload);

                    _q.Enqueue(payload);
                }
            }
        }

        [TestMethod]
        public void testAddRemoveThreaded()
        {
            var mockStorage = new MockStorage();

            var q = new PersistentBlockingQueue(mockStorage, new PayloadToJsonString());

            int expectedRecordCount = 1000;

            var consumer = new MockConsumer(expectedRecordCount, q);

            var producer = new MockProducer(expectedRecordCount / 2, q);
            var secondProducer = new MockProducer(expectedRecordCount / 2, q);

            var consumerThread = new Thread(new ThreadStart(consumer.Consume));
            consumerThread.Start();

            var producerThread = new Thread(new ThreadStart(producer.Produce));
            producerThread.Start();

            var secondProducerThread = new Thread(new ThreadStart(secondProducer.Produce));
            secondProducerThread.Start();

            consumerThread.Join(1000); // time out if errors

            Assert.AreEqual(expectedRecordCount, consumer.Consumed.Count);
        }

        [TestMethod]
        public void testMultipleConsumers()
        {
            var mockStorage = new MockStorage();

            var q = new PersistentBlockingQueue(mockStorage, new PayloadToJsonString());

            int expectedRecordCount = 1000;

            var producer = new MockProducer(expectedRecordCount, q);

            var producerThread = new Thread(new ThreadStart(producer.Produce));
            producerThread.Start();

            var threads = new List<Thread>();
            var consumers = new List<MockConsumer>();

            for (int i = 0; i < expectedRecordCount; i++)
            {
                var consumer = new MockConsumer(1, q);
                var consumerThread = new Thread(new ThreadStart(consumer.Consume));
                consumerThread.Start();
                threads.Add(consumerThread);
                consumers.Add(consumer);
            }

            threads.ForEach(t => { t.Join(100); });

            var total = (from c in consumers select c.Consumed.Count).Sum();
            bool allConsumedOneItem = !consumers.Any(item => { return item.Consumed.Count != 1; });

            Assert.IsTrue(allConsumedOneItem);
            Assert.AreEqual(expectedRecordCount, total);
        }

        [TestMethod]
        public void testConsumerTimeout()
        {
            var mockStorage = new MockStorage();

            var q = new PersistentBlockingQueue(mockStorage, new PayloadToJsonString());

            var consumer = new MockConsumer(1, q, 10);
            var consumerThread = new Thread(new ThreadStart(consumer.Consume));
            consumerThread.Start();

            consumerThread.Join(500);

            Assert.AreEqual(0, consumer.Consumed.Count);
        }
    }
}
