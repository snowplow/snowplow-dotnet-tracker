/*
 * TransactionItem.cs
 * 
 * Copyright (c) 2014-2016 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Ed Lewis, Joshua Beemster
 * Copyright: Copyright (c) 2014-2016 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System.Collections.Generic;

namespace Snowplow.Tracker.Models.Events
{
    public struct TransactionItem
    {
        public string sku;
        public double price;
        public int quantity;
        public string name;
        public string category;
        public List<Dictionary<string, object>> context;

        /// <summary>
        /// A struct representing a single item in an ecommerce transaction
        /// </summary>
        /// <param name="sku">SKU (stock keeping unit) for the item</param>
        /// <param name="price">Price of the item</param>
        /// <param name="quantity">Quantity of the item purchased</param>
        /// <param name="name">Name of the item</param>
        /// <param name="category">Category of the item</param>
        /// <param name="context">List of custom contexts for the item</param>
        public TransactionItem(string sku, double price, int quantity, string name = null, string category = null, List<Dictionary<string, object>> context = null)
        {
            this.sku = sku;
            this.price = price;
            this.quantity = quantity;
            this.name = name;
            this.category = category;
            this.context = context;
        }
    }
}
