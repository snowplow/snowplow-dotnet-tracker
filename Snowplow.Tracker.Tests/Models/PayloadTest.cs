/*
 * PayloadTest.cs
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
    [TestClass]
    public class PayloadTest
    {
        [TestMethod]
        public void testAddString()
        {
            var payload = new Payload();
            payload.Add("name", "value");
            var pairs = payload.NvPairs;
            Assert.AreEqual(pairs["name"], "value");
        }

        [TestMethod]
        public void testAddNumber()
        {
            var payload = new Payload();
            payload.Add("price", 99.9);
            var pairs = payload.NvPairs;
            Assert.AreEqual(pairs["price"], "99.9");
        }

        [TestMethod]
        public void testAddEmptyString()
        {
            var payload = new Payload();
            payload.Add("empty", "");
            var pairs = payload.NvPairs;
            Assert.IsFalse(pairs.ContainsKey("name"));
        }

        [TestMethod]
        public void testAddNull()
        {
            var payload = new Payload();
            string nullString = null;
            payload.Add("null", nullString);
            var pairs = payload.NvPairs;
            Assert.IsFalse(pairs.ContainsKey("null"));
        }

        [TestMethod]
        public void testAddDict()
        {
            var payload = new Payload();
            var dict = new Dictionary<string, string>
            {
                { "one", "un" },
                { "two", "deux" },
                { "three", "trois" }
            };
            payload.AddDict(dict);
            var pairs = payload.NvPairs;
            foreach (KeyValuePair<string, string> nvPair in dict)
            {
                Assert.AreEqual(nvPair.Value, pairs[nvPair.Key]);
            }
        }

        [TestMethod]
        public void testAddJson()
        {
            var payload = new Payload();
            var json = new Dictionary<string, object>
            {
                {"schema", "iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"},
                {"data", new Dictionary<string, object>
                {
                    { "schema", "iglu:com.acme/test/jsonschema/1-0-0" },
                    { "data", new Dictionary<string, object>
                    {
                        { "user_type", "test" }
                    }
                    }
                }
                }
            };
            payload.AddJson(json, false, "ue_px", "ue_pr");
            var pairs = payload.NvPairs;
            var expected = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"",""data"":{""schema"":""iglu:com.acme/test/jsonschema/1-0-0"",""data"":{""user_type"":""test""}}}";
            Assert.AreEqual(pairs["ue_pr"], expected);
        }
    }
}
