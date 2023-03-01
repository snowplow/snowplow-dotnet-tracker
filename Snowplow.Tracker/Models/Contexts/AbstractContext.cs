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
using System.Collections.Generic;

namespace Snowplow.Tracker.Models.Contexts {
    public abstract class AbstractContext<T> : IContext {

        protected SelfDescribingJson context;
        protected string schema;
        protected Dictionary<string, object> data = new Dictionary<string, object>();

        public abstract T Build ();
    
        /// <summary>
        /// Does the addition of key-value pairs to the data dictionary.
        /// </summary>
        /// <param name="">If set to <c>true</c> .</param>
        /// <param name="value">Value.</param>
        /// <param name="force">Boolean to remove checks before adding.</param>
        protected void DoAdd(string key, object value, bool force = false) {
            if (force) {
                this.data.Add (key, value);
            } else {
                if (value != null)
                {
                    Type vType = value.GetType ();
                    if (String.IsNullOrEmpty(key)) {
                        // If the key is null or empty do not add.
                        return;
                    } else if (typeof(string) == vType && String.IsNullOrEmpty((string)value)) {
                        // If the value is a string ensure it is not null or empty.
                        return;
                    }
                    this.data[key] = value;
                }
            }
        }

        // --- Interface Methods

        /// <summary>
        /// Gets the context as a self describing json.
        /// </summary>
        /// <returns>The context as self describing json.</returns>
        public SelfDescribingJson GetJson() {
            return this.context;
        }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <returns>The schema.</returns>
        public string GetSchema() {
            return this.schema;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <returns>The data.</returns>
        public Dictionary<string, object> GetData() {
            return this.data;
        }
    }
}
