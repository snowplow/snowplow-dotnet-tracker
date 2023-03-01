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

using Xamarin.Forms;

namespace Snowplow.Demo.App.Utils
{
    public static class PropertyManager
    {
        /// <summary>
        /// Saves a key value pair
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public static async void SaveKeyValue(string key, object value)
        {
            Application.Current.Properties[key] = value;
            await Application.Current.SavePropertiesAsync();
        }

        /// <summary>
        /// Returns a string value for a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value or an empty string by default</returns>
        public static string GetStringValue(string key, string valueDefault = "")
        {
            try
            {
                var value = (string)Application.Current.Properties[key];
                return value == null ? valueDefault : value;
            }
            catch
            {
                return valueDefault;
            }
        }

        /// <summary>
        /// Returns a bool value for a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value or false by default</returns>
        public static bool GetBoolValue(string key)
        {
            try
            {
                return (bool)Application.Current.Properties[key];
            }
            catch
            {
                return false;
            }
        }
    }
}
