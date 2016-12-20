using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Tests.Logging
{
    [TestClass]
    public class ConsoleLoggerTest
    {

        [TestMethod]
        public void testFormatting()
        {
            Exception e;
            try
            {
                throw new Exception("Broken");
            } catch (Exception x)
            {
                e = x;
            }
            var log = ConsoleLogger.FormatMessage("test", "message", e);
            Assert.AreEqual("test: message\n" + e.ToString() + "\n" + e.StackTrace.ToString(), log);
        }

    }
}
