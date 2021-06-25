/*
 * AbstractPayload.cs
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

namespace Snowplow.Tracker.Models {
    public abstract class AbstractPayload : IPayload {

        /// <summary>
        /// Gets the dictionary within the Payload
        /// </summary>
        /// <returns>The payload</returns>
        public Dictionary<string, object> Payload { get; private set; }

        /// <summary>
        /// Creates a new Dictionary to store KV pairs in
        /// </summary>
        public AbstractPayload() {
            Payload = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the byte size of the key-value pairs in the payload
        /// </summary>
        /// <returns>The total byte size</returns>
        public long GetByteSize() {
            return Utils.GetUTF8Length (ToString ());
        }

        /// <summary>
        /// Returns a JSON string representing the payload.
        /// </summary>
        /// <returns>A JSON string representing the payload.</returns>
        public override string ToString() {
            return Utils.DictToJSONString(Payload);
        }
    }
}
