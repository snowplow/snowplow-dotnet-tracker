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

using System;

namespace Snowplow.Tracker.Logging
{
    public class ConsoleLogger : ILogger
    {

        /// <summary>
        /// Format a message - type: message, then the stacktrace of any exception
        /// </summary>
        /// <param name="type">String to start - e.g. "Info"</param>
        /// <param name="message">Description of message</param>
        /// <param name="e">A stacktrace/exception to attach</param>
        /// <returns></returns>
        public static string FormatMessage(string type, string message, Exception e = null)
        {
            var content = String.Format("{0}: {1}",
                                        type,
                                        message);

            if (e != null && e.StackTrace != null)
            {
                content += String.Format("\n{0}\n{1}", e.ToString(), e.StackTrace.ToString());
            }

            return content;
        }

        /// <summary>
        /// Write an Error: message [stacktrace] message to the Console
        /// </summary>
        /// <param name="message">Description of problem</param>
        /// <param name="e">Exception causing problem</param>
        public void Error(string message, Exception e)
        {
            Console.WriteLine(FormatMessage("Error", message, e));
        }

        /// <summary>
        /// Write an Info: message string to the Console
        /// </summary>
        /// <param name="message">Description of action</param>
        public void Info(string message)
        {
            Console.WriteLine(FormatMessage("Info", message, null));
        }

        /// <summary>
        ///  Write an Error: message [stacktrace] message to the Console
        /// </summary>
        /// <param name="message">Description of the problem</param>
        /// <param name="e">Optionally, a stacktrace to attach</param>
        public void Warn(string message, Exception e = null)
        {
            Console.WriteLine(FormatMessage("Warning", message, e));
        }
    }
}
