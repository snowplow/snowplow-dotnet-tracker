/*
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
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Models.Events;
using System;
using System.Collections.Generic;

namespace Snowplow.Tracker.Tests.Models.Events
{
    [TestClass]
    public class EcommerceTransactionItemTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Sku cannot be null or empty.")]
        public void testInitEcommerceTransactionItemWithNullSku()
        {
            new EcommerceTransactionItem().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Price cannot be null.")]
        public void testInitEcommerceTransactionItemWithNullPrice()
        {
            new EcommerceTransactionItem()
                .SetSku("sku")
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Quantity cannot be null.")]
        public void testInitEcommerceTransactionItemWithNullQuantity()
        {
            new EcommerceTransactionItem()
                .SetSku("sku")
                .SetPrice(10.56)
                .Build();
        }

        [TestMethod]
        public void testInitEcommerceTransactionItem()
        {
            var eti = new EcommerceTransactionItem()
                .SetSku("sku")
                .SetPrice(20.1)
                .SetQuantity(5)
                .SetName("name")
                .SetCategory("category")
                .SetTrueTimestamp(123456789123)
                .Build();

            eti.SetItemId("itemId");
            eti.SetCurrency("AUD");

            Assert.IsNotNull(eti);
            Assert.AreEqual(Constants.EVENT_ECOMM_ITEM, eti.GetPayload().Payload[Constants.EVENT]);
            Assert.AreEqual("itemId", eti.GetPayload().Payload[Constants.TI_ITEM_ID]);
            Assert.AreEqual("sku", eti.GetPayload().Payload[Constants.TI_ITEM_SKU]);
            Assert.AreEqual("name", eti.GetPayload().Payload[Constants.TI_ITEM_NAME]);
            Assert.AreEqual("category", eti.GetPayload().Payload[Constants.TI_ITEM_CATEGORY]);
            Assert.AreEqual("20.10", eti.GetPayload().Payload[Constants.TI_ITEM_PRICE]);
            Assert.AreEqual("5", eti.GetPayload().Payload[Constants.TI_ITEM_QUANTITY]);
            Assert.AreEqual("AUD", eti.GetPayload().Payload[Constants.TI_ITEM_CURRENCY]);

            Assert.IsNotNull(eti.GetContexts());
            Assert.IsTrue(eti.GetPayload().Payload.ContainsKey(Constants.EID));
            Assert.IsTrue(eti.GetPayload().Payload.ContainsKey(Constants.TIMESTAMP));
            Assert.IsTrue(eti.GetPayload().Payload.ContainsKey(Constants.TRUE_TIMESTAMP));
        }
    }
}
