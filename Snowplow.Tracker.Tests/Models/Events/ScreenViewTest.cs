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
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Events;
using System;

namespace Snowplow.Tracker.Tests.Models.Events
{
    [TestClass]
    public class ScreenViewTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Both Name and Id cannot be null or empty.")]
        public void testInitScreenViewWithNullIdAndName()
        {
            new ScreenView().Build();
        }

        [TestMethod]
        public void testInitScreenView()
        {
            var sv = new ScreenView()
                .SetId("someId")
                .SetName("someName")
                .SetTrueTimestamp(123456789123)
                .Build();

            var sdj = (SelfDescribingJson) sv.GetPayload();

            Assert.IsNotNull(sv);
            Assert.IsNotNull(sdj);
            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/screen_view/jsonschema/1-0-0"",""data"":{""name"":""someName"",""id"":""someId""}}", sdj.ToString());
            Assert.IsNotNull(sv.GetContexts());
        }
    }
}
