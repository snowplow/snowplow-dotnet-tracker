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

using System.Collections.Generic;
using Snowplow.Tracker.Models.Contexts;

namespace Snowplow.Tracker.Models.Events
{
    public abstract class AbstractEvent<T> : IEvent
    {

        protected List<IContext> customContexts = new List<IContext>();
        protected long deviceCreatedTimestamp = Utils.GetTimestamp();
        protected long? trueTimestamp = null;
        protected string eventId = Utils.GetGUID();

        public abstract T Self ();
        public abstract T Build ();

        // --- Builder Methods

        /// <summary>
        /// Sets the custom context list for the event.
        /// </summary>
        /// <returns>The custom context.</returns>
        /// <param name="customContexts">Custom contexts.</param>
        public T SetCustomContext(List<IContext> customContexts) {
            if (customContexts != null) {
                this.customContexts = customContexts;
            }
            return Self ();
        }

        /// <summary>
        /// Sets the timestamp (in ms since unix epoch).
        /// </summary>
        /// <returns>The timestamp.</returns>
        /// <param name="deviceCreatedTimestamp">Timestamp.</param>
        public T SetDeviceCreatedTimestamp(long deviceCreatedTimestamp) {
            this.deviceCreatedTimestamp = deviceCreatedTimestamp;
            return Self ();
        }

        /// <summary>
        /// Sets the true-timestamp (in ms since unix epoch).
        /// </summary>
        /// <returns>The timestamp.</returns>
        /// <param name="timestamp">Timestamp.</param>
        public T SetTrueTimestamp(long? trueTimestamp)
        {
            this.trueTimestamp = trueTimestamp;
            return Self();
        }

        /// <summary>
        /// Sets the event identifier (as a GUID).
        /// </summary>
        /// <returns>The event identifier.</returns>
        /// <param name="eventId">Event identifier.</param>
        public T SetEventId(string eventId) {
            if (!string.IsNullOrEmpty (eventId)) {
                this.eventId = eventId;
            }
            return Self ();
        }

        // --- Helper Methods

        /// <summary>
        /// Adds the common key-value pairs to an event payload
        /// </summary>
        /// <param name="payload">The payload to append values to</param>
        /// <returns>The complete payload</returns>
        protected Payload AddDefaultPairs(Payload payload) {
            payload.Add(Constants.EID, eventId);
            payload.Add(Constants.TIMESTAMP, deviceCreatedTimestamp.ToString());
            if (trueTimestamp != null)
            {
                payload.Add(Constants.TRUE_TIMESTAMP, trueTimestamp.ToString());
            }
            return payload;
        }

        // --- Interface Methods

        /// <summary>
        /// Gets the list of custom contexts attached to the event.
        /// </summary>
        /// <returns>The custom contexts list</returns>
        public List<IContext> GetContexts() {
            return customContexts;
        }

        /// <summary>
        /// Gets the device created event timestamp that has been set.
        /// </summary>
        /// <returns>The event timestamp</returns>
        public long GetDeviceCreatedTimestamp() {
            return deviceCreatedTimestamp;
        }

        /// <summary>
        /// Gets the true event timestamp that has been set.
        /// </summary>
        /// <returns>The true event timestamp</returns>
        public long? GetTrueTimestamp()
        {
            return trueTimestamp;
        }

        /// <summary>
        /// Gets the event GUID that has been set.
        /// </summary>
        /// <returns>The event guid</returns>
        public  string GetEventId() {
            return eventId;
        }

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        public abstract IPayload GetPayload ();
    }
}
