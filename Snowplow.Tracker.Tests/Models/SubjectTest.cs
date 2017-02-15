/*
 * SubjectTest.cs
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
using System.Collections.Generic;
using Snowplow.Tracker.Models;

namespace Snowplow.Tracker.Tests.Models
{
    [TestClass]
    public class SubjectTest
    {
        [TestMethod]
        public void testSubjectInitialization()
        {
            var subject = new Subject();
            Assert.AreEqual(subject._payload.Payload["p"], "pc");
        }

        [TestMethod]
        public void testSubjectSetterMethods()
        {
            var subject = new Subject();
            subject.SetPlatform(Platform.Mob);
            subject.SetUserId("malcolm");
            subject.SetScreenResolution(100, 200);
            subject.SetViewport(50, 60);
            subject.SetColorDepth(24);
            subject.SetTimezone("Europe London");
            subject.SetLang("en");
            subject.SetIpAddress("127.0.0.1");
            subject.SetUseragent("useragent");
            subject.SetDomainUserId("duid");
            subject.SetNetworkUserId("tnuid");

            var expected = new Dictionary<string, string>
            {
                {"p", "mob"},
                {"uid", "malcolm"},
                {"res", "100x200"},
                {"vp", "50x60"},
                {"cd", "24"},
                {"tz", "Europe London"},
                {"lang", "en"},
                {"ip", "127.0.0.1"},
                {"ua", "useragent"},
                {"duid", "duid"},
                {"tnuid", "tnuid"}
            };

            foreach (string key in expected.Keys)
            {
                Assert.AreEqual(subject._payload.Payload[key], expected[key]);
            }
        }
    }
}
