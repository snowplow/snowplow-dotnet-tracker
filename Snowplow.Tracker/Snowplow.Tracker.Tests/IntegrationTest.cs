/*
 * IntegrationTest.cs
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
using System.Collections.Specialized;
using System.Web;
using System.Net.Fakes;
//using System.Fakes;
//using FakesClass;

namespace Snowplow.Tracker.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        private static List<NameValueCollection> payloads = new List<NameValueCollection>();

        [TestMethod]
        public void testTracker()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = (request) => {
                    var pairs = HttpUtility.ParseQueryString(request.RequestUri.Query);
                    payloads.Add(pairs);
                    return null;
                };

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net");
                t.trackStructEvent("myCategory", "myAction");
                Assert.AreEqual(payloads[payloads.Count - 1]["se_ac"], "myAction");
            }
        }
    }
}
