﻿/*
 * IntegrationTest.cs
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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Storage;
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Contexts;
using Snowplow.Tracker.Models.Events;
using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Endpoints;
using Snowplow.Tracker.Emitters;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Tests
{
    [TestClass]
    public class IntegrationTest
    {

        private Tracker tracker = Tracker.Instance;

        private const string _testDbFilename = @"integration_test.db";
        private const string _testDbJournalFilename = @"integration_test-journal.db";

        // --- Mocks

        class MockGet
        {
            public List<string> Queries { get; private set; } = new List<string>();
            public int StatusCode { get; set; } = 200;

            public RequestResult HttpGet(string uri, bool oversize, List<long> itemIds)
            {
                Queries.Insert(0, uri);

                Task<int> responseTask = new Task<int>(() => StatusCode);
                responseTask.Start();

                return new RequestResult()
                {
                    IsOversize = oversize,
                    ItemIds = itemIds,
                    StatusCodeTask = responseTask
                };
            }
        }

        class PostRequest
        {
            public string Uri { get; set; }
            public string PostData { get; set; }
        }

        class MockPost
        {
            public List<PostRequest> Queries { get; private set; } = new List<PostRequest>();
            public int StatusCode { get; set; } = 200;

            public RequestResult HttpPost(string uri, string postData, bool oversize, List<long> itemIds)
            {
                var postRec = new PostRequest { Uri = uri, PostData = postData };
                Queries.Insert(0, postRec);

                Task<int> responseTask = new Task<int>(() => StatusCode);
                responseTask.Start();

                return new RequestResult()
                {
                    IsOversize = oversize,
                    ItemIds = itemIds,
                    StatusCodeTask = responseTask
                };
            }
        }

        // --- Helpers

       private void assertQueryContainsDefaults(string queryString)
        {
            var opening = @"^http://snowplowanalytics.com/i\?";
            var trailing = @"&eid=[^&]+&dtm=[^&]+&tv=[^&]+&tna=testNamespace&aid=testAppId&p=pc&stm=[0-9]{13}";

            var expectedOpening = new Regex(opening);
            Assert.IsTrue(expectedOpening.Match(queryString).Success, queryString + " is missing " + expectedOpening.ToString());

            var expectedTrailing = new Regex(trailing);
            Assert.IsTrue(expectedTrailing.Match(queryString).Success, queryString + " is missing " + expectedTrailing.ToString());
        }

        private void assertQueryContainsAll(string queryString, Dictionary<string,string> elements)
        {
            foreach (var k in elements.Keys)
            {
                var matcher = new Regex("[&?]" + k + "=(?<value>[^&]+)");
                var match = matcher.Match(queryString);

                Assert.IsTrue(match.Success, "Couldn't find key " + k + " in query " + queryString);

                var found = match.Result("${value}");
                string decoded = WebUtility.UrlDecode(found);

                Assert.AreEqual(elements[k], decoded, "key " + k + " has the wrong value " + decoded);
            }
        } 

        // --- Subject setters

        private bool ensureSubjectSet(Tracker tracker)
        {
            var subjectOne = new Subject();
            var subjectTwo = new Subject();

            tracker.SetSubject(subjectOne);
            tracker.SetPlatform(Platform.Mob);
            tracker.SetUserId("malcolm");
            tracker.SetScreenResolution(100, 200);
            tracker.SetViewport(50, 60);
            tracker.SetColorDepth(24);
            tracker.SetTimezone("Europe London");
            tracker.SetLang("en");

            tracker.SetSubject(subjectTwo);
            tracker.SetUserId("6561");
            tracker.SetLang("fr");
            tracker.SetScreenResolution(150, 250);

            var expectedOne = new Dictionary<string, string>
            {
                {"p", "mob"},
                {"uid", "malcolm"},
                {"res", "100x200"},
                {"vp", "50x60"},
                {"cd", "24"},
                {"tz", "Europe London"},
                {"lang", "en"},
            };

            var expectedTwo = new Dictionary<string, string>
            {
                {"p", "pc"},
                {"res", "150x250"},
                {"lang", "fr"},
            };

            foreach (string key in expectedOne.Keys)
            {
                Assert.AreEqual(subjectOne._payload.Payload[key], expectedOne[key]);
            }

            foreach (string key in expectedTwo.Keys)
            {
                Assert.AreEqual(subjectTwo._payload.Payload[key], expectedTwo[key]);
            }

            tracker.SetSubject(new Subject());

            return true;
        }

        // --- Event testers

        private bool ensurePageViewsWorkGet(Tracker t, MockGet g)
        {
            t.Track(new PageView()
                .SetPageUrl("http://www.example.com")
                .SetReferrer("http://www.referrer.com")
                .SetPageTitle("title page")
                .Build());
            t.Flush();

            var expectedRegex = new Regex(
                String.Format(@"http://snowplowanalytics.com/i\?e=pv&url={0}&page={1}&refr={2}&eid=[^&]+&dtm=[^&]+&tv=[^&]+&tna=testNamespace&aid=testAppId&p=pc",
                    Regex.Escape(WebUtility.UrlEncode("http://www.example.com")),
                    Regex.Escape(WebUtility.UrlEncode("title page")),
                    Regex.Escape(WebUtility.UrlEncode("http://www.referrer.com"))
                )
            );

            var actual = g.Queries[0];

            Assert.IsTrue(Uri.IsWellFormedUriString(actual, UriKind.Absolute));
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));

            return true;
        }

        private bool ensureStructEventsWorkGet(Tracker t, MockGet g)
        {
            t.Track(new Structured()
                .SetCategory("myCategory")
                .SetAction("myAction")
                .SetLabel("myLabel")
                .SetProperty("myProperty")
                .SetValue(17)
                .Build());
            t.Flush();

            var expectedRegex = new Regex(
                String.Format(@"http://snowplowanalytics.com/i\?e=se&se_ca={0}&se_ac={1}&se_la={2}&se_pr={3}&se_va={4}&eid=[^&]+&dtm=[^&]+&tv=[^&]+&tna=testNamespace&aid=testAppId&p=pc",
                    Regex.Escape(WebUtility.UrlEncode("myCategory")),
                    Regex.Escape(WebUtility.UrlEncode("myAction")),
                    Regex.Escape(WebUtility.UrlEncode("myLabel")),
                    Regex.Escape(WebUtility.UrlEncode("myProperty")),
                    Regex.Escape(WebUtility.UrlEncode("17"))
                )
            );

            var actual = g.Queries[0];

            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match regex {1}", actual, expectedRegex.ToString()));

            return true;
        }

        private bool ensureEcommerceTransactionWorksGet(Tracker t, MockGet g)
        {
            var hat = new EcommerceTransactionItem().SetSku("pbz0026").SetPrice(20).SetQuantity(1).Build();
            var shirt = new EcommerceTransactionItem().SetSku("pbz0038").SetPrice(15).SetQuantity(1).SetName("shirt").SetCategory("clothing").Build();
            var items = new List<EcommerceTransactionItem> { hat, shirt };

            t.Track(new EcommerceTransaction()
                .SetOrderId("6a8078be")
                .SetTotalValue(35)
                .SetAffiliation("affiliation")
                .SetTaxValue(3)
                .SetShipping(0)
                .SetCity("Phoenix")
                .SetState("Arizona")
                .SetCountry("US")
                .SetCurrency("USD")
                .SetItems(items)
                .Build());
            t.Flush();

            // other events may exist - lets find our best guesses for each

            var transactionActual = (from item in g.Queries
                                     where item.Contains("Phoenix")
                                     select item).ToList()[0];

            var hatActual = (from item in g.Queries
                             where item.Contains("pbz0026")
                             select item).ToList()[0];

            var shirtActual = (from item in g.Queries
                               where item.Contains("pbz0038")
                               select item).ToList()[0];

            var expectedTransaction = new Dictionary<string, string>
            {
                {"e", "tr"},
                {"tr_id", "6a8078be"},
                {"tr_tt", "35.00"},
                {"tr_af", "affiliation"},
                {"tr_tx", "3.00"},
                {"tr_sh", "0.00"},
                {"tr_ci", "Phoenix"},
                {"tr_st", "Arizona"},
                {"tr_co", "US"},
                {"tr_cu", "USD"}
            };

            assertQueryContainsDefaults(transactionActual);
            assertQueryContainsAll(transactionActual, expectedTransaction);

            var expectedHat = new Dictionary<string, string>
            {
                {"e", "ti"},
                {"ti_id", "6a8078be"},
                {"ti_sk", "pbz0026"},
                {"ti_pr", "20.00"},
                {"ti_qu", "1"},
                {"ti_cu", "USD"}
            };

            assertQueryContainsDefaults(hatActual);
            assertQueryContainsAll(hatActual, expectedHat);

            var expectedShirt = new Dictionary<string, string>
            {
                {"e", "ti"},
                {"ti_id", "6a8078be"},
                {"ti_sk", "pbz0038"},
                {"ti_nm", "shirt"},
                {"ti_ca", "clothing"},
                {"ti_pr", "15.00"},
                {"ti_qu", "1"},
                {"ti_cu", "USD"},
            };
            
            assertQueryContainsDefaults(shirtActual);
            assertQueryContainsAll(shirtActual, expectedShirt);

            return true;
        }

        private bool ensureUnstructEventGet(Tracker t, MockGet g, bool expectBase64 = false)
        {
            var eventJson = new SelfDescribingJson(
                "iglu:com.acme/test/jsonschema/1-0-0", 
                new Dictionary<string, string> {
                    { "page", "testpage" },
                    { "user", "tester" }
                }
            );

            t.Track(new SelfDescribing().SetEventData(eventJson).Build());
            t.Flush();

            var actual = g.Queries[0];

            var expectedPayload = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"",""data"":{""schema"":""iglu:com.acme/test/jsonschema/1-0-0"",""data"":{""page"":""testpage"",""user"":""tester""}}}";
            var expected = new Dictionary<string, string>
            {
                {"e", "ue"}
            };

            if (expectBase64)
            {
                string base64encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedPayload));
                expected.Add("ue_px", base64encoded);
            }
            else
            {
                expected.Add("ue_pr", expectedPayload);
            }

            assertQueryContainsDefaults(actual);
            assertQueryContainsAll(actual, expected);

            return true;
        }

        private bool ensureScreenViewWorksGet(Tracker t, MockGet g, bool expectB64 = true)
        {
            if (!expectB64)
            {
                Assert.Fail("non b64 mode not supported");
            }

            t.Track(new ScreenView().SetName("entry screen").SetId("0001").Build());
            t.Flush();

            var actual = g.Queries[0];

            var expectedJsonString = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"",""data"":{""schema"":""iglu:com.snowplowanalytics.snowplow/screen_view/jsonschema/1-0-0"",""data"":{""name"":""entry screen"",""id"":""0001""}}}";
            var expectedB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedJsonString));

            var expected = new Dictionary<string, string>
            {
                {"e", "ue"},
                {"ue_px", expectedB64}
            };
            
            assertQueryContainsDefaults(actual);
            assertQueryContainsAll(actual, expected);

            return true;
        }

        private bool ensureTimingWorksGet(Tracker t, MockGet g, bool expectB64 = true)
        {
            if (!expectB64)
            {
                Assert.Fail("non b64 mode not supported");
            }

            t.Track(new Timing().SetCategory("category").SetVariable("variable").SetTiming(5).SetLabel("label").Build());
            t.Flush();

            var actual = g.Queries[0];

            var expectedJsonString = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"",""data"":{""schema"":""iglu:com.snowplowanalytics.snowplow/timing/jsonschema/1-0-0"",""data"":{""category"":""category"",""label"":""label"",""timing"":5,""variable"":""variable""}}}";
            var expectedB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedJsonString));

            var expected = new Dictionary<string, string>
            {
                {"e", "ue"},
                {"ue_px", expectedB64}
            };

            assertQueryContainsDefaults(actual);
            assertQueryContainsAll(actual, expected);

            return true;
        }

        private bool ensureContextsWorkGet(Tracker t, MockGet g, bool useb64 = true)
        {
            Assert.IsTrue(useb64);

            var pageContext = new GenericContext()
                .SetSchema("iglu:com.snowplowanalytics.snowplow/page/jsonschema/1-0-0")
                .Add("type", "test")
                .Add("public", false)
                .Build();

            var userContext = new GenericContext()
                .SetSchema("iglu:com.snowplowanalytics.snowplow/user/jsonschema/1-0-0")
                .Add("age", 40)
                .Add("name", "ned")
                .Build();

            var contexts = new List<IContext>
            {
                pageContext,
                userContext
            };

            t.Track(new PageView().SetPageUrl("http://www.example.com").SetCustomContext(contexts).Build());
            t.Flush();

            var actual = g.Queries[0];

            var expected = new Dictionary<string, string>
            {
                {"e", "pv"},
                {"url", "http://www.example.com"}
            };

            var expectedJsonString = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/contexts/jsonschema/1-0-1"",""data"":[{""schema"":""iglu:com.snowplowanalytics.snowplow/page/jsonschema/1-0-0"",""data"":{""type"":""test"",""public"":false}},{""schema"":""iglu:com.snowplowanalytics.snowplow/user/jsonschema/1-0-0"",""data"":{""age"":40,""name"":""ned""}}]}";
            var expectedB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedJsonString));

            var extractCx = new Regex("cx=(?<cx>[^&]+)");

            Assert.IsTrue(extractCx.IsMatch(actual), "couldn't find cx in query");

            var match = extractCx.Match(actual);

            Assert.IsTrue(match.Success);

            var actualCx = match.Result("${cx}");

            Assert.AreEqual(expectedB64, actualCx);

            return true;
        }

        private bool ensureSettersWorkGet(Tracker t, MockGet g)
        {
            t.SetPlatform(Platform.Mob);
            t.SetUserId("malcolm");
            t.SetScreenResolution(100, 200);
            t.SetViewport(50, 60);
            t.SetColorDepth(24);
            t.SetTimezone("Europe London");
            t.SetLang("en");

            t.Track(new PageView()
                .SetPageUrl("http://www.example.com")
                .SetPageTitle("title page")
                .SetReferrer("http://www.referrer.com")
                .SetTrueTimestamp(1000000000000)
                .Build());
            t.Flush();

            var actual = g.Queries[0];

            var expected = new Dictionary<string, string>
            {
                {"e", "pv"},
                {"url", "http://www.example.com"},
                {"page", "title page"},
                {"refr", "http://www.referrer.com"},
                {"tv", Version.VERSION},
                {"tna", "testNamespace"},
                {"aid", "testAppId"},
                {"p", "mob"},
                {"res", "100x200"},
                {"vp", "50x60"},
                {"cd", "24"},
                {"tz", "Europe London"},
                {"lang", "en"},
                {"ttm", "1000000000000"}
            };

            foreach (var k in expected.Keys)
            {
                var matcher = new Regex("[&?]" + k + "=(?<value>[^&]+)");
                var match = matcher.Match(actual);

                Assert.IsTrue(match.Success, "Couldn't find key " + k + " in query " + actual);

                var found = match.Result("${value}");
                string decoded = WebUtility.UrlDecode(found);

                Assert.AreEqual(expected[k], decoded, "key " + k + " has the wrong value " + decoded);
            }

            return true;
        }

        // --- Tests

        [TestMethod]
        public void testTracker()
        {
            var storage = new LiteDBStorage(_testDbFilename);

            Assert.AreEqual(0, storage.TotalItems);

            var queue = new PersistentBlockingQueue(storage, new PayloadToJsonString());

            var getRequestMock = new MockGet();
            var postRequestMock = new MockPost();

            var endpoint = new SnowplowHttpCollectorEndpoint("snowplowanalytics.com",
                                                             postMethod: new SnowplowHttpCollectorEndpoint.PostDelegate(postRequestMock.HttpPost),
                                                             getMethod: new SnowplowHttpCollectorEndpoint.GetDelegate(getRequestMock.HttpGet));

            Assert.IsFalse(tracker.Started);

            tracker.Start(new AsyncEmitter(endpoint, queue, sendLimit: 1), trackerNamespace: "testNamespace", appId: "testAppId", encodeBase64: false);

            Assert.IsTrue(tracker.Started);
            Assert.IsTrue(ensureSubjectSet(tracker));
            Assert.IsTrue(ensurePageViewsWorkGet(tracker, getRequestMock));
            Assert.IsTrue(ensureStructEventsWorkGet(tracker, getRequestMock));
            Assert.IsTrue(ensureEcommerceTransactionWorksGet(tracker, getRequestMock));
            Assert.IsTrue(ensureUnstructEventGet(tracker, getRequestMock));

            tracker.Stop();

            Assert.IsFalse(tracker.Started);

            // Restart with base64 on
            tracker.Start(new AsyncEmitter(endpoint, queue, sendLimit: 1), trackerNamespace: "testNamespace", appId: "testAppId", encodeBase64: true);

            Assert.IsTrue(tracker.Started);
            Assert.IsTrue(ensureUnstructEventGet(tracker, getRequestMock, true));
            Assert.IsTrue(ensureScreenViewWorksGet(tracker, getRequestMock, true));
            Assert.IsTrue(ensureTimingWorksGet(tracker, getRequestMock, true));
            Assert.IsTrue(ensureContextsWorkGet(tracker, getRequestMock, true));
            Assert.IsTrue(ensureSettersWorkGet(tracker, getRequestMock));

            tracker.Stop();

            Assert.IsFalse(tracker.Started);
        }

        [TestInitialize]
        public void cleanupDb()
        {
            if (File.Exists(_testDbFilename))
            {
                File.Delete(_testDbFilename);
            }

            if (File.Exists(_testDbJournalFilename))
            {
                File.Delete(_testDbJournalFilename);
            }
        }

        [TestCleanup]
        public void stopTracker()
        {
            tracker.Stop();
            tracker = null;
        }
    }
}
