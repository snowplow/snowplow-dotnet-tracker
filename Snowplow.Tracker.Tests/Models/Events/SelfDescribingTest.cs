/*
 * SelfDescribingTest.cs
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
using Snowplow.Tracker.Models.Events;
using System;
using System.Collections.Generic;

namespace Snowplow.Tracker.Tests.Models.Events
{
    [TestClass]
    public class SelfDescribingTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"EventData cannot be null.")]
        public void testInitSelfDescribingWithNullEventData()
        {
            new SelfDescribing().Build();
        }

        [TestMethod]
        public void testInitSelfDescribing()
        {
            var sdj = new SelfDescribingJson("iglu:com.acme/some_event/jsonschema/1-0-0", new Dictionary<string, string>
            {
                { "hello", "world" }
            });

            var sd = new SelfDescribing()
                .SetEventData(sdj)
                .SetTrueTimestamp(123456789123)
                .Build();

            Assert.IsNotNull(sd);
            Assert.AreEqual(Constants.EVENT_UNSTRUCTURED, sd.GetPayload().Payload[Constants.EVENT]);
            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"",""data"":{""schema"":""iglu:com.acme/some_event/jsonschema/1-0-0"",""data"":{""hello"":""world""}}}", sd.GetPayload().Payload[Constants.UNSTRUCTURED]);

            sd.SetBase64Encode(true);

            Assert.AreEqual("eyJzY2hlbWEiOiJpZ2x1OmNvbS5zbm93cGxvd2FuYWx5dGljcy5zbm93cGxvdy91bnN0cnVjdF9ldmVudC9qc29uc2NoZW1hLzEtMC0wIiwiZGF0YSI6eyJzY2hlbWEiOiJpZ2x1OmNvbS5hY21lL3NvbWVfZXZlbnQvanNvbnNjaGVtYS8xLTAtMCIsImRhdGEiOnsiaGVsbG8iOiJ3b3JsZCJ9fX0=", sd.GetPayload().Payload[Constants.UNSTRUCTURED_ENCODED]);

            Assert.IsNotNull(sd.GetContexts());
            Assert.IsTrue(sd.GetPayload().Payload.ContainsKey(Constants.EID));
            Assert.IsTrue(sd.GetPayload().Payload.ContainsKey(Constants.TIMESTAMP));
            Assert.IsTrue(sd.GetPayload().Payload.ContainsKey(Constants.TRUE_TIMESTAMP));
        }
    }
}
