/*
 * Timing.cs
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

using System.Collections.Generic;

namespace Snowplow.Tracker.Models.Events
{
    public class Timing : AbstractEvent<Timing>
    {

        private string category;
        private string variable;
        private int timing;
        private string label;
        
        private bool timingSet = false;

        /// <summary>
        /// Sets the category.
        /// </summary>
        /// <returns>The category.</returns>
        /// <param name="category">Category.</param>
        public Timing SetCategory(string category) {
            this.category = category;
            return this;
        }

        /// <summary>
        /// Sets the variable.
        /// </summary>
        /// <returns>The variable.</returns>
        /// <param name="variable">Variable.</param>
        public Timing SetVariable(string variable) {
            this.variable = variable;
            return this;
        }

        /// <summary>
        /// Sets the timing.
        /// </summary>
        /// <returns>The timing.</returns>
        /// <param name="timing">Timing.</param>
        public Timing SetTiming(int timing) {
            this.timing = timing;
            timingSet = true;
            return this;
        }

        /// <summary>
        /// Sets the label.
        /// </summary>
        /// <returns>The label.</returns>
        /// <param name="label">Label.</param>
        public Timing SetLabel(string label) {
            this.label = label;
            return this;
        }
        
        public override Timing Self() {
            return this;
        }
        
        public override Timing Build() {
            Utils.CheckArgument (!string.IsNullOrEmpty(category), "Category cannot be null or empty.");
            Utils.CheckArgument (!string.IsNullOrEmpty(variable), "Variable cannot be null or empty.");
            Utils.CheckArgument (timingSet, "Timing cannot be null.");
            return this;
        }
        
        // --- Interface Methods

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        public override IPayload GetPayload() {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload.Add (Constants.UT_CATEGORY, this.category);
            if (!string.IsNullOrEmpty(this.label)) {
                payload.Add (Constants.UT_LABEL, this.label);
            }
            payload.Add (Constants.UT_TIMING, this.timing);
            payload.Add (Constants.UT_VARIABLE, this.variable);
            return new SelfDescribingJson (Constants.SCHEMA_USER_TIMINGS, payload);
        }
    }
}
