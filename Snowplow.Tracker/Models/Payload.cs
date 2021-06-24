/*
 * TrackerPayload.cs
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

namespace Snowplow.Tracker.Models
{
    public class Payload : AbstractPayload
    {

        /// <summary>
        /// Add the specified key and value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return;
            }
            Payload[key] = value;
        }

        /// <summary>
        /// Adds a dictionary of key-value pairs
        /// </summary>
        /// <param name="dictionary">Dictionary to add</param>
        public void AddDict(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> entry in dictionary)
            {
                Add(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Adds a dictionary of key-value pairs
        /// </summary>
        /// <param name="dictionary">Dictionary to add</param>
        public void AddDict(Dictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                return;
            }
            foreach (KeyValuePair<string, object> entry in dictionary)
            {
                if (entry.Value is string)
                {
                    Add(entry.Key, (string)entry.Value);
                }
            }
        }

        /// <summary>
        /// Adds a json dictionary as an encoded string
        /// </summary>
        /// <param name="jsonDict">Json dict.</param>
        /// <param name="encodeBase64">If set to <c>true</c> encode base64.</param>
        /// <param name="typeEncoded">Type encoded.</param>
        /// <param name="typeNotEncoded">Type not encoded.</param>
        public void AddJson(Dictionary<string, object> jsonDict, bool encodeBase64, string typeEncoded, string typeNotEncoded)
        {
            if (jsonDict == null || jsonDict.Count == 0)
            {
                return;
            }
            string jsonString = Utils.DictToJSONString(jsonDict);
            if (!string.IsNullOrEmpty(jsonString))
            {
                if (encodeBase64)
                {
                    Add(typeEncoded, Utils.Base64EncodeString(jsonString));
                }
                else
                {
                    Add(typeNotEncoded, jsonString);
                }
            }
        }
    }
}
