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

namespace Snowplow.Tracker.Models.Events
{
    public class SelfDescribing : AbstractEvent<SelfDescribing>
    {
        
        private SelfDescribingJson eventData;
        private bool base64Encode = false;

        /// <summary>
        /// Sets the event data.
        /// </summary>
        /// <returns>The event data.</returns>
        /// <param name="eventData">Event data.</param>
        public SelfDescribing SetEventData(SelfDescribingJson eventData) {
            this.eventData = eventData;
            return this;
        }
        
        public override SelfDescribing Self() {
            return this;
        }
        
        public override SelfDescribing Build() {
            Utils.CheckArgument (eventData != null, "EventData cannot be null.");
            return this;
        }

        public void SetBase64Encode(bool base64Encode) {
            this.base64Encode = base64Encode;
        }
        
        // --- Interface Methods

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        public override IPayload GetPayload() {
            Payload payload = new Payload();
            payload.Add (Constants.EVENT, Constants.EVENT_UNSTRUCTURED);
            SelfDescribingJson envelope = new SelfDescribingJson(Constants.SCHEMA_UNSTRUCT_EVENT, this.eventData.Payload);
            payload.AddJson(envelope.Payload, this.base64Encode, Constants.UNSTRUCTURED_ENCODED, Constants.UNSTRUCTURED);
            return AddDefaultPairs (payload);
        }
    }
}
