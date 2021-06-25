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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Models.Contexts;
using System;

namespace Snowplow.Tracker.Tests.Models.Contexts
{
    [TestClass]
    public class SessionContextTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Session context requires 'userId'.")]
        public void testInitSessionContextWithNullUserId()
        {
            new SessionContext().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Session context requires 'sessionId'.")]
        public void testInitSessionContextWithNullSessionId()
        {
            new SessionContext()
                .SetUserId("userId")
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Session context requires 'sessionIndex'.")]
        public void testInitSessionContextWithNullSessionIndex()
        {
            new SessionContext()
                .SetUserId("userId")
                .SetSessionId("sessionId")
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Session context requires 'previousSessionId'.")]
        public void testInitSessionContextWithNullPreviousSessionId()
        {
            new SessionContext()
                .SetUserId("userId")
                .SetSessionId("sessionId")
                .SetSessionIndex(1)
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Session context requires 'storageMechanism'.")]
        public void testInitSessionContextWithNullStorageMechanism()
        {
            new SessionContext()
                .SetUserId("userId")
                .SetSessionId("sessionId")
                .SetSessionIndex(1)
                .SetPreviousSessionId(null)
                .Build();
        }

        [TestMethod]
        public void testInitSessionContext()
        {
            var sc = new SessionContext()
                .SetUserId("userId")
                .SetSessionId("sessionId")
                .SetSessionIndex(1)
                .SetPreviousSessionId(null)
                .SetStorageMechanism(StorageMechanism.LocalStorage)
                .SetFirstEventId("firstEventId")
                .Build();

            Assert.IsNotNull(sc);
            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/client_session/jsonschema/1-0-1"",""data"":{""userId"":""userId"",""sessionId"":""sessionId"",""sessionIndex"":1,""previousSessionId"":null,""storageMechanism"":""LOCAL_STORAGE"",""firstEventId"":""firstEventId""}}", sc.GetJson().ToString());
        }
    }
}
