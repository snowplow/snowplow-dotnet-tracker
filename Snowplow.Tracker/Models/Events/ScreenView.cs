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

using System;

namespace Snowplow.Tracker.Models.Events
{
    public class ScreenView : AbstractEvent<ScreenView>
    {
        
        private string name;
        private string id;

        /// <summary>
        /// Sets the name.
        /// </summary>
        /// <returns>The name.</returns>
        /// <param name="name">Name.</param>
        public ScreenView SetName(string name) {
            this.name = name;
            return this;
        }

        /// <summary>
        /// Sets the identifier.
        /// </summary>
        /// <returns>The identifier.</returns>
        /// <param name="id">Identifier.</param>
        public ScreenView SetId(string id) {
            this.id = id;
            return this;
        }
        
        public override ScreenView Self() {
            return this;
        }
        
        public override ScreenView Build() {
            Utils.CheckArgument (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(id), "Both Name and Id cannot be null or empty.");
            return this;
        }
        
        // --- Interface Methods

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        public override IPayload GetPayload() {
            Payload payload = new Payload();
            payload.Add (Constants.SV_NAME, this.name);
            payload.Add (Constants.SV_ID, this.id);
            return new SelfDescribingJson (Constants.SCHEMA_SCREEN_VIEW, payload.Payload);
        }
    }
}
