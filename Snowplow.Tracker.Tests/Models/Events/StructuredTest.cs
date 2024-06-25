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
using Snowplow.Tracker.Models.Events;
using System;
using System.Globalization;

namespace Snowplow.Tracker.Tests.Models.Events
{
    [TestClass]
    public class StructuredTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Category cannot be null or empty.")]
        public void testInitStructuredWithNullCategory()
        {
            new Structured().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Action cannot be null or empty.")]
        public void testInitStructuredWithNullAction()
        {
            new Structured()
                .SetCategory("category")
                .Build();
        }

        [TestMethod]
        public void testInitStructured()
        {
            var s = new Structured()
                .SetCategory("category")
                .SetAction("action")
                .SetLabel("label")
                .SetProperty("property")
                .SetValue(1.2)
                .SetTrueTimestamp(123456789123)
                .Build();

            Assert.IsNotNull(s);
            Assert.AreEqual(Constants.EVENT_STRUCTURED, s.GetPayload().Payload[Constants.EVENT]);
            Assert.AreEqual("category", s.GetPayload().Payload[Constants.SE_CATEGORY]);
            Assert.AreEqual("action", s.GetPayload().Payload[Constants.SE_ACTION]);
            Assert.AreEqual("label", s.GetPayload().Payload[Constants.SE_LABEL]);
            Assert.AreEqual("property", s.GetPayload().Payload[Constants.SE_PROPERTY]);
            Assert.AreEqual("1.2", s.GetPayload().Payload[Constants.SE_VALUE]);

            Assert.IsNotNull(s.GetContexts());
            Assert.IsTrue(s.GetPayload().Payload.ContainsKey(Constants.EID));
            Assert.IsTrue(s.GetPayload().Payload.ContainsKey(Constants.TIMESTAMP));
            Assert.IsTrue(s.GetPayload().Payload.ContainsKey(Constants.TRUE_TIMESTAMP));
        }

        [TestMethod]
        public void testValueIgnoresCurrentLocale()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("sk-SK");
            var s = new Structured()
                .SetCategory("category")
                .SetAction("action")
                .SetLabel("label")
                .SetProperty("property")
                .SetValue(1.2)
                .Build();

            Assert.AreEqual("1.2", s.GetPayload().Payload[Constants.SE_VALUE]);
        }
    }
}
