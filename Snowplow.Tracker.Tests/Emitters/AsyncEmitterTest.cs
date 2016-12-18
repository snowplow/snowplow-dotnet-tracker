using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Emitters.Endpoints;
using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Tests.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Tests.Emitters
{
    [TestClass]
    public class AsyncEmitterTest
    {
        class MockEndpoint : IEndpoint
        {
            public bool Send(Payload p)
            {
                return true;
            }
        }

        private AsyncEmitter buildMockEmitter()
        {
            var q = new PersistentBlockingQueue(new MockStorage(), new PayloadToJsonString());
            AsyncEmitter e = new AsyncEmitter(new MockEndpoint(), q);

            return e;
        }

        [TestMethod]
        public void testEmitterStartStop()
        {
            var e = buildMockEmitter();

            e.Start();
            e.Stop();

            Assert.IsFalse(e.Running);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
                           @"Cannot start - already started")]
        public void testEmitterStartAlreadyStarted()
        {
            var e = buildMockEmitter();

            e.Start();

            try
            {
                e.Start();
            } finally
            {
                e.Stop();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
                   @"Cannot stop - already stopped")]
        public void testEmitterStopAlreadyStopped()
        {
            var e = buildMockEmitter();

            e.Start();
            e.Stop();

            e.Stop();
        }

        [TestMethod] 
        public void testEmitterRestart()
        {
            var e = buildMockEmitter();

            e.Start();
            Assert.IsTrue(e.Running);
            e.Stop();
            Assert.IsFalse(e.Running);
            e.Start();
            Assert.IsTrue(e.Running);
            e.Stop();
            Assert.IsFalse(e.Running);
        }

    }
}
