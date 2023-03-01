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
    public class NoLogging : ILogger
    {
        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="message">Ignored</param>
        /// <param name="e">Ignored</param>
        public void Error(string message, Exception e)
        {

        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="message">Ignored</param>
        public void Info(string message)
        {

        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="message">Ignored</param>
        /// <param name="e">Ignored</param>
        public void Warn(string message, Exception e = null)
        {

        }
    }
}
