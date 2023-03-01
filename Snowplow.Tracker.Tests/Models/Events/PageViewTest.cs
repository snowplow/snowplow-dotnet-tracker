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

namespace Snowplow.Tracker.Tests.Models.Events
{
    [TestClass]
    public class PageViewTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"PageUrl cannot be null or empty.")]
        public void testInitPageViewWithNullPageUrl()
        {
            new PageView()
                .SetPageUrl("")
                .Build();
        }

        [TestMethod]
        public void testInitPageView()
        {
            var pv = new PageView()
                .SetPageUrl("somePageUrl")
                .SetPageTitle("somePageTitle")
                .SetReferrer("someReferrer")
                .SetTrueTimestamp(123456789123)
                .Build();

            Assert.IsNotNull(pv);
            Assert.AreEqual(Constants.EVENT_PAGE_VIEW, pv.GetPayload().Payload[Constants.EVENT]);
            Assert.AreEqual("somePageUrl", pv.GetPayload().Payload[Constants.PAGE_URL]);
            Assert.AreEqual("somePageTitle", pv.GetPayload().Payload[Constants.PAGE_TITLE]);
            Assert.AreEqual("someReferrer", pv.GetPayload().Payload[Constants.PAGE_REFR]);

            Assert.IsNotNull(pv.GetContexts());
            Assert.IsTrue(pv.GetPayload().Payload.ContainsKey(Constants.EID));
            Assert.IsTrue(pv.GetPayload().Payload.ContainsKey(Constants.TIMESTAMP));
            Assert.IsTrue(pv.GetPayload().Payload.ContainsKey(Constants.TRUE_TIMESTAMP));
        }
    }
}
