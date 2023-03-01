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
    public class Structured : AbstractEvent<Structured>
    {

        private string category;
        private string action;
        private string label;
        private string property;
        private double value;

        private bool valueSet = false;

        /// <summary>
        /// Sets the category.
        /// </summary>
        /// <returns>The category.</returns>
        /// <param name="category">Category.</param>
        public Structured SetCategory(string category) {
            this.category = category;
            return this;
        }

        /// <summary>
        /// Sets the action.
        /// </summary>
        /// <returns>The action.</returns>
        /// <param name="action">Action.</param>
        public Structured SetAction(string action) {
            this.action = action;
            return this;
        }

        /// <summary>
        /// Sets the label.
        /// </summary>
        /// <returns>The label.</returns>
        /// <param name="label">Label.</param>
        public Structured SetLabel(string label) {
            this.label = label;
            return this;
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <returns>The property.</returns>
        /// <param name="property">Property.</param>
        public Structured SetProperty(string property) {
            this.property = property;
            return this;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="value">Value.</param>
        public Structured SetValue(double value) {
            this.value = value;
            valueSet = true;
            return this;
        }

        public override Structured Self() {
            return this;
        }

        public override Structured Build() {
            Utils.CheckArgument (!string.IsNullOrEmpty(category), "Category cannot be null or empty.");
            Utils.CheckArgument (!string.IsNullOrEmpty(action), "Action cannot be null or empty.");
            return this;
        }

        // --- Interface Methods

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        public override IPayload GetPayload() {
            Payload payload = new Payload();
            payload.Add (Constants.EVENT, Constants.EVENT_STRUCTURED);
            payload.Add (Constants.SE_CATEGORY, this.category);
            payload.Add (Constants.SE_ACTION, this.action);
            payload.Add (Constants.SE_LABEL, this.label);
            payload.Add (Constants.SE_PROPERTY, this.property);
            payload.Add (Constants.SE_VALUE, valueSet ? value.ToString() : null);
            return AddDefaultPairs (payload);
        }
    }
}
