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

        public Subject()
        {
            nvPairs = new Dictionary<string, string>
            {
                { "p", "pc" }
            };
        }

        public Subject setPlatform(string value)
        {
            nvPairs["p"] = value;
            return this;
        }

        public Subject setUserId(string id)
        {
            nvPairs["uid"] = id;
            return this;
        }

        public Subject setScreenResolution(int width, int height)
        {
            nvPairs["res"] = width.ToString() + "x" + height.ToString();
            return this;
        }

        public Subject setViewport(int width, int height)
        {
            nvPairs["vp"] = width.ToString() + "x" + height.ToString();
            return this;
        }

        public Subject setColorDepth(int depth)
        {
            nvPairs["cd"] = depth.ToString();
            return this;
        }

        public Subject setTimezone(string timezone)
        {
            nvPairs["tz"] = timezone;
            return this;
        }

        public Subject setLang(string lang)
        {
            nvPairs["lang"] = lang;
            return this;
        }
    }
}
