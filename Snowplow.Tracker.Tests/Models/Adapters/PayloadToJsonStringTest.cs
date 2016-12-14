using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Models.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Tests.Models.Adapters
{

    [TestClass]
    public class PayloadToJsonStringTest
    {

        [TestMethod]
        public void testToStringGoodJson()
        {
            var payload = new Payload();
            payload.Add("name", "value");

            var p = new PayloadToJsonString();

            var serialized = p.ToString(payload);

            Assert.AreEqual(@"{""NvPairs"":{""name"":""value""}}", serialized);
        }

        [TestMethod]
        public void testFromStringGoodJson()
        {
            var p = new PayloadToJsonString();

            var actual = p.FromString(@"{""NvPairs"":{""name"":""value""}}");

            var expected = new Payload();
            expected.Add("name", "value");

            CollectionAssert.AreEqual(expected.NvPairs, actual.NvPairs);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException),
                           @"Invalid JSON: ""{""")]
        public void testFromStringBadJson()
        {
            var p = new PayloadToJsonString();
            p.FromString(@"{");
        }

    }
}
