/*
 * IContext.cs
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

using System;
using System.Collections.Generic;

namespace Snowplow.Tracker.Models.Contexts
{
    public interface IContext
    {

        /// <summary>
        /// Gets the context as a self describing json.
        /// </summary>
        /// <returns>The context as self describing json.</returns>
        SelfDescribingJson GetJson();

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <returns>The schema.</returns>
        string GetSchema();

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <returns>The data.</returns>
        Dictionary<string, object> GetData();
    }
}
