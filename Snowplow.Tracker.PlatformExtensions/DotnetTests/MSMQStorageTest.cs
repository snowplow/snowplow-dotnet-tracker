/*
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
 */

#define MSMQ_ENABLED

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.PlatformExtensions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace DotnetTests
{
    [TestClass]
#if (!MSMQ_ENABLED)
    [Ignore]
#endif
    public class MSMQStorageTest
    {
               
        [TestMethod]
        public void TestWhenLoadedZeroTotalItems()
        {
            using (MSMQStorage s = new MSMQStorage(@".\Private$\SnowplowTrackerTestZeroItems"))
            {
                var n = new PerformanceCounterCategory("MSMQ Queue").GetInstanceNames();
                Assert.AreEqual(0, s.TotalItems);
            }          
        }

        [TestMethod]
        public void TestPutTakeOne()
        {
            using (MSMQStorage s = new MSMQStorage(@".\Private$\SnowplowTrackerTestPutTakeOne"))
            {
                s.Purge();
                Assert.AreEqual(0, s.TotalItems);

                s.Put("hello world");
                Assert.AreEqual(1, s.TotalItems);

                var items = s.TakeLast(1);
                Assert.AreEqual(1, s.TotalItems); // not deleted yet
                Assert.AreEqual(1, items.Count);
                Assert.AreEqual("hello world", items[0].Item);

                s.Delete(new List<string>() { items[0].Id } );                
                Assert.AreEqual(0, s.TotalItems);
            }
        }

        [TestMethod]
        public void TestPutTakeMany()
        {
            using (MSMQStorage s = new MSMQStorage(@".\Private$\SnowplowTrackerTestPutTakeMany"))
            {
                s.Purge();
                Assert.AreEqual(0, s.TotalItems);

                var expected = new List<string>();
                for (var i=0; i<100; i++)
                {
                    s.Put("" + i);
                    expected.Insert(0, "" + i);
                }

                Assert.AreEqual(100, s.TotalItems);

                var items = s.TakeLast(100);
                Assert.AreEqual(100, s.TotalItems);
                Assert.AreEqual(100, items.Count);

                var values = (from item in items
                              select item.Item).ToList();

                CollectionAssert.AreEqual(expected, values);

                var ids = from item in items
                          select item.Id;
                
                s.Delete(ids.ToList());
                Assert.AreEqual(0, s.TotalItems);
            }
        }

        [TestMethod]
        public void TestTakeWithNoElementsInQueue()
        {
            using (MSMQStorage s = new MSMQStorage(@".\Private$\SnowplowTrackerTestTakeWithNoElementsInQueue"))
            {
                s.Purge();
                var items = s.TakeLast(1);
                Assert.AreEqual(0, items.Count);
                Assert.AreEqual(0, s.TotalItems);
            }
        }

        [TestMethod]
        public void TestMultipleTakesReturnSame()
        {
            using (MSMQStorage s = new MSMQStorage(@".\Private$\SnowplowTrackerTestTakeMultipleReturnsSame"))
            {
                s.Purge();
                Assert.AreEqual(0, s.TotalItems);

                var expected = new List<string>();
                for (var i = 0; i < 100; i++)
                {
                    s.Put("" + i);
                    expected.Insert(0, "" + i);
                }

                for (var i = 0; i < 10; i++)
                {
                    var items = s.TakeLast(100);
                    var values = (from item in items
                                 select item.Item).ToList();
                    CollectionAssert.AreEqual(expected, values);
                }

                Assert.AreEqual(100, s.TotalItems);
            }
        }

        [TestMethod]
        public void TestDeleteRemovesItems()
        {
            using (MSMQStorage s = new MSMQStorage(@".\Private$\SnowplowTrackerTestDeleteRemovesItems"))
            {
                s.Purge();
                Assert.AreEqual(0, s.TotalItems);

                var expected = new List<string>();
                for (var i = 0; i < 100; i++)
                {
                    s.Put("" + i);
                    expected.Insert(0, "" + i);
                }

                var taken = s.TakeLast(100);
                var expectedLength = s.TotalItems;

                Assert.AreEqual(100, expectedLength);
                
                foreach (var item in taken)
                {
                    Assert.AreEqual(true, s.Delete(new List<string>() { item.Id }));
                    expectedLength -= 1;
                    Assert.AreEqual(expectedLength, s.TotalItems);
                }

                Assert.AreEqual(0, s.TotalItems);
            }
        }

        [TestMethod]
        public void TestDeleteInvalidArgument()
        {
            using (MSMQStorage s = new MSMQStorage(@".\Private$\SnowplowTrackerTestInvalidArgDelete"))
            {
                s.Purge();
                var good = s.Delete(new List<string>() { "??" });
                Assert.AreEqual(false, good);
            }
        }
    }
}
