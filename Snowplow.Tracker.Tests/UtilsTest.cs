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

using System.Text.RegularExpressions;

using System.IO;
using System.Collections.Generic;

namespace Snowplow.Tracker.Tests
{
    [TestClass]
    public class UtilsTest
    {
        // --- Helpers

        private string getTempFile()
        {
            var fn = Path.GetTempFileName();
            File.Delete(fn);
            return fn;
        }

        // --- Test Methods

        [TestMethod]
        public void testGetTimestamp()
        {
            Assert.AreEqual(13, Utils.GetTimestamp().ToString().Length);
        }

        [TestMethod]
        public void testGetGUID()
        {
            var guidRegex = new Regex("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-4[0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}");
            var actual = Utils.GetGUID();
            Assert.IsTrue(guidRegex.Match(actual).Success, guidRegex.ToString() + " didn't match " + actual);
        }

        [TestMethod]
        public void testDictToJSONString()
        {
            var dict = new Dictionary<string, object>
            {
                { "hello", "world" },
                { "event", false },
                { "count", 1 }
            };

            Assert.AreEqual(@"{""hello"":""world"",""event"":false,""count"":1}", Utils.DictToJSONString(dict));
        }

        [TestMethod]
        public void testGetUTF8Length()
        {
            Assert.AreEqual(11, Utils.GetUTF8Length("hello world"));
        }

        [TestMethod]
        public void testBase64EncodeString()
        {
            Assert.AreEqual("aGVsbG8gd29ybGQ=", Utils.Base64EncodeString("hello world"));
        }

        [TestMethod]
        public void testWriteReadDictionaryToFromFile()
        {
            var dict = new Dictionary<string, object>
            {
                { "hello", "world" },
                { "event", false },
                { "count", 1 }
            };
            var path = getTempFile();
            var result = Utils.WriteDictionaryToFile(path, dict);

            Assert.IsTrue(result);

            var readDict = Utils.ReadDictionaryFromFile(path);

            Assert.AreEqual(Utils.DictToJSONString(dict), Utils.DictToJSONString(readDict));

            File.Delete(path);
        }

        [TestMethod]
        public void testIsTimeInRange()
        {
            var currTime = Utils.GetTimestamp();
            var range = 300000;

            Assert.IsTrue(Utils.IsTimeInRange(currTime - 100000, currTime, range));
            Assert.IsFalse(Utils.IsTimeInRange(currTime - 1000000, currTime, range));
        }
    }
}
