/*
 * PayloadToJsonString.cs
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
 * Authors: Ed Lewis
 * Copyright: Copyright (c) 2016 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Models.Adapters
{
    public class PayloadToJsonString : IPayloadToString
    {

        /// <summary>
        /// Inflate a Payload object from a properly formatted JSON string
        /// </summary>
        /// <param name="p">Serialized JSON string</param>
        /// <returns>A Payload object represented by the serialized JSON</returns>
        public Payload FromString(string p)
        {
            try
            {
                var s = JsonConvert.DeserializeObject<Payload>(p);
                return s;
            }
            catch (Exception e)
            {
                throw new ArgumentException(String.Format(@"Invalid JSON: ""{0}""", p), e);
            }
        }

        /// <summary>
        /// Serialize a Payload object as a JSON string
        /// </summary>
        /// <param name="p">Payload to serialize</param>
        /// <returns>Serialized representation of p</returns>
        public string ToString(Payload p)
        {
            return JsonConvert.SerializeObject(p);
        }
    }
}
