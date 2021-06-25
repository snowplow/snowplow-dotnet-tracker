/*
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
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections;
using Newtonsoft.Json;

namespace Snowplow.Tracker
{
    public class Utils
    {

        /// <summary>
        /// Gets the timestamp for an event
        /// </summary>
        /// <returns>The timestamp for the event</returns>
        public static long GetTimestamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }

        /// <summary>
        /// Returns a newly generated GUID string
        /// </summary>
        /// <returns>a new GUID</returns>
        public static string GetGUID()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Returns a dictionary converted to a JSON String
        /// </summary>
        /// <returns>the dictionary as a JSON String</returns>
        public static string DictToJSONString(IDictionary dict)
        {
            try
            {
                var s = JsonConvert.SerializeObject(dict);
                return s;
            }
            catch (Exception e)
            {
                throw new ArgumentException(String.Format(@"Invalid JSON: ""{0}""", dict), e);
            }
        }

        /// <summary>
        /// Gets the length of the UTF8 String.
        /// </summary>
        /// <returns>The UTF8 length</returns>
        /// <param name="str">String to get the length of</param>
        public static long GetUTF8Length(string str)
        {
            return System.Text.Encoding.UTF8.GetByteCount(str);
        }

        /// <summary>
        /// Base64 encodes a string
        /// </summary>
        /// <returns>the encoded string</returns>
        public static string Base64EncodeString(string stringToEncode)
        {
            byte[] plainTextBytes = StringToBytes(stringToEncode);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Strings to bytes.
        /// </summary>
        /// <returns>Converts strings to bytes and returns the byte array</returns>
        /// <param name="str">The string to convert</param>
        public static byte[] StringToBytes(string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// Serialize the specified dictionary.
        /// </summary>
        /// <param name="dict">The dictionary to serialize</param>
        public static byte[] SerializeDictionary(Dictionary<string, object> dict)
        {
            try
            {
                DataContractSerializer mSerializer = new DataContractSerializer(typeof(Dictionary<string, object>));
                MemoryStream mStream = new MemoryStream();
                mSerializer.WriteObject(mStream, dict);
                return mStream.ToArray();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <param name="bytes">The byte array to deserialize</param>
        public static Dictionary<string, object> DeserializeDictionary(byte[] bytes)
        {
            try
            {
                DataContractSerializer mSerializer = new DataContractSerializer(typeof(Dictionary<string, object>));
                MemoryStream mStream = new MemoryStream(bytes);
                return (Dictionary<string, object>) mSerializer.ReadObject(mStream);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks the argument providied and will throw an exception if it fails.
        /// </summary>
        /// <param name="argument">If set to <c>true</c> argument.</param>
        /// <param name="message">Message to print in failure case.</param>
        public static void CheckArgument(bool argument, string message)
        {
            if (!argument)
            {
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Writes the dictionary to file.
        /// </summary>
        /// <returns><c>true</c>, if dictionary to file was written, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        /// <param name="dictionary">Dictionary.</param>
        public static bool WriteDictionaryToFile(string path, Dictionary<string, object> dictionary)
        {
            try
            {
                File.WriteAllBytes(path, SerializeDictionary(dictionary));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the dictionary from file.
        /// </summary>
        /// <returns>The dictionary from file.</returns>
        /// <param name="path">Path.</param>
        public static Dictionary<string, object> ReadDictionaryFromFile(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                return DeserializeDictionary(bytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Determines if is time in range the specified startTime checkTime range.
        /// </summary>
        /// <returns><c>true</c> if is time in range the specified startTime checkTime range; otherwise, <c>false</c>.</returns>
        /// <param name="startTime">Start time.</param>
        /// <param name="checkTime">Check time.</param>
        /// <param name="range">Range.</param>
        public static bool IsTimeInRange(long startTime, long checkTime, long range)
        {
            return (startTime > (checkTime - range));
        }
    }
}
