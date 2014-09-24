/*
 * Log.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace Snowplow.Tracker
{
    public static class Log
    {
        private static Logger logger = LogManager.GetLogger("Snowplow.Tracker");
        private static ColoredConsoleTarget logTarget = new ColoredConsoleTarget();
        private static LoggingRule loggingRule = new LoggingRule("*", LogLevel.Info, logTarget);
        private static bool loggingConfigured = false;

        public enum Level
        {
            Trace = 0,
            Debug = 1,
            Info = 2,
            Warn = 3,
            Error = 4,
            Fatal = 5,
            Off = 6
        }
        
        public static Logger Logger
        {
            get
            {
                if (! loggingConfigured)
                {
                    logTarget.Layout = "${longdate} ${level} ${logger}: ${message} ${exception:format=tostring}";
                    LogManager.Configuration.LoggingRules.Add(loggingRule);
                    loggingConfigured = true;
                    SetLogLevel(Level.Info);
                    loggingConfigured = true;
                }
                return logger;
            }
        }

        /// <summary>
        /// Set the level at which messages will be logged
        /// </summary>
        /// <param name="newLevel">Trace, Debug, Info, Warn, Error, Fatal, or Off</param>
        public static void SetLogLevel(Level newLevel)
        {
            foreach (int level in Enumerable.Range(0, 6))
            {
                if (level < (int)newLevel)
                {
                    loggingRule.DisableLoggingForLevel(LogLevel.FromOrdinal(level));
                }
                else
                {
                    loggingRule.EnableLoggingForLevel(LogLevel.FromOrdinal(level));
                }
            }
            LogManager.ReconfigExistingLoggers();
        }
    }
}
