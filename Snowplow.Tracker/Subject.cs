/*
 * Subject.cs
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

namespace Snowplow.Tracker
{
    public class Subject
    {
        public Dictionary<string, string> nvPairs;

        /// <summary>
        /// Create a subject representing a user
        /// </summary>
        public Subject()
        {
            nvPairs = new Dictionary<string, string>
            {
                { "p", "pc" }
            };
        }

        /// <summary>
        /// Set the user's platform
        /// </summary>
        /// <param name="value">The platform</param>
        /// <returns>this</returns>
        public Subject SetPlatform(Platform value)
        {
            nvPairs["p"] = value.ToString().ToLower();
            return this;
        }

        /// <summary>
        /// Set a unique ID for the user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>this</returns>
        public Subject SetUserId(string id)
        {
            nvPairs["uid"] = id;
            return this;
        }


        /// <summary>
        /// Set the user's screen resolution
        /// </summary>
        /// <param name="width">Screen width in pixels</param>
        /// <param name="height">Screen height in pixels</param>
        /// <returns>this</returns>
        public Subject SetScreenResolution(int width, int height)
        {
            nvPairs["res"] = String.Format("{0}x{1}", width.ToString(), height.ToString());
            return this;
        }

        /// <summary>
        /// Set the user's viewport dimensions
        /// </summary>
        /// <param name="width">Viewport width in pixels</param>
        /// <param name="height">Viewport height in pixels</param>
        /// <returns>this</returns>
        public Subject SetViewport(int width, int height)
        {
            nvPairs["vp"] = String.Format("{0}x{1}", width.ToString(), height.ToString());
            return this;
        }

        /// <summary>
        /// Set the user device's color depth
        /// </summary>
        /// <param name="depth">Number of distinct colors the device can display</param>
        /// <returns>this</returns>
        public Subject SetColorDepth(int depth)
        {
            nvPairs["cd"] = depth.ToString();
            return this;
        }

        /// <summary>
        /// Set the user's timezone
        /// </summary>
        /// <param name="timezone">Timezone (for example, "Europe/London")</param>
        /// <returns>this</returns>
        public Subject SetTimezone(string timezone)
        {
            nvPairs["tz"] = timezone;
            return this;
        }


        /// <summary>
        /// Set the user's language
        /// </summary>
        /// <param name="lang">Language</param>
        /// <returns>this</returns>
        public Subject SetLang(string lang)
        {
            nvPairs["lang"] = lang;
            return this;
        }
    }
}
