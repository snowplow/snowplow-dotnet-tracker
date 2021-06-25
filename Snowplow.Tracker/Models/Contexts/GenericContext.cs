/*
 * GenericContext.cs
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

using System.Collections.Generic;
using Snowplow.Tracker.Models;

namespace Snowplow.Tracker.Models.Contexts
{
    public class GenericContext : AbstractContext<GenericContext>
    {

        /// <summary>
        /// Sets the schema.
        /// </summary>
        /// <returns>The schema.</returns>
        /// <param name="schema">Schema.</param>
        public GenericContext SetSchema(string schema) {
            this.schema = schema;
            return this;
        }

        /// <summary>
        /// Add the specified key and value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public GenericContext Add(string key, object value) {
            this.DoAdd (key, value, true);
            return this;
        }
        
        /// <summary>
        /// Adds the dict.
        /// </summary>
        /// <param name="dictionary">The dictionary of params to add.</param>
        public GenericContext AddDict(Dictionary<string, object> dictionary) {
            if (dictionary == null) {
                return this;
            }
            foreach (KeyValuePair<string, object> entry in dictionary) {
                this.DoAdd (entry.Key, entry.Value, true);
            }
            return this;
        }

        public override GenericContext Build() {
            this.context = new SelfDescribingJson (this.schema, this.data);
            return this;
        }
    }
}
