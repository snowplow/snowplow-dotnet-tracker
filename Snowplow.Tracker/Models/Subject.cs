/*
 * Subject.cs
 * 
 * Copyright (c) 2014-2016 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Ed Lewis, Joshua Beemster
 * Copyright: Copyright (c) 2014-2016 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;

namespace Snowplow.Tracker.Models
{
    public class Subject
    {
        public Payload _payload { get; private set; }

        /// <summary>
        /// Create a subject representing a user
        /// </summary>
        public Subject()
        {
            _payload = new Payload();
            SetPlatform(Platform.Pc);
        }

        /// <summary>
        /// Set the user's platform
        /// </summary>
        /// <param name="value">The platform</param>
        /// <returns>this</returns>
        public Subject SetPlatform(Platform value)
        {
            _payload.Add(Constants.PLATFORM, value.ToString().ToLower());
            return this;
        }

        /// <summary>
        /// Set a unique ID for the user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>this</returns>
        public Subject SetUserId(string id)
        {
            _payload.Add(Constants.UID, id);
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
            _payload.Add(Constants.RESOLUTION, String.Format("{0}x{1}", width.ToString(), height.ToString()));
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
            _payload.Add(Constants.VIEWPORT, String.Format("{0}x{1}", width.ToString(), height.ToString()));
            return this;
        }

        /// <summary>
        /// Set the user device's color depth
        /// </summary>
        /// <param name="depth">Number of distinct colors the device can display</param>
        /// <returns>this</returns>
        public Subject SetColorDepth(int depth)
        {
            _payload.Add(Constants.COLOR_DEPTH, depth.ToString());
            return this;
        }

        /// <summary>
        /// Set the user's timezone
        /// </summary>
        /// <param name="timezone">Timezone (for example, "Europe/London")</param>
        /// <returns>this</returns>
        public Subject SetTimezone(string timezone)
        {
            _payload.Add(Constants.TIMEZONE, timezone);
            return this;
        }

        /// <summary>
        /// Set the user's language
        /// </summary>
        /// <param name="lang">Language</param>
        /// <returns>this</returns>
        public Subject SetLang(string lang)
        {
            _payload.Add(Constants.LANGUAGE, lang);
            return this;
        }

        /// <summary>
        /// Sets the ip address.
        /// </summary>
        /// <param name="ipAddress">Ip address.</param>
        public Subject SetIpAddress(String ipAddress)
        {
            _payload.Add(Constants.IP_ADDRESS, ipAddress);
            return this;
        }

        /// <summary>
        /// Sets the useragent.
        /// </summary>
        /// <param name="useragent">Useragent.</param>
        public Subject SetUseragent(String useragent)
        {
            _payload.Add(Constants.USERAGENT, useragent);
            return this;
        }

        /// <summary>
        /// Sets the domain user identifier.
        /// </summary>
        /// <param name="domainUserId">Domain user identifier.</param>
        public Subject SetDomainUserId(String domainUserId)
        {
            _payload.Add(Constants.DOMAIN_UID, domainUserId);
            return this;
        }

        /// <summary>
        /// Sets the network user identifier.
        /// </summary>
        /// <param name="networkUserId">Network user identifier.</param>
        public Subject SetNetworkUserId(String networkUserId)
        {
            _payload.Add(Constants.NETWORK_UID, networkUserId);
            return this;
        }
    }
}
