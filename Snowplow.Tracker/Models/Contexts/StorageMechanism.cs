/*
 * StorageMechanism.cs
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

namespace Snowplow.Tracker.Models.Contexts
{
    public class StorageMechanism
    {

        public string Value { get; set; }

        private StorageMechanism(string value)
        {
            Value = value;
        }

        public static StorageMechanism Sqlite { get { return new StorageMechanism("SQLITE"); } }
        public static StorageMechanism Cookie1 { get { return new StorageMechanism("COOKIE_1"); } }
        public static StorageMechanism Cookie3 { get { return new StorageMechanism("COOKIE_3"); } }
        public static StorageMechanism LocalStorage { get { return new StorageMechanism("LOCAL_STORAGE"); } }
        public static StorageMechanism FlashLso { get { return new StorageMechanism("FLASH_LSO"); } }
    }
}
