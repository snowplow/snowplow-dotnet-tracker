using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Logging
{
    public class NoLogging : ILogger
    {
        public void Error(string message, Exception e)
        {

        }

        public void Info(string message)
        {

        }

        public void Warn(string message, Exception e = null)
        {

        }
    }
}
