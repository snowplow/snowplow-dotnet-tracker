/*
 * EcommerceTransactionTest.cs
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Models.Events;
using System;
using System.Collections.Generic;

namespace Snowplow.Tracker.Tests.Models.Events
{
    [TestClass]
    public class EcommerceTransactionTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"OrderId cannot be null or empty.")]
        public void testInitEcommerceTransactionWithNullOrderId()
        {
            new EcommerceTransaction().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"Items cannot be null.")]
        public void testInitEcommerceTransactionWithNullItems()
        {
            new EcommerceTransaction()
                .SetOrderId("orderId")
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"TotalValue cannot be null.")]
        public void testInitEcommerceTransactionWithNullTotalValue()
        {
            new EcommerceTransaction()
                .SetOrderId("orderId")
                .SetItems(new List<EcommerceTransactionItem>())
                .Build();
        }

        [TestMethod]
        public void testInitEcommerceTransaction()
        {
            var et = new EcommerceTransaction()
                .SetOrderId("orderId")
                .SetTotalValue(20.1)
                .SetAffiliation("affiliation")
                .SetTaxValue(1.56)
                .SetShipping(10.5)
                .SetCity("city")
                .SetState("state")
                .SetCountry("country")
                .SetCurrency("AUD")
                .SetItems(new List<EcommerceTransactionItem>())
                .SetTrueTimestamp(123456789123)
                .Build();

            Assert.IsNotNull(et);
            Assert.AreEqual(Constants.EVENT_ECOMM, et.GetPayload().Payload[Constants.EVENT]);
            Assert.AreEqual("orderId", et.GetPayload().Payload[Constants.TR_ID]);
            Assert.AreEqual("20.10", et.GetPayload().Payload[Constants.TR_TOTAL]);
            Assert.AreEqual("affiliation", et.GetPayload().Payload[Constants.TR_AFFILIATION]);
            Assert.AreEqual("1.56", et.GetPayload().Payload[Constants.TR_TAX]);
            Assert.AreEqual("10.50", et.GetPayload().Payload[Constants.TR_SHIPPING]);
            Assert.AreEqual("city", et.GetPayload().Payload[Constants.TR_CITY]);
            Assert.AreEqual("state", et.GetPayload().Payload[Constants.TR_STATE]);
            Assert.AreEqual("country", et.GetPayload().Payload[Constants.TR_COUNTRY]);
            Assert.AreEqual("AUD", et.GetPayload().Payload[Constants.TR_CURRENCY]);

            Assert.IsNotNull(et.GetContexts());
            Assert.IsTrue(et.GetPayload().Payload.ContainsKey(Constants.EID));
            Assert.IsTrue(et.GetPayload().Payload.ContainsKey(Constants.TIMESTAMP));
            Assert.IsTrue(et.GetPayload().Payload.ContainsKey(Constants.TRUE_TIMESTAMP));
        }
    }
}
