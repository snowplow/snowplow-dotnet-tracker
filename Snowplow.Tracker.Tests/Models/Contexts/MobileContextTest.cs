/*
 * MobileContextTest.cs
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
using Snowplow.Tracker.Models.Contexts;
using System;

namespace Snowplow.Tracker.Tests.Models.Contexts
{
    [TestClass]
    public class MobileContextTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Mobile context requires 'osType'.")]
        public void testInitMobileContextWithNullOsType()
        {
            new MobileContext().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Mobile context requires 'osVersion'.")]
        public void testInitMobileContextWithNullOsVersion()
        {
            new MobileContext()
                .SetOsType("iOS")
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Mobile context requires 'deviceManufacturer'.")]
        public void testInitMobileContextWithNullDeviceManufacturer()
        {
            new MobileContext()
                .SetOsType("iOS")
                .SetOsVersion("8.4")
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Mobile context requires 'deviceModel'.")]
        public void testInitMobileContextWithNullDeviceModel()
        {
            new MobileContext()
                .SetOsType("iOS")
                .SetOsVersion("8.4")
                .SetDeviceManufacturer("Apple")
                .Build();
        }

        [TestMethod]
        public void testInitMobileContext()
        {
            var mc = new MobileContext()
                .SetOsType("iOS")
                .SetOsVersion("8.4")
                .SetDeviceManufacturer("Apple")
                .SetDeviceModel("Apple iPhone")
                .SetCarrier("FREE")
                .SetNetworkType(NetworkType.Mobile)
                .SetNetworkTechnology("LTE")
                .Build();

            Assert.IsNotNull(mc);
            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/mobile_context/jsonschema/1-0-1"",""data"":{""osType"":""iOS"",""osVersion"":""8.4"",""deviceManufacturer"":""Apple"",""deviceModel"":""Apple iPhone"",""carrier"":""FREE"",""networkType"":""mobile"",""networkTechnology"":""LTE""}}", mc.GetJson().ToString());
        }
    }
}
