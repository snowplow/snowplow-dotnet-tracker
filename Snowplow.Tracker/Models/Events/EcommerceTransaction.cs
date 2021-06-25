/*
 * EcommerceTransaction.cs
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
    public class EcommerceTransaction : AbstractEvent<EcommerceTransaction>
    {

        private string orderId;
        private double totalValue;
        private string affiliation;
        private double taxValue;
        private double shipping;
        private string city;
        private string state;
        private string country;
        private string currency;
        private List<EcommerceTransactionItem> items;

        private bool totalValueSet = false;
        private bool taxValueSet = false;
        private bool shippingSet = false;

        /// <summary>
        /// Sets the order identifier.
        /// </summary>
        /// <returns>The order identifier.</returns>
        /// <param name="orderId">Order identifier.</param>
        public EcommerceTransaction SetOrderId(string orderId) {
            this.orderId = orderId;
            return this;
        }

        /// <summary>
        /// Sets the total value.
        /// </summary>
        /// <returns>The total value.</returns>
        /// <param name="totalValue">Total value.</param>
        public EcommerceTransaction SetTotalValue(double totalValue) {
            this.totalValue = totalValue;
            this.totalValueSet = true;
            return this;
        }

        /// <summary>
        /// Sets the affiliation.
        /// </summary>
        /// <returns>The affiliation.</returns>
        /// <param name="affiliation">Affiliation.</param>
        public EcommerceTransaction SetAffiliation(string affiliation) {
            this.affiliation = affiliation;
            return this;
        }

        /// <summary>
        /// Sets the tax value.
        /// </summary>
        /// <returns>The tax value.</returns>
        /// <param name="taxValue">Tax value.</param>
        public EcommerceTransaction SetTaxValue(double taxValue) {
            this.taxValue = taxValue;
            this.taxValueSet = true;
            return this;
        }

        /// <summary>
        /// Sets the shipping.
        /// </summary>
        /// <returns>The shipping.</returns>
        /// <param name="shipping">Shipping.</param>
        public EcommerceTransaction SetShipping(double shipping) {
            this.shipping = shipping;
            this.shippingSet = true;
            return this;
        }
    
        /// <summary>
        /// Sets the city.
        /// </summary>
        /// <returns>The city.</returns>
        /// <param name="city">City.</param>
        public EcommerceTransaction SetCity(string city) {
            this.city = city;
            return this;
        }

        /// <summary>
        /// Sets the state.
        /// </summary>
        /// <returns>The state.</returns>
        /// <param name="state">State.</param>
        public EcommerceTransaction SetState(string state) {
            this.state = state;
            return this;
        }

        /// <summary>
        /// Sets the country.
        /// </summary>
        /// <returns>The country.</returns>
        /// <param name="country">Country.</param>
        public EcommerceTransaction SetCountry(string country) {
            this.country = country;
            return this;
        }

        /// <summary>
        /// Sets the currency.
        /// </summary>
        /// <returns>The currency.</returns>
        /// <param name="currency">Currency.</param>
        public EcommerceTransaction SetCurrency(string currency) {
            this.currency = currency;
            return this;
        }

        /// <summary>
        /// Sets the items.
        /// </summary>
        /// <returns>The items.</returns>
        /// <param name="items">Items.</param>
        public EcommerceTransaction SetItems(List<EcommerceTransactionItem> items) {
            this.items = items;
            return this;
        }

        public override EcommerceTransaction Self() {
            return this;
        }
        
        public override EcommerceTransaction Build() {
            Utils.CheckArgument (!string.IsNullOrEmpty(orderId), "OrderId cannot be null or empty.");
            Utils.CheckArgument (items != null, "Items cannot be null.");
            Utils.CheckArgument (totalValueSet, "TotalValue cannot be null.");
            return this;
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <returns>The items.</returns>
        public List<EcommerceTransactionItem> GetItems() {
            return this.items;
        }

        /// <summary>
        /// Gets the order identifier.
        /// </summary>
        /// <returns>The order identifier.</returns>
        public string GetOrderId() {
            return this.orderId;
        }

        /// <summary>
        /// Gets the currency.
        /// </summary>
        /// <returns>The currency.</returns>
        public string GetCurrency() {
            return this.currency;
        }
        
        // --- Interface Methods

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        public override IPayload GetPayload() {
            Payload payload = new Payload();
            payload.Add (Constants.EVENT, Constants.EVENT_ECOMM);
            payload.Add (Constants.TR_ID, this.orderId);
            payload.Add (Constants.TR_TOTAL, string.Format("{0:0.00}", this.totalValue));
            payload.Add (Constants.TR_AFFILIATION, this.affiliation);
            payload.Add (Constants.TR_TAX, this.taxValueSet ? (string.Format("{0:0.00}", this.taxValue)) : null);
            payload.Add (Constants.TR_SHIPPING, this.shippingSet ? (string.Format("{0:0.00}", this.shipping)) : null);
            payload.Add (Constants.TR_CITY, this.city);
            payload.Add (Constants.TR_STATE, this.state);
            payload.Add (Constants.TR_COUNTRY, this.country);
            payload.Add (Constants.TR_CURRENCY, this.currency);
            return AddDefaultPairs (payload);
        }
    }
}
