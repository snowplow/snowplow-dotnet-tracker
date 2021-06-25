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

using System;
using System.Collections.Generic;
using System.Threading;
using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Models.Contexts;

namespace Snowplow.Tracker.Models
{
    public class ClientSession : IDisposable
    {
        private SessionContext _sessionContext;

        private long _foregroundTimeout;
        private long _backgroundTimeout;
        private long _checkInterval;
        private long _accessedLast;
        private StorageMechanism _sessionStorage = StorageMechanism.LocalStorage;
        public bool Background { get; private set; } = false;
        public string FirstEventId { get; private set; }
        public string UserId { get; private set; }
        public string CurrentSessionId { get; private set; }
        public string PreviousSessionId { get; private set; }
        public int SessionIndex { get; private set; }
        public Timer SessionCheckTimer { get; private set; }

        private string _savePath;
        private ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Session"/> class.
        /// </summary>
        /// <param name="foregroundTimeout">Foreground timeout.</param>
        /// <param name="backgroundTimeout">Background timeout.</param>
        /// <param name="checkInterval">Check interval.</param>
        public ClientSession(string savePath, long foregroundTimeout = 600, long backgroundTimeout = 300, long checkInterval = 15, ILogger l = null)
        {
            this._savePath = savePath;
            this._logger = l ?? new NoLogging();
            this._foregroundTimeout = foregroundTimeout * 1000;
            this._backgroundTimeout = backgroundTimeout * 1000;
            this._checkInterval = checkInterval * 1000;

            Dictionary<string, object> maybeSessionDict = Utils.ReadDictionaryFromFile(this._savePath);
            if (maybeSessionDict == null)
            {
                this.UserId = Utils.GetGUID();
                this.CurrentSessionId = null;
            }
            else
            {
                object userId = "";
                object sessionId = "";
                object previousId = "";
                object sessionIndex = 0;

                if (maybeSessionDict.TryGetValue(Constants.SESSION_USER_ID, out userId))
                {
                    this.UserId = (string)userId;
                };
                if (maybeSessionDict.TryGetValue(Constants.SESSION_ID, out sessionId))
                {
                    this.CurrentSessionId = (string)sessionId;
                };
                if (maybeSessionDict.TryGetValue(Constants.SESSION_PREVIOUS_ID, out previousId))
                {
                    this.PreviousSessionId = (string)previousId;
                };
                if (maybeSessionDict.TryGetValue(Constants.SESSION_INDEX, out sessionIndex))
                {
                    this.SessionIndex = (int)sessionIndex;
                };
            }

            UpdateSession();
            UpdateAccessedLast();
            UpdateSessionDict();
            Utils.WriteDictionaryToFile(this._savePath, _sessionContext.GetData());
        }

        // --- Public

        /// <summary>
        /// Gets the session context.
        /// </summary>
        /// <returns>The session context.</returns>
        /// <param name="eventId">Event identifier.</param>
        public SessionContext GetSessionContext(string eventId)
        {
            StartChecker();
            UpdateAccessedLast();
            if (FirstEventId == null)
            {
                FirstEventId = eventId;
                _sessionContext.SetFirstEventId(eventId);
                _sessionContext.Build();
            }
            return _sessionContext;
        }

        /// <summary>
        /// Starts the session checker.
        /// </summary>
        public void StartChecker()
        {
            if (SessionCheckTimer == null)
            {
                SessionCheckTimer = new Timer(CheckSession, null, (int)_checkInterval, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Stops the session checker.
        /// </summary>
        public void StopChecker()
        {
            if (SessionCheckTimer != null)
            {
                SessionCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
                SessionCheckTimer.Dispose();
                SessionCheckTimer = null;
            }
        }

        /// <summary>
        /// Sets the foreground timeout seconds.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public void SetForegroundTimeoutSeconds(long timeout)
        {
            _foregroundTimeout = timeout * 1000;
        }

        /// <summary>
        /// Sets the background timeout seconds.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public void SetBackgroundTimeoutSeconds(long timeout)
        {
            _backgroundTimeout = timeout * 1000;
        }

        /// <summary>
        /// Sets the check interval seconds.
        /// </summary>
        /// <param name="interval">Interval.</param>
        public void SetCheckIntervalSeconds(long interval)
        {
            _checkInterval = interval * 1000;
        }

        /// <summary>
        /// Sets the background truth.
        /// </summary>
        /// <param name="truth">If set to <c>true</c> truth.</param>
        public void SetBackground(bool truth)
        {
            Background = truth;

            // Restart checker if it is coming back to foreground
            if (!Background)
            {
                StartChecker();
            }
        }

        /// <summary>
        /// Gets the foreground timeout.
        /// </summary>
        /// <returns>The foreground timeout.</returns>
        public long GetForegroundTimeout()
        {
            return _foregroundTimeout / 1000;
        }

        /// <summary>
        /// Gets the background timeout.
        /// </summary>
        /// <returns>The background timeout.</returns>
        public long GetBackgroundTimeout()
        {
            return _backgroundTimeout / 1000;
        }

        /// <summary>
        /// Gets the check interval.
        /// </summary>
        /// <returns>The check interval.</returns>
        public long GetCheckInterval()
        {
            return _checkInterval / 1000;
        }

        // --- Private

        /// <summary>
        /// Checks the session.
        /// </summary>
        private void CheckSession(object state)
        {
            _logger.Info("Session: Checking session...");

            long checkTime = Utils.GetTimestamp();
            long range = 0;

            if (Background)
            {
                range = _backgroundTimeout;
            }
            else
            {
                range = _foregroundTimeout;
            }

            if (!Utils.IsTimeInRange(_accessedLast, checkTime, range))
            {
                _logger.Info("Session: Session expired; resetting session.");
                UpdateSession();
                UpdateAccessedLast();
                UpdateSessionDict();
                Utils.WriteDictionaryToFile(this._savePath, _sessionContext.GetData());

                if (Background)
                {
                    _logger.Info("Session: Timeout in background, pausing session checking...");
                    StopChecker();
                    return;
                }
            }

            SessionCheckTimer.Change((int)_checkInterval, Timeout.Infinite);
        }

        /// <summary>
        /// Updates the session.
        /// </summary>
        private void UpdateSession()
        {
            PreviousSessionId = CurrentSessionId;
            CurrentSessionId = Utils.GetGUID();
            SessionIndex++;
            FirstEventId = null;
        }

        /// <summary>
        /// Updates the accessed last.
        /// </summary>
        private void UpdateAccessedLast()
        {
            _accessedLast = Utils.GetTimestamp();
        }

        /// <summary>
        /// Updates the session dict.
        /// </summary>
        private void UpdateSessionDict()
        {
            SessionContext newSessionContext = new SessionContext()
                    .SetUserId(UserId)
                    .SetSessionId(CurrentSessionId)
                    .SetPreviousSessionId(PreviousSessionId)
                    .SetSessionIndex(SessionIndex)
                    .SetStorageMechanism(_sessionStorage)
                    .Build();
            _sessionContext = newSessionContext;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup - stop the session thread
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopChecker();
            }
        }

        /// <summary>
        /// Cleanup - stop the session thread
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        ~ClientSession()
        {
            Close();
        }
    }
}
