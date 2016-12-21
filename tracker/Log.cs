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
using NLog;

namespace Snowplow.Tracker
{
    public static class Log
    {
        private static Logger logger = LogManager.GetLogger("Snowplow.Tracker");

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
                return logger;
            }
        }

        /// <summary>
        /// Set the level at which messages will be logged
        /// </summary>
        /// <param name="newLevel">Trace, Debug, Info, Warn, Error, Fatal, or Off</param>
        [Obsolete("SetLogLevel is deprecated, please use NLog.config instead.", true)]
        public static void SetLogLevel(Level newLevel)
        {
            // All log settings must be configured from the NLog.config file
        }
    }
}
