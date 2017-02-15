/*
 * IPayload.cs
 * 
 * Copyright (c) 2014-2017 Snowplow Analytics Ltd. All rights reserved.
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
 * Copyright: Copyright (c) 2014-2017 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System.Collections.Generic;

namespace Snowplow.Tracker.Models {
    public interface IPayload {

        /// <summary>
        /// Gets the dictionary within the Payload
        /// </summary>
        /// <returns>The payload</returns>
        Dictionary<string, object> Payload { get; }

        /// <summary>
        /// Gets the byte size of the key-value pairs in the payload
        /// </summary>
        /// <returns>The total byte size</returns>
        long GetByteSize ();

        /// <summary>
        /// Returns a string that represents the current <see cref="IPayload"/>.
        /// </summary>
        /// <returns>A string that represents the current <see cref="IPayload"/>.</returns>
        string ToString ();
    }
}
