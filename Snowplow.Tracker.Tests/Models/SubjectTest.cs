/*
 * SubjectTest.cs
 * 
 * Copyright (c) 2014 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Fred Blundun
 * Copyright: Copyright (c) 2014 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Snowplow.Tracker.Tests
{
    // Emitter that does no operations
    class MockEmitter : IEmitter
    {
        public void Close()
        {
        }

        public void Flush()
        {
        }

        public void Input(Payload payload)
        {
        }
    }

    [TestClass]
    public class SubjectTest
    {
        [TestMethod]
        public void testSubjectInitialization()
        {
            var subject = new Subject();
            Assert.AreEqual(subject.nvPairs["p"], "pc");
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
            var expected = new Dictionary<string, string>
            {
                {"p", "mob"},
                {"uid", "malcolm"},
                {"res", "100x200"},
                {"vp", "50x60"},
                {"cd", "24"},
                {"tz", "Europe London"},
                {"lang", "en"},
            };
            foreach (string key in expected.Keys)
            {
                Assert.AreEqual(subject.nvPairs[key], expected[key]);
            }
        }
    }
}
