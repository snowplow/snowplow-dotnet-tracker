/*
 * TimingTest.cs
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Events;
using System;

namespace Snowplow.Tracker.Tests.Models.Events
{
    [TestClass]
    public class TimingTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Category cannot be null or empty.")]
        public void testInitTimingWithNullCategory()
        {
            new Timing().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Variable cannot be null or empty.")]
        public void testInitTimingWithNullVariable()
        {
            new Timing()
                .SetCategory("category")
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Timing cannot be null.")]
        public void testInitTimingWithNullTiming()
        {
            new Timing()
                .SetCategory("category")
                .SetVariable("variable")
                .Build();
        }

        [TestMethod]
        public void testInitTiming()
        {
            var t = new Timing()
                .SetCategory("category")
                .SetVariable("variable")
                .SetTiming(10)
                .SetLabel("label")
                .SetTrueTimestamp(123456789123)
                .Build();

            var sdj = (SelfDescribingJson)t.GetPayload();

            Assert.IsNotNull(t);
            Assert.IsNotNull(sdj);
            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/timing/jsonschema/1-0-0"",""data"":{""category"":""category"",""label"":""label"",""timing"":10,""variable"":""variable""}}", sdj.ToString());
            Assert.IsNotNull(t.GetContexts());
        }
    }
}
