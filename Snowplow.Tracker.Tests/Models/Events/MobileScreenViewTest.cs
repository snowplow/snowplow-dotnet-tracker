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

using Snowplow.Tracker.Models.Events;

namespace Snowplow.Tracker.Tests.Models.Events
{
    [TestClass]
    public class MobileScreenViewTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Name cannot be null or empty.")]
        public void testInitScreenViewWithNullIdAndName()
        {
            new MobileScreenView(null, null).Build();
        }

        [TestMethod]
        public void testInitMinimal()
        {
            MobileScreenView sv = new MobileScreenView("id", "name").Build();
            Assert.IsNotNull(sv);
            Dictionary<string, object> payload = (Dictionary<string, object>)sv.GetPayload().Payload["data"];
            Assert.AreEqual(2, payload.Count);
            Assert.AreEqual("id", payload[Constants.SV_ID]);
            Assert.AreEqual("name", payload[Constants.SV_NAME]);
            Assert.AreEqual("iglu:com.snowplowanalytics.mobile/screen_view/jsonschema/1-0-0", (string)sv.GetPayload().Payload["schema"]);
        }

        [TestMethod]
        public void testInitFull()
        {
            MobileScreenView sv = new MobileScreenView("id", "name")
                .SetType("type")
                .SetPreviousName("previousName")
                .SetPreviousId("previousId")
                .SetPreviousType("previousType")
                .SetTransitionType("transitionType")
                .Build();
            Dictionary<string, object> payload = (Dictionary<string, object>)sv.GetPayload().Payload["data"];
            Assert.AreEqual(7, payload.Count);
            Assert.AreEqual("id", payload[Constants.SV_ID]);
            Assert.AreEqual("name", payload[Constants.SV_NAME]);
            Assert.AreEqual("type", payload[Constants.SV_TYPE]);
            Assert.AreEqual("previousId", payload[Constants.SV_PREVIOUS_ID]);
            Assert.AreEqual("previousName", payload[Constants.SV_PREVIOUS_NAME]);
            Assert.AreEqual("previousType", payload[Constants.SV_PREVIOUS_TYPE]);
            Assert.AreEqual("transitionType", payload[Constants.SV_TRANSITION_TYPE]);
            Assert.AreEqual("iglu:com.snowplowanalytics.mobile/screen_view/jsonschema/1-0-0", (string)sv.GetPayload().Payload["schema"]);
        }

        [TestMethod]
        public void testGetters()
        {
            MobileScreenView sv = new MobileScreenView("id", "name")
                .SetType("type")
                .SetPreviousName("previousName")
                .SetPreviousId("previousId")
                .SetPreviousType("previousType")
                .SetTransitionType("transitionType");
            Assert.AreEqual("id", sv.GetId());
            Assert.AreEqual("name", sv.GetName());
            Assert.AreEqual("type", sv.GetScreenType());
            Assert.AreEqual("previousId", sv.GetPreviousId());
            Assert.AreEqual("previousName", sv.GetPreviousName());
            Assert.AreEqual("previousType", sv.GetPreviousType());
            Assert.AreEqual("transitionType", sv.GetTransitionType());
        }
    }
}
