/*
 * RedisEmitter.cs
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
using Sider;
using System.Web.Script.Serialization;

namespace Snowplow.Tracker
{
    public class RedisEmitter : IEmitter, IDisposable
    {
        private RedisClient rdb;
        private String key;
        private bool disposed = false;

        private static JavaScriptSerializer jss = new JavaScriptSerializer();

        /// <summary>
        /// Create an emitter to send events to a Redis database
        /// </summary>
        /// <param name="rdb">Database to send events to</param>
        /// <param name="key">Key under which to store events</param>
        public RedisEmitter(RedisClient rdb = null, string key = "snowplow")
        {
            this.rdb = rdb ?? new RedisClient();
            this.key = key;
        }

        /// <summary>
        /// Store an event in Redis in string JSON form
        /// </summary>
        /// <param name="payload">Event to store</param>
        public void Input(Dictionary<string, string> payload)
        {
            rdb.RPush(key, jss.Serialize(payload));
        }

        public void Flush(bool sync = false)
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    rdb.Dispose();
                }
                disposed = true;
            }
        }

    }
}
