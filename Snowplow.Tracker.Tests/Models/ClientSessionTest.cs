/*
 * ClientSessionTest.cs
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
using Snowplow.Tracker.Models;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System;

namespace Snowplow.Tracker.Tests.Models
{
    [TestClass]
    public class ClientSessionTest
    {
        private string getTempFile()
        {
            var fn = Path.GetTempFileName();
            File.Delete(fn);
            return fn;
        }

        [TestMethod]
        public void testClientSessionInitialization()
        {
            var fn = getTempFile();

            var clientSession = new ClientSession(fn);
            Assert.AreEqual(clientSession.GetForegroundTimeout(), 600);
            Assert.AreEqual(clientSession.GetBackgroundTimeout(), 300);
            Assert.AreEqual(clientSession.GetCheckInterval(), 15);
            Assert.AreEqual(clientSession.Background, false);

            string userId = clientSession.UserId;
            string sessionId = clientSession.CurrentSessionId;
            string previousId = clientSession.PreviousSessionId;
            int sessionIndex = clientSession.SessionIndex;

            Assert.AreEqual(previousId, null);

            clientSession.Dispose();
            clientSession = null;

            var newClientSession = new ClientSession(fn);

            Assert.AreEqual(newClientSession.UserId, userId);
            Assert.AreEqual(newClientSession.PreviousSessionId, sessionId);
            Assert.AreEqual(newClientSession.SessionIndex, 2);

            File.Delete(fn);
        }

        [TestMethod]
        public void testClientSessionSetters()
        {
            var fn = getTempFile();

            var clientSession = new ClientSession(fn);
            clientSession.SetForegroundTimeoutSeconds(300);
            clientSession.SetBackgroundTimeoutSeconds(600);
            clientSession.SetCheckIntervalSeconds(30);
            clientSession.SetBackground(true);

            Assert.AreEqual(clientSession.GetForegroundTimeout(), 300);
            Assert.AreEqual(clientSession.GetBackgroundTimeout(), 600);
            Assert.AreEqual(clientSession.GetCheckInterval(), 30);
            Assert.AreEqual(clientSession.Background, true);

            File.Delete(fn);
        }

        [TestMethod]
        public void testClientSessionGetSessionContext()
        {
            var fn = getTempFile();

            var clientSession = new ClientSession(fn);
            
            var expectedRegex = new Regex(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/client_session/jsonschema/1-0-1"",""data"":{""userId"":""[^,]+"",""sessionId"":""[^,]+"",""previousSessionId"":null,""sessionIndex"":1,""storageMechanism"":""LOCAL_STORAGE"",""firstEventId"":""first-id""}}");

            var sessionContext1 = clientSession.GetSessionContext("first-id");
            Assert.IsTrue(expectedRegex.Match(sessionContext1.GetJson().ToString()).Success, String.Format("{0} doesn't match {1}", sessionContext1.GetJson().ToString(), expectedRegex.ToString()));

            var sessionContext2 = clientSession.GetSessionContext("second-id");
            Assert.IsTrue(expectedRegex.Match(sessionContext2.GetJson().ToString()).Success, String.Format("{0} doesn't match {1}", sessionContext2.GetJson().ToString(), expectedRegex.ToString()));

            File.Delete(fn);
        }

        [TestMethod]
        public void testClientSessionTimingOut()
        {
            var fn = getTempFile();

            var clientSession = new ClientSession(fn, foregroundTimeout: 2, backgroundTimeout: 2, checkInterval: 1);

            string userId = clientSession.UserId;
            string sessionId = clientSession.CurrentSessionId;
            string previousId = clientSession.PreviousSessionId;
            int sessionIndex = clientSession.SessionIndex;

            clientSession.StartChecker();

            Thread.Sleep(3000);

            Assert.AreEqual(clientSession.UserId, userId);
            Assert.AreEqual(clientSession.PreviousSessionId, sessionId);
            Assert.AreEqual(clientSession.SessionIndex, 2);

            clientSession.SetBackground(true);

            Thread.Sleep(3000);

            Assert.IsNull(clientSession.SessionCheckTimer);

            Assert.AreEqual(clientSession.UserId, userId);
            Assert.AreEqual(clientSession.SessionIndex, 3);

            clientSession.SetBackground(false);

            Assert.IsNotNull(clientSession.SessionCheckTimer);

            clientSession.StopChecker();

            Assert.IsNull(clientSession.SessionCheckTimer);

            File.Delete(fn);
        }
    }
}
