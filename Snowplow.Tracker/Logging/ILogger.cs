using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Logging
{
    public interface ILogger
    {
        void Info(string message);
        void Warn(string message, Exception e = null);
        void Error(string message, Exception e);
    }
}
