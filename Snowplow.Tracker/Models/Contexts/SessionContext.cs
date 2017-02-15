/*
 * SessionContext.cs
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

namespace Snowplow.Tracker.Models.Contexts
{
    public class SessionContext : AbstractContext<SessionContext>
    {

        /// <summary>
        /// Sets the user identifier.
        /// </summary>
        /// <returns>The user identifier.</returns>
        /// <param name="userId">User identifier.</param>
        public SessionContext SetUserId(string userId) {
            this.DoAdd (Constants.SESSION_USER_ID, userId);
            return this;
        }

        /// <summary>
        /// Sets the session identifier.
        /// </summary>
        /// <returns>The session identifier.</returns>
        /// <param name="sessionId">Session identifier.</param>
        public SessionContext SetSessionId(string sessionId) {
            this.DoAdd (Constants.SESSION_ID, sessionId);
            return this;
        }

        /// <summary>
        /// Sets the index of the session.
        /// </summary>
        /// <returns>The session index.</returns>
        /// <param name="sessionIndex">Session index.</param>
        public SessionContext SetSessionIndex(int sessionIndex) {
            this.DoAdd (Constants.SESSION_INDEX, sessionIndex);
            return this;
        }

        /// <summary>
        /// Sets the previous session identifier.
        /// </summary>
        /// <returns>The previous session identifier.</returns>
        /// <param name="previousSessionId">Previous session identifier.</param>
        public SessionContext SetPreviousSessionId(string previousSessionId) {
            this.DoAdd (Constants.SESSION_PREVIOUS_ID, previousSessionId, true);
            return this;
        }

        /// <summary>
        /// Sets the storage mechanism.
        /// </summary>
        /// <returns>The storage mechanism.</returns>
        /// <param name="storageMechanism">Storage mechanism.</param>
        public SessionContext SetStorageMechanism(StorageMechanism storageMechanism) {
            this.DoAdd (Constants.SESSION_STORAGE, storageMechanism.Value);
            return this;
        }

        /// <summary>
        /// Sets the first event identifier.
        /// </summary>
        /// <returns>The first event identifier.</returns>
        /// <param name="firstEventId">First event identifier.</param>
        public SessionContext SetFirstEventId(string firstEventId) {
            this.DoAdd (Constants.SESSION_FIRST_ID, firstEventId);
            return this;
        }
        
        public override SessionContext Build() {
            Utils.CheckArgument (this.data.ContainsKey(Constants.SESSION_USER_ID), "Session context requires 'userId'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.SESSION_ID), "Session context requires 'sessionId'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.SESSION_INDEX), "Session context requires 'sessionIndex'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.SESSION_PREVIOUS_ID), "Session context requires 'previousSessionId'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.SESSION_STORAGE), "Session context requires 'storageMechanism'.");
            this.schema = Constants.SCHEMA_SESSION;
            this.context = new SelfDescribingJson (this.schema, this.data);
            return this;
        }
    }
}
