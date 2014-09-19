/*
 * MsmqTest.cs
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
using System.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Snowplow.Tracker.Tests
{
    [TestClass]
    public class MsmqTest
    {
        [TestMethod]
        public void testMsmqEmitterCreatesQueue()
        {
            var randomPath = String.Format(@".\private$\{0}", new Random().Next(10, 1000));
            var msmqEmitter = new MsmqEmitter(randomPath);
            Assert.IsTrue(MessageQueue.Exists(randomPath));
            MessageQueue.Delete(randomPath);
            Assert.IsFalse(MessageQueue.Exists(randomPath));
        }

        [TestMethod]
        public void testMsmqEmitterDefaultQueue()
        {
            var path = @".\private$\TestQueue";
            var msmqEmitter = new MsmqEmitter(path);
            msmqEmitter.Queue.Purge();
            msmqEmitter.Input(new Dictionary<string, string> { { "name", "value" } });
            msmqEmitter.Input(new Dictionary<string, string> { { "e", "pv" } });
            var messageEnumerator = msmqEmitter.Queue.GetMessageEnumerator2();
            var messages = new List<Message>();
            while (messageEnumerator.MoveNext())
            {
                Message evt = messageEnumerator.Current;
                messages.Add(evt);
            }
            Assert.AreEqual(messages[0].Body, @"{""name"":""value""}");
            Assert.AreEqual(messages[1].Body, @"{""e"":""pv""}");
            Assert.AreEqual(messages.Count, 2);
        }

        [TestMethod]
        public void testMsmqEmitterReadFromExistingQueue()
        {
            var path = @".\private$\TestQueue";
            var msmqEmitter1 = new MsmqEmitter(path);
            msmqEmitter1.Queue.Purge();
            msmqEmitter1.Input(new Dictionary<string, string> { { "name", "value" } });
            msmqEmitter1.Input(new Dictionary<string, string> { { "e", "pv" } });
            var msmqEmitter2 = new MsmqEmitter(path);
            var messageEnumerator = msmqEmitter2.Queue.GetMessageEnumerator2();
            var messages = new List<Message>();
            while (messageEnumerator.MoveNext())
            {
                Message evt = messageEnumerator.Current;
                messages.Add(evt);
            }
            Assert.AreEqual(messages[0].Body, @"{""name"":""value""}");
            Assert.AreEqual(messages[1].Body, @"{""e"":""pv""}");
            Assert.AreEqual(messages.Count, 2);
        }

    }
}
