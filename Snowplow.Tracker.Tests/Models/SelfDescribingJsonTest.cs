/*
 * SelfDescribingJsonTest.cs
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
    public class SelfDescribingJsonTest
    {
        [TestMethod]
        public void testInitSDJWithObject()
        {
            object nullObj = null;
            var sdj = new SelfDescribingJson("iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-4", nullObj);

            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-4""}", sdj.ToString());

            object dictObj = new Dictionary<string, object>
            {
                { "hello", "world" }
            };
            sdj.SetData(dictObj);

            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-4"",""data"":{""hello"":""world""}}", sdj.ToString());
        }

        [TestMethod]
        public void testInitSDJWithSelfDescribingJson()
        {
            SelfDescribingJson nullSdj = null;
            var sdj = new SelfDescribingJson("iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-4", nullSdj);

            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-4""}", sdj.ToString());

            object dictObj = new Dictionary<string, object>
            {
                { "hello", "world" }
            };
            var notNullSdj = new SelfDescribingJson("iglu:com.snowplowanalytics.snowplow/test_event/jsonschema/1-0-4", dictObj);
            sdj.SetData(notNullSdj);

            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-4"",""data"":{""schema"":""iglu:com.snowplowanalytics.snowplow/test_event/jsonschema/1-0-4"",""data"":{""hello"":""world""}}}", sdj.ToString());
        }
    }
}
