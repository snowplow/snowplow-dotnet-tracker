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
using System.Text;
using System.Web;
using System.Messaging;
using System.Net;
using System.Net.Fakes;
using Microsoft.QualityTools.Testing.Fakes;

namespace Snowplow.Tracker.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        private static List<NameValueCollection> payloads = new List<NameValueCollection>();
        private static FakesDelegates.Func<HttpWebRequest, WebResponse> fake = (request) => {
            var pairs = HttpUtility.ParseQueryString(request.RequestUri.Query);
            payloads.Add(pairs);
            var responseShim = new ShimHttpWebResponse();
            responseShim.StatusCodeGet = () => HttpStatusCode.OK;
            return responseShim;
        };
        private static FakesDelegates.Func<HttpWebRequest, WebResponse> badFake = (request) =>
        {
            var pairs = HttpUtility.ParseQueryString(request.RequestUri.Query);
            var responseShim = new ShimHttpWebResponse();
            throw new WebException();
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
                t.TrackPageView("http://www.example.com", "title page", "http://www.referrer.com");
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
                t.TrackStructEvent("myCategory", "myAction", "myLabel", "myProperty", 17);
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
                t.TrackEcommerceTransaction("6a8078be", 35, "affiliation", 3, 0, "Phoenix", "Arizona", "US", "USD", items);
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

        [TestMethod]
        public void testTrackUnstructEventNonBase64()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net", null, null, null, false);
                var eventJson = new Dictionary<string, object>
                {
                    {"schema", "iglu:com.acme/test/jsonschema/1-0-0"},
                    {"data", new Dictionary<string, string>
                    {
                        { "page", "testpage" },
                        { "user", "tester" }
                    }
                }
                };
                t.TrackUnstructEvent(eventJson);
                var expected = new Dictionary<string, string>
                {
                    {"e", "ue"}
                };
                checkResult(expected, payloads[payloads.Count - 1]);
                var expectedJsonString = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"",""data"":{""schema"":""iglu:com.acme/test/jsonschema/1-0-0"",""data"":{""page"":""testpage"",""user"":""tester""}}}";
                string actualJsonString = payloads[payloads.Count - 1]["ue_pr"];
                Assert.AreEqual(expectedJsonString, actualJsonString);
            }
        }

        [TestMethod]
        public void testTrackUnstructEventBase64()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net");
                var eventJson = new Dictionary<string, object>
                {
                    {"schema", "iglu:com.acme/test/jsonschema/1-0-0"},
                    {"data", new Dictionary<string, string>
                    {
                        { "page", "testpage" },
                        { "user", "tester" }
                    }
                }
                };
                t.TrackUnstructEvent(eventJson);
                var expected = new Dictionary<string, string>
                {
                    {"e", "ue"}
                };
                checkResult(expected, payloads[payloads.Count - 1]);
                var expectedJsonString = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"",""data"":{""schema"":""iglu:com.acme/test/jsonschema/1-0-0"",""data"":{""page"":""testpage"",""user"":""tester""}}}";
                byte[] data = Convert.FromBase64String(payloads[payloads.Count - 1]["ue_px"]);
                string actualJsonString = Encoding.UTF8.GetString(data);
                Assert.AreEqual(expectedJsonString, actualJsonString);
            }
        }

        [TestMethod]
        public void testTrackScreenView()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net");
                t.TrackScreenView("entry screen", "0001");
                var expected = new Dictionary<string, string>
                {
                    {"e", "ue"}
                };
                checkResult(expected, payloads[payloads.Count - 1]);
                var expectedJsonString = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0"",""data"":{""schema"":""iglu:com.snowplowanalytics.snowplow/screen_view/jsonschema/1-0-0"",""data"":{""name"":""entry screen"",""id"":""0001""}}}";
                byte[] data = Convert.FromBase64String(payloads[payloads.Count - 1]["ue_px"]);
                string actualJsonString = Encoding.UTF8.GetString(data);
                Assert.AreEqual(expectedJsonString, actualJsonString);
            }
        }

        [TestMethod]
        public void testSetterMethods()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net", null, "cf", "train simulator");
                t.SetPlatform(Platform.Mob);
                t.SetUserId("malcolm");                
                t.SetScreenResolution(100, 200);
                t.SetViewport(50, 60);
                t.SetColorDepth(24);
                t.SetTimezone("Europe London");
                t.SetLang("en");
                t.TrackPageView("http://www.example.com", "title page", "http://www.referrer.com", null, 1000000000000);
                var expected = new Dictionary<string, string>
                {
                    {"e", "pv"},
                    {"url", "http://www.example.com"},
                    {"page", "title page"},
                    {"refr", "http://www.referrer.com"},
                    {"tv", "cs-0.1.0"},
                    {"tna", "cf"},
                    {"aid", "train simulator"},
                    {"p", "mob"},
                    {"res", "100x200"},
                    {"vp", "50x60"},
                    {"cd", "24"},
                    {"tz", "Europe London"},
                    {"lang", "en"},
                    {"dtm", "1000000000000"}
                };

                checkResult(expected, payloads[payloads.Count - 1]);
            }
        }

        [TestMethod]
        public void testContext()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var pageContext = new Dictionary<string, object>
                {
                    {"schema", "iglu:com.snowplowanalytics.snowplow/page/jsonschema/1-0-0"},
                    {"data", new Dictionary<string, object>
                        {
                            { "type", "test" },
                            { "public", false }
                        }
                    }
                };

                var userContext = new Dictionary<string, object>
                {
                    {"schema", "iglu:com.snowplowanalytics.snowplow/user/jsonschema/1-0-0"},
                    {"data", new Dictionary<string, object>
                        {
                            { "age", 40 },
                            { "name", "Ned" }
                        }
                    }
                };

                var context = new List<Dictionary<string, object>>
                {
                    pageContext,
                    userContext
                };

                var t = new Tracker("d3rkrsqld9gmqf.cloudfront.net");
                t.TrackPageView("http://www.example.com", null, null, context);
                var expected = new Dictionary<string, string>
                {
                    {"e", "pv"},
                    {"url", "http://www.example.com"}
                };
                checkResult(expected, payloads[payloads.Count - 1]);
                var expectedJsonString = @"{""schema"":""iglu:com.snowplowanalytics.snowplow/contexts/1-0-0"",""data"":[{""schema"":""iglu:com.snowplowanalytics.snowplow/page/jsonschema/1-0-0"",""data"":{""type"":""test"",""public"":false}},{""schema"":""iglu:com.snowplowanalytics.snowplow/user/jsonschema/1-0-0"",""data"":{""age"":40,""name"":""Ned""}}]}";
                byte[] data = Convert.FromBase64String(payloads[payloads.Count - 1]["cx"]);
                string actualJsonString = Encoding.UTF8.GetString(data);
                Assert.AreEqual(expectedJsonString, actualJsonString);
            }
        }

        [TestMethod]
        public void testOnSuccess()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                int successes = -1;
                var e = new Emitter("d3rkrsqld9gmqf.cloudfront.net", HttpProtocol.HTTP, null, HttpMethod.GET, 2, (successCount) =>
                {
                    successes = successCount;
                });
                var t = new Tracker(e);
                t.TrackPageView("first");
                t.TrackPageView("second");
                Assert.AreEqual(2, successes);
            }
        }

        [TestMethod]
        public void testOnFailure()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = badFake;

                int? successes = null;
                List<Dictionary<string, string>> failureList = null;
                var e = new Emitter("d3rkrsqld9gmqf.cloudfront.net", HttpProtocol.HTTP, null, HttpMethod.GET, 2, null, (successCount, failures) =>
                {
                    successes = successCount;
                    failureList = failures;
                });
                var t = new Tracker(e);
                t.TrackPageView("first");
                t.TrackPageView("second");
                Assert.AreEqual(0, successes);
                Assert.AreEqual("first", failureList[0]["url"]);
                Assert.AreEqual("second", failureList[1]["url"]);
            }
        }

        [TestMethod]
        public void testAsyncTrackPageView()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;

                var t = new Tracker(new AsyncEmitter("d3rkrsqld9gmqf.cloudfront.net"));
                t.TrackPageView("http://www.example.com", "title page", "http://www.referrer.com");
                var expected = new Dictionary<string, string>
                {
                    {"e", "pv"},
                    {"url", "http://www.example.com"},
                    {"page", "title page"},
                    {"refr", "http://www.referrer.com"}
                };
                t.Flush(true);
                checkResult(expected, payloads[payloads.Count - 1]);
            }
        }

        [TestMethod]
        public void testAsyncPostOnSuccess()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = fake;
                int successes = -1;
                var e = new AsyncEmitter("d3rkrsqld9gmqf.cloudfront.net", HttpProtocol.HTTP, null, HttpMethod.POST, 10, (successCount) =>
                {
                    successes = successCount;
                });
                var t = new Tracker(e);
                t.TrackPageView("first");
                t.TrackPageView("second");
                t.Flush(true);
                Assert.AreEqual(2, successes);
            }
        }

        [TestMethod]
        public void testAsyncPostOnFailure()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = badFake;

                int? successes = null;
                List<Dictionary<string, string>> failureList = null;
                var e = new AsyncEmitter("d3rkrsqld9gmqf.cloudfront.net", HttpProtocol.HTTP, null, HttpMethod.POST, 10, null, (successCount, failures) =>
                {
                    successes = successCount;
                    failureList = failures;
                });
                var t = new Tracker(e);
                t.TrackPageView("first");
                t.TrackPageView("second");
                t.Flush(true);
                Assert.AreEqual(0, successes);
                Assert.AreEqual("first", failureList[0]["url"]);
                Assert.AreEqual("second", failureList[1]["url"]);
            }
        }

        [TestMethod]
        public void testOfflineTracking()
        {
            var defaultPath = @".\private$\Snowplow.Tracker";
            if (MessageQueue.Exists(defaultPath))
            {
                MessageQueue.Delete(defaultPath);
            }

            var emitter1 = new Emitter("d3rkrsqld9gmqf.cloudfront.net");

            var t = new Tracker(new List<IEmitter> { emitter1 });
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ShimHttpWebRequest.AllInstances.GetResponse = badFake;

                t.TrackStructEvent("msmqCategory1", "msmqAction1");

                ShimHttpWebRequest.AllInstances.GetResponse = fake;
                t.TrackStructEvent("msmqCategory2", "msmqAction2");

                var expected1 = new Dictionary<string, string>
                {
                    {"e", "se"},
                    {"se_ca", "msmqCategory1"},
                    {"se_ac", "msmqAction1"}
                };

                var expected2 = new Dictionary<string, string>
                {
                    {"e", "se"},
                    {"se_ca", "msmqCategory2"},
                    {"se_ac", "msmqAction2"}
                };

                checkResult(expected2, payloads[payloads.Count - 2]);
                checkResult(expected1, payloads[payloads.Count - 1]);
            }
        }

    }
}
