/*
 * IEvent.cs
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
using Snowplow.Tracker.Models.Contexts;

namespace Snowplow.Tracker.Models.Events
{
    public interface IEvent
    {

        /// <summary>
        /// Gets the list of custom contexts attached to the event.
        /// </summary>
        /// <returns>The custom contexts list</returns>
        List<IContext> GetContexts();

        /// <summary>
        /// Gets the device created event timestamp that has been set.
        /// </summary>
        /// <returns>The event timestamp</returns>
        long GetDeviceCreatedTimestamp();

        /// <summary>
        /// Gets the true event timestamp that has been set.
        /// </summary>
        /// <returns>The event timestamp</returns>
        long? GetTrueTimestamp();

        /// <summary>
        /// Gets the event GUID that has been set.
        /// </summary>
        /// <returns>The event guid</returns>
        string GetEventId();

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        IPayload GetPayload();
    }
}
