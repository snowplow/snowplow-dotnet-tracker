/*
 * SelfDescribingJson.cs
 * 
 * Copyright (c) 2021 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Fred Blundun, Ed Lewis, Joshua Beemster
 * Copyright: Copyright (c) 2021 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;

namespace Snowplow.Tracker.Models
{
    public class SelfDescribingJson : AbstractPayload
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Payload.SelfDescribingJson"/> class.
        /// </summary>
        /// <param name="schema">A schema String</param>
        /// <param name="data">A data Object</param>
        public SelfDescribingJson(string schema, object data)
        {
            SetSchema(schema);
            SetData(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Payload.SelfDescribingJson"/> class.
        /// </summary>
        /// <param name="schema">A schema String</param>
        /// <param name="data">A SelfDescribingJson object</param>
        public SelfDescribingJson(string schema, SelfDescribingJson data)
        {
            SetSchema(schema);
            SetData(data);
        }

        /// <summary>
        /// Sets the schema
        /// </summary>
        /// <returns>The SelfDescribingJson</returns>
        /// <param name="schema">A Schema String</param>
        public SelfDescribingJson SetSchema(string schema)
        {
            Utils.CheckArgument(!string.IsNullOrEmpty(schema), "Schema cannot be null or empty.");
            Payload[Constants.SCHEMA] = schema;
            return this;
        }

        /// <summary>
        /// Sets the data with an Object
        /// </summary>
        /// <returns>The SelfDescribingJson</returns>
        /// <param name="data">A data Object</param>
        public SelfDescribingJson SetData(object data)
        {
            if (data == null)
            {
                return this;
            }
            Payload[Constants.DATA] = data;
            return this;
        }

        /// <summary>
        /// Sets the data with the contents of another SelfDescribingJson
        /// </summary>
        /// <returns>The SelfDescribingJson with a nested SelfDescribingJson</returns>
        /// <param name="data">The SelfDescribingJson to nest</param>
        public SelfDescribingJson SetData(SelfDescribingJson data)
        {
            if (data == null)
            {
                return this;
            }
            Payload[Constants.DATA] = data.Payload;
            return this;
        }
    }
}
