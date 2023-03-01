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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Models;
using System;

namespace Snowplow.Tracker.Tests.Models.Adapters
{
    [TestClass]
    public class PayloadToJsonStringTest
    {
        [TestMethod]
        public void testToStringGoodJson()
        {
            var payload = new Payload();
            payload.Add("name", "value");

            var p = new PayloadToJsonString();

            var serialized = p.ToString(payload);

            Assert.AreEqual(@"{""Payload"":{""name"":""value""}}", serialized);
        }

        [TestMethod]
        public void testFromStringGoodJson()
        {
            var p = new PayloadToJsonString();

            var actual = p.FromString(@"{""Payload"":{""name"":""value""}}");

            var expected = new Payload();
            expected.Add("name", "value");

            CollectionAssert.AreEqual(expected.Payload, actual.Payload);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Invalid JSON: ""{""")]
        public void testFromStringBadJson()
        {
            var p = new PayloadToJsonString();
            p.FromString(@"{");
        }
    }
}
