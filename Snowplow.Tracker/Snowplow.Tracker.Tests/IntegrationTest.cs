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
using System.Net;
using System.Net.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
//using System.Fakes;
//using FakesClass;

namespace Snowplow.Tracker.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        private static List<NameValueCollection> payloads = new List<NameValueCollection>();
        private static FakesDelegates.Func<HttpWebRequest, WebResponse> fake = (request) => {
            var pairs = HttpUtility.ParseQueryString(request.RequestUri.Query);
            payloads.Add(pairs);
            return null;
        };

        private static void checkResult(Dictionary<string, string> expected, NameValueCollection actual)
        {
            foreach (string key in expected.Keys)
            {
                Assert.AreEqual(expected[key], actual[key]);
            }
        }

        [TestMethod]
        public void testTrackPageView()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net");
                t.trackPageView("http://www.example.com", "title page", "http://www.referrer.com");
                var expected = new Dictionary<string, string>
                {
                    {"e", "pv"},
                    {"url", "http://www.example.com"},
                    {"page", "title page"},
                    {"refr", "http://www.referrer.com"}
                };

                checkResult(expected, payloads[payloads.Count - 1]);
            }
        }

        [TestMethod]
        public void testTrackStructEvent()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net");
                t.trackStructEvent("myCategory", "myAction", "myLabel", "myProperty", 17);
                var expected = new Dictionary<string, string>
                {
                    {"e", "se"},
                    {"se_ca", "myCategory"},
                    {"se_ac", "myAction"},
                    {"se_la", "myLabel"},
                    {"se_pr", "myProperty"},
                    {"se_va", "17"}
                };

                checkResult(expected, payloads[payloads.Count - 1]);
            }
        }

        [TestMethod]
        public void testTrackEcommerceTransaction()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net");
                var hat = new TransactionItem("pbz0026", 20, 1);
                var shirt = new TransactionItem("pbz0038", 15, 1, "shirt", "clothing");
                var items = new List<TransactionItem> { hat, shirt };
                t.trackEcommerceTransaction("6a8078be", 35, "affiliation", 3, 0, "Phoenix", "Arizona", "US", "USD", items);
                var expectedTransaction = new Dictionary<string, string>
                {
                    {"e", "tr"},
                    {"tr_id", "6a8078be"},
                    {"tr_tt", "35"},
                    {"tr_af", "affiliation"},
                    {"tr_tx", "3"},
                    {"tr_sh", "0"},
                    {"tr_ci", "Phoenix"},
                    {"tr_st", "Arizona"},
                    {"tr_co", "US"},
                    {"tr_cu", "USD"}

                };
                var expectedHat = new Dictionary<string, string>
                {
                    {"e", "ti"},
                    {"ti_id", "6a8078be"},
                    {"ti_sk", "pbz0026"},
                    {"ti_pr", "20"},
                    {"ti_qu", "1"},
                    {"ti_cu", "USD"}
                };
                var expectedShirt = new Dictionary<string, string>
                {
                    {"e", "ti"},
                    {"ti_id", "6a8078be"},
                    {"ti_sk", "pbz0038"},
                    {"ti_pr", "15"},
                    {"ti_qu", "1"},
                    {"ti_nm", "shirt"},
                    {"ti_ca", "clothing"},
                    {"ti_cu", "USD"}
                };
                checkResult(expectedTransaction, payloads[payloads.Count - 3]);
                checkResult(expectedHat, payloads[payloads.Count - 2]);
                checkResult(expectedShirt, payloads[payloads.Count - 1]);
            }
        }
    }
}
