/*
 * EcommerceTransactionItem.cs
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

namespace Snowplow.Tracker.Models.Events
{
    public class EcommerceTransactionItem : AbstractEvent<EcommerceTransactionItem>
    {

        private string itemId;
        private string sku;
        private double price;
        private int quantity;
        private string name;
        private string category;
        private string currency;

        private bool priceSet = false;
        private bool quantitySet = false;

        /// <summary>
        /// Sets the item identifier, should be the same as the parent OrderId.
        /// </summary>
        /// <returns>The item identifier.</returns>
        /// <param name="itemId">Item identifier.</param>
        public void SetItemId(string itemId) {
            this.itemId = itemId;
        }

        /// <summary>
        /// Sets the sku.
        /// </summary>
        /// <returns>The sku.</returns>
        /// <param name="sku">Sku.</param>
        public EcommerceTransactionItem SetSku(string sku) {
            this.sku = sku;
            return this;
        }

        /// <summary>
        /// Sets the price.
        /// </summary>
        /// <returns>The price.</returns>
        /// <param name="price">Price.</param>
        public EcommerceTransactionItem SetPrice(double price) {
            this.price = price;
            this.priceSet = true;
            return this;
        }

        /// <summary>
        /// Sets the quantity.
        /// </summary>
        /// <returns>The quantity.</returns>
        /// <param name="quantity">Quantity.</param>
        public EcommerceTransactionItem SetQuantity(int quantity) {
            this.quantity = quantity;
            this.quantitySet = true;
            return this;
        }

        /// <summary>
        /// Sets the name.
        /// </summary>
        /// <returns>The name.</returns>
        /// <param name="name">Name.</param>
        public EcommerceTransactionItem SetName(string name) {
            this.name = name;
            return this;
        }

        /// <summary>
        /// Sets the category.
        /// </summary>
        /// <returns>The category.</returns>
        /// <param name="category">Category.</param>
        public EcommerceTransactionItem SetCategory(string category) {
            this.category = category;
            return this;
        }

        /// <summary>
        /// Sets the currency.
        /// </summary>
        /// <param name="currency">Currency.</param>
        public void SetCurrency(string currency) {
            this.currency = currency;
        }
        
        public override EcommerceTransactionItem Self() {
            return this;
        }
        
        public override EcommerceTransactionItem Build() {
            Utils.CheckArgument(!string.IsNullOrEmpty(sku), "Sku cannot be null or empty.");
            Utils.CheckArgument(priceSet, "Price cannot be null.");
            Utils.CheckArgument(quantitySet, "Quantity cannot be null.");
            return this;
        }
        
        // --- Interface Methods

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        public override IPayload GetPayload() {
            Payload payload = new Payload();
            payload.Add (Constants.EVENT, Constants.EVENT_ECOMM_ITEM);
            payload.Add (Constants.TI_ITEM_ID, this.itemId);
            payload.Add (Constants.TI_ITEM_SKU, this.sku);
            payload.Add (Constants.TI_ITEM_NAME, this.name);
            payload.Add (Constants.TI_ITEM_CATEGORY, this.category);
            payload.Add (Constants.TI_ITEM_PRICE, string.Format("{0:0.00}", this.price));
            payload.Add (Constants.TI_ITEM_QUANTITY, this.quantity.ToString());
            payload.Add (Constants.TI_ITEM_CURRENCY, this.currency);
            return AddDefaultPairs (payload);
        }
    }
}
