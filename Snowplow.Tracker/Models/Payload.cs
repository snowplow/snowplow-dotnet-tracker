/*
 * Payload.cs
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
using Newtonsoft.Json;

namespace Snowplow.Tracker
{
    public class Payload
    {
        private Dictionary<string, string> nvPairs;
        public Payload()
        {
            nvPairs = new Dictionary<string, string>();
        }

        public void Add(string name, double? value)
        {
            Add(name, value.ToString());
        }

        public void Add(string name, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                nvPairs.Add(name, value);
            }
        }

        public void AddDict(Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> nvPair in dict)
            {
                Add(nvPair.Key, nvPair.Value);
            }
        }

        public void AddJson(Dictionary <string, object> jsonDict, bool encodeBase64, string typeWhenEncoded, string typeWhenNotEncoded)
        {
            if (jsonDict != null && jsonDict.Count > 0)
            {
                string jsonString = JsonConvert.SerializeObject(jsonDict);
                if (encodeBase64)
                {
                    byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
                    string encodedDict = System.Convert.ToBase64String(plainTextBytes);
                    Add(typeWhenEncoded, encodedDict);
                }
                else
                {
                    Add(typeWhenNotEncoded, jsonString);
                }
            }
        }

        public Dictionary<string, string> NvPairs
        {
            get
            {
                return nvPairs;
            }
        }

    }
}
