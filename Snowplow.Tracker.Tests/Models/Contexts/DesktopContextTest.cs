/*
 * DesktopContextTest.cs
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
    public class DesktopContextTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Desktop Context requires 'osType'.")]
        public void testInitDesktopContextWithNullOsType()
        {
            new DesktopContext().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Desktop Context requires 'osVersion'.")]
        public void testInitDesktopContextWithNullOsVersion()
        {
            new DesktopContext()
                .SetOsType("macOS")
                .Build();
        }

        [TestMethod]
        public void testInitDesktopContext()
        {
            var dc = new DesktopContext()
                .SetOsType("Windows")
                .SetOsVersion("10")
                .SetOsServicePack("6")
                .SetOsIs64Bit(true)
                .SetDeviceManufacturer("Asus")
                .SetDeviceModel("unknown")
                .SetDeviceProcessorCount(8)
                .Build();

            Assert.IsNotNull(dc);
            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/desktop_context/jsonschema/1-0-0"",""data"":{""osType"":""Windows"",""osVersion"":""10"",""osServicePack"":""6"",""osIs64Bit"":true,""deviceManufacturer"":""Asus"",""deviceModel"":""unknown"",""deviceProcessorCount"":8}}", dc.GetJson().ToString());
        }
    }
}
