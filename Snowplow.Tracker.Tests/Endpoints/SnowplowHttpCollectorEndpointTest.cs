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
using Snowplow.Tracker.Endpoints;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Snowplow.Tracker.Models;
using static Snowplow.Tracker.Endpoints.SnowplowHttpCollectorEndpoint;
using System;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Tests.Endpoints
{
    [TestClass]
    public class SnowplowHttpCollectorEndpointTest
    {
        class MockGet
        {
            public List<string> Queries { get; private set; } = new List<string>();
            public int StatusCode { get; set; } = 200;

            public RequestResult HttpGet(string uri, bool oversize, List<string> itemIds)
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

            public RequestResult HttpPost(string uri, string postData, bool oversize, List<string> itemIds)
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

        [TestMethod]
        public void testSendGetRequestGoodHttp()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("hello", "world");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.SuccessIds.Count == 1);
            Assert.IsTrue(getReq.Queries.Count == 1);

            var actual = getReq.Queries[0];
            var expectedRegex = new Regex("http://somewhere\\.com/i\\?hello=world&stm=[0-9]{13}");
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));
        }

        [TestMethod]
        public void testSendGetRequestGoodHttps()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("hello", "world");
            payload.Add("ts", "123");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));
            
            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.SuccessIds.Count == 1);
            Assert.IsTrue(getReq.Queries.Count == 1);

            var actual = getReq.Queries[0];
            var expectedRegex = new Regex("https://somewhere\\.com/i\\?hello=world&ts=123&stm=[0-9]{13}");
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));
        }

        [TestMethod]
        public void testSendGetRequestNoResponseHttps()
        {
            var getReq = new MockGet() { StatusCode = 404 };
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("hello", "world");
            payload.Add("ts", "123");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.FailureIds.Count == 1);
            Assert.IsTrue(getReq.Queries.Count == 1);

            var actual = getReq.Queries[0];
            var expectedRegex = new Regex("https://somewhere\\.com/i\\?hello=world&ts=123&stm=[0-9]{13}");
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));
        }

        [TestMethod]
        public void testSendGetRequestNon200ResponseHttps()
        {
            var getReq = new MockGet() { StatusCode = 500 };
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("hello", "world");
            payload.Add("ts", "123");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.FailureIds.Count == 1);
            Assert.IsTrue(getReq.Queries.Count == 1);

            var actual = getReq.Queries[0];
            var expectedRegex = new Regex("https://somewhere\\.com/i\\?hello=world&ts=123&stm=[0-9]{13}");
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));
        }

        [TestMethod]
        public void testGetIgnoreSchemePathQueryInUri()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("something://somewhere.com/things?1=1", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("sample", "value");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.SuccessIds.Count == 1);
            Assert.IsTrue(getReq.Queries.Count == 1);

            var actual = getReq.Queries[0];
            var expectedRegex = new Regex("https://somewhere\\.com/i\\?sample=value&stm=[0-9]{13}");
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));
        }

        [TestMethod]
        public void testGetParametersEncoded()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("<", ">");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.SuccessIds.Count == 1);
            Assert.IsTrue(getReq.Queries.Count == 1);

            var actual = getReq.Queries[0];
            var expectedRegex = new Regex("https://somewhere\\.com/i\\?%3C=%3E&stm=[0-9]{13}");
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));
        }

        [TestMethod]
        public void testGetPortIsSet()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet), port: 999);
            var payload = new Payload();
            payload.Add("foo", "bar");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.SuccessIds.Count == 1);
            Assert.IsTrue(getReq.Queries.Count == 1);

            var actual = getReq.Queries[0];
            var expectedRegex = new Regex("https://somewhere\\.com:999/i\\?foo=bar&stm=[0-9]{13}");
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));
        }

        [TestMethod]
        public void testPostHttpGood()
        {
            var postReq = new MockPost();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, method: HttpMethod.POST, postMethod: new PostDelegate(postReq.HttpPost));
            var payload = new Payload();
            payload.Add("foo", "bar");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.SuccessIds.Count == 1);
            Assert.IsTrue(postReq.Queries.Count == 1);
            Assert.AreEqual(@"https://somewhere.com/com.snowplowanalytics.snowplow/tp2", postReq.Queries[0].Uri);

            var actual = postReq.Queries[0].PostData;
            var expectedRegex = new Regex("{\\\"schema\\\":\\\"iglu:com\\.snowplowanalytics\\.snowplow/payload_data/jsonschema/1-0-4\\\",\\\"data\\\":\\[{\\\"foo\\\":\\\"bar\\\",\\\"stm\\\":\\\"[0-9]{13}\\\"}\\]}");
            Assert.IsTrue(expectedRegex.Match(actual).Success, String.Format("{0} doesn't match {1}", actual, expectedRegex.ToString()));
        }

        [TestMethod]
        public void testPostHttpNoResponse()
        {
            var postReq = new MockPost() { StatusCode = 404 };
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, method: HttpMethod.POST, postMethod: new PostDelegate(postReq.HttpPost));
            var payload = new Payload();

            payload.Add("foo", "bar");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.AreEqual(1, sendResult.FailureIds.Count);
            Assert.AreEqual(1, postReq.Queries.Count);
        }

        [TestMethod]
        public void testPostHttpNon200Response()
        {
            var postReq = new MockPost() { StatusCode = 404 };
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, method: HttpMethod.POST, postMethod: new PostDelegate(postReq.HttpPost));
            var payload = new Payload();
            payload.Add("foo", "bar");

            var sendList = new List<Tuple<string, Payload>>();
            sendList.Add(Tuple.Create("0", payload));

            var sendResult = endpoint.Send(sendList);

            Assert.IsTrue(sendResult.FailureIds.Count == 1);
            Assert.IsTrue(postReq.Queries.Count == 1);
        }

        [TestMethod]
        [Ignore]
        public void testHttpPostPingSever()
        {
            // test the post method against an endpoint - skipped because it's really a manual test
            string returns200 = "http://requestb.in/sqt0d1sq";
            string returns403 = "http://snowplowanalytics.com/nothere/ok";
            string cannotConnect = "http://localhost:1231";

            var resp = SnowplowHttpCollectorEndpoint.HttpPost(returns200, @"[{""foo"":""bar""}]", false, new List<string> { "0" });
            Assert.AreEqual(200, resp.StatusCodeTask.Result);

            var bad = SnowplowHttpCollectorEndpoint.HttpPost(returns403, @"[{""foo"":""bar""}]", false, new List<string> { "0" });
            Assert.AreEqual(403, bad.StatusCodeTask.Result);

            var nowhere = SnowplowHttpCollectorEndpoint.HttpPost(cannotConnect, @"[{""foo"":""bar""}]", false, new List<string> { "0" });
            Assert.AreEqual(-1, nowhere.StatusCodeTask.Result);
        }

        [TestMethod]
        [Ignore]
        public void testHttpGetPingServer()
        {
            // test a get method against the endpoint - skipped because it really pings the server
            string returns200 = "http://snowplowanalytics.com";
            string returns404 = "http://snowplowanalytics.com/nothere/ok";
            string cannotConnect = "http://localhost:1231";

            var resp = SnowplowHttpCollectorEndpoint.HttpGet(returns200, false, new List<string> { "0" });
            Assert.AreEqual(200, resp.StatusCodeTask.Result);

            var bad = SnowplowHttpCollectorEndpoint.HttpGet(returns404, false, new List<string> { "0" });
            Assert.AreEqual(404, bad.StatusCodeTask.Result);

            var nowhere = SnowplowHttpCollectorEndpoint.HttpGet(cannotConnect, false, new List<string> { "0" });
            Assert.AreEqual(-1, nowhere.StatusCodeTask.Result);
        }
    }
}
