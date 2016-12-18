using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Emitters.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Snowplow.Tracker.Emitters.Endpoints.SnowplowHttpCollectorEndpoint;

namespace Snowplow.Tracker.Tests.Endpoints
{
    [TestClass]
    public class SnowplowHttpCollectorEndpointTest
    {

        class MockGet
        {
            public List<string> Queries { get; private set; } = new List<string>();
            public int? ReturnValue { get; set; } = 200;

            public int? HttpGet(string uri)
            {
                Queries.Insert(0, uri);
                return ReturnValue;
            }
        }

        class PostRequest
        {
            public string Uri { get; set; }
            public string PostData { get; set;  }
        }

        class MockPost
        {
            public List<PostRequest> Queries { get; private set; } = new List<PostRequest>();
            public int? Response { get; set; } = 200;

            public int? HttpPost(string uri, string postData)
            {
                var postRec = new PostRequest { Uri = uri, PostData = postData };
                Queries.Insert(0, postRec);
                return Response;
            }
        }

        [TestMethod]
        public void testSendGetRequestGoodHttp()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("hello", "world");

            var resp = endpoint.Send(payload);

            Assert.IsTrue(resp);
            Assert.IsTrue(getReq.Queries.Count == 1);
            Assert.AreEqual(@"http://somewhere.com/i?hello=world", getReq.Queries[0]);
        }


        [TestMethod]
        public void testSendGetRequestGoodHttps()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("hello", "world");
            payload.Add("ts", "123");

            var resp = endpoint.Send(payload);

            Assert.IsTrue(resp);
            Assert.IsTrue(getReq.Queries.Count == 1);
            Assert.AreEqual(@"https://somewhere.com/i?hello=world&ts=123", getReq.Queries[0]);
        }
        
        [TestMethod]
        public void testSendGetRequestNoResponseHttps()
        {
            var getReq = new MockGet() { ReturnValue = null };
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("hello", "world");
            payload.Add("ts", "123");

            var resp = endpoint.Send(payload);

            Assert.IsFalse(resp);
            Assert.IsTrue(getReq.Queries.Count == 1);
            Assert.AreEqual(@"https://somewhere.com/i?hello=world&ts=123", getReq.Queries[0]);
        }

        [TestMethod]
        public void testSendGetRequestNon200ResponseHttps()
        {
            var getReq = new MockGet() { ReturnValue = 500 };
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("hello", "world");
            payload.Add("ts", "123");

            var resp = endpoint.Send(payload);

            Assert.IsFalse(resp);
            Assert.IsTrue(getReq.Queries.Count == 1);
            Assert.AreEqual(@"https://somewhere.com/i?hello=world&ts=123", getReq.Queries[0]);
        }

        [TestMethod] 
        public void testGetIgnoreSchemePathQueryInUri()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("something://somewhere.com/things?1=1", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("sample", "value");
            var resp = endpoint.Send(payload);

            Assert.IsTrue(resp);
            Assert.IsTrue(getReq.Queries.Count == 1);
            Assert.AreEqual(@"https://somewhere.com/i?sample=value", getReq.Queries[0]);
        }

        [TestMethod]
        public void testGetParametersEncoded()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet));
            var payload = new Payload();
            payload.Add("<", ">");
            var resp = endpoint.Send(payload);

            Assert.IsTrue(resp);
            Assert.IsTrue(getReq.Queries.Count == 1);
            Assert.AreEqual(@"https://somewhere.com/i?%3C=%3E", getReq.Queries[0]);
        }

        [TestMethod]
        public void testGetPortIsSet()
        {
            var getReq = new MockGet();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, getMethod: new GetDelegate(getReq.HttpGet), port: 999);
            var payload = new Payload();
            payload.Add("foo", "bar");
            var resp = endpoint.Send(payload);

            Assert.IsTrue(resp);
            Assert.IsTrue(getReq.Queries.Count == 1);
            Assert.AreEqual(@"https://somewhere.com:999/i?foo=bar", getReq.Queries[0]);
        }

        [TestMethod]
        public void testPostHttpGood()
        {
            var postReq = new MockPost();
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, method: HttpMethod.POST, postMethod: new PostDelegate(postReq.HttpPost));
            var payload = new Payload();

            payload.Add("foo", "bar");

            var resp = endpoint.Send(payload);

            Assert.IsTrue(resp);
            Assert.IsTrue(postReq.Queries.Count == 1);
            Assert.AreEqual(@"https://somewhere.com/com.snowplowanalytics.snowplow/tp2", postReq.Queries[0].Uri);
            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-0"",""data"":[{""foo"":""bar""}]}", postReq.Queries[0].PostData);
        }

        [TestMethod]
        public void testPostHttpNoResponse()
        {
            var postReq = new MockPost() { Response = null };
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, method: HttpMethod.POST, postMethod: new PostDelegate(postReq.HttpPost));
            var payload = new Payload();

            payload.Add("foo", "bar");

            var resp = endpoint.Send(payload);

            Assert.IsFalse(resp);
            Assert.IsTrue(postReq.Queries.Count == 1);
        }


        [TestMethod]
        public void testPostHttpNon200Response()
        {
            var postReq = new MockPost() { Response = 404 };
            var endpoint = new SnowplowHttpCollectorEndpoint("somewhere.com", HttpProtocol.HTTPS, method: HttpMethod.POST, postMethod: new PostDelegate(postReq.HttpPost));
            var payload = new Payload();

            payload.Add("foo", "bar");

            var resp = endpoint.Send(payload);

            Assert.IsFalse(resp);
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

            var resp = SnowplowHttpCollectorEndpoint.HttpPost(returns200, @"[{""foo"":""bar""}]");
            Assert.AreEqual(200, resp);

            var bad = SnowplowHttpCollectorEndpoint.HttpPost(returns403, @"[{""foo"":""bar""}]");
            Assert.AreEqual(403, bad);

            var nowhere = SnowplowHttpCollectorEndpoint.HttpPost(cannotConnect, @"[{""foo"":""bar""}]");
            Assert.IsNull(nowhere);
        }

        [TestMethod]
        [Ignore]
        public void testHttpGetPingServer()
        {
            // test a get method against the endpoint - skipped because it really pings the server
            string returns200 = "http://snowplowanalytics.com";
            string returns404 = "http://snowplowanalytics.com/nothere/ok";
            string cannotConnect = "http://localhost:1231";

            var resp = SnowplowHttpCollectorEndpoint.HttpGet(returns200);
            Assert.AreEqual(200, resp);

            var bad = SnowplowHttpCollectorEndpoint.HttpGet(returns404);
            Assert.AreEqual(404, bad);

            var nowhere = SnowplowHttpCollectorEndpoint.HttpGet(cannotConnect);
            Assert.IsNull(nowhere);
        }

    }
}
