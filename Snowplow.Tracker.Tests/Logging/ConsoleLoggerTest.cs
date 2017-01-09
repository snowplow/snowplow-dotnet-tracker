/*
 * ConsoleLoggerTest.cs
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
using Snowplow.Tracker.Logging;
using System;

namespace Snowplow.Tracker.Tests.Logging
{
    [TestClass]
    public class ConsoleLoggerTest
    {

        [TestMethod]
        public void testFormatting()
        {
            Exception e;
            try
            {
                throw new Exception("Broken");
            }
            catch (Exception x)
            {
                e = x;
            }
            var log = ConsoleLogger.FormatMessage("test", "message", e);
            Assert.AreEqual("test: message\n" + e.ToString() + "\n" + e.StackTrace.ToString(), log);
        }

    }
}
