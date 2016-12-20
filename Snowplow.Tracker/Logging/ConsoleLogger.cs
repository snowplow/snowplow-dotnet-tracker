using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Logging
{
    public class ConsoleLogger : ILogger
    {

        public static string FormatMessage(string type, string message, Exception e = null)
        {
            var content = String.Format("{0}: {1}",
                                        type,
                                        message);

            if (e!=null && e.StackTrace != null)
            {
                content += String.Format("\n{0}\n{1}", e.ToString(), e.StackTrace.ToString());
            }

            return content;
        }

        public void Error(string message, Exception e)
        {
            Console.WriteLine(FormatMessage("Error", message, e));
        }

        public void Info(string message)
        {
            Console.WriteLine(FormatMessage("Info", message, null));
        }

        public void Warn(string message, Exception e = null)
        {
            Console.WriteLine(FormatMessage("Warning", message, e));
        }
    }
}
