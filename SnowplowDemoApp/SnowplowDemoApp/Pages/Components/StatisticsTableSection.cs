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

using System;

using Snowplow.Tracker.PlatformExtensions;
using System.Threading;

using Xamarin.Forms;

namespace SnowplowDemoApp.Pages.Components
{
	public class StatisticsTableSection
	{
        public TableSection Section { get; private set; }

        // --- UI Elements

        private KeyValueCell _trackerRunningTextCell;
        private KeyValueCell _deviceOnlineTextCell;
        private KeyValueCell _eventsMadeTextCell;
        private KeyValueCell _eventsSuccessCount;
        private KeyValueCell _eventsFailureCount;
        private KeyValueCell _eventsDatabaseCount;
        private KeyValueCell _sessionIndex;

        private Timer _statisticUpdater;

        // --- Constructor

        public StatisticsTableSection()
        {
            _trackerRunningTextCell = new KeyValueCell("Tracker running:", "");
            _deviceOnlineTextCell = new KeyValueCell("Device online:", "");
            _eventsMadeTextCell = new KeyValueCell("Events made:", "");
            _eventsSuccessCount = new KeyValueCell("Success count:", "");
            _eventsFailureCount = new KeyValueCell("Failure count:", "");
            _eventsDatabaseCount = new KeyValueCell("DB size:", "");
            _sessionIndex = new KeyValueCell("Session #:", "");

            Section = new TableSection("Statistics")
            {
                _trackerRunningTextCell,
                _deviceOnlineTextCell,
                _eventsMadeTextCell,
                _eventsSuccessCount,
                _eventsFailureCount,
                _eventsDatabaseCount,
                _sessionIndex
            };
        }

        /// <summary>
        /// Starts the session checker.
        /// </summary>
        public void StartUpdater()
        {
            if (_statisticUpdater == null)
            {
                _statisticUpdater = new Timer(UpdateStatistics, null, 1000, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Stops the session checker.
        /// </summary>
        public void StopUpdater()
        {
            if (_statisticUpdater != null)
            {
                _statisticUpdater.Change(Timeout.Infinite, Timeout.Infinite);
                _statisticUpdater.Dispose();
                _statisticUpdater = null;
            }
        }

        /// <summary>
        /// Updates the statistics
        /// </summary>
        public void UpdateStatistics(object state = null)
        {
            Device.BeginInvokeOnMainThread(() => {
                _trackerRunningTextCell.ValueLabel.Text = "" + Snowplow.Tracker.Tracker.Instance.Started;
                _deviceOnlineTextCell.ValueLabel.Text = "" + SnowplowTrackerPlatformExtension.Current.IsDeviceOnline();
                _eventsMadeTextCell.ValueLabel.Text = "" + Analytics.Snowplow.SessionMadeCount;
                _eventsSuccessCount.ValueLabel.Text = "" + Analytics.Snowplow.SessionSuccessCount;
                _eventsFailureCount.ValueLabel.Text = "" + Analytics.Snowplow.SessionFailureCount;
                _eventsDatabaseCount.ValueLabel.Text = "" + Analytics.Snowplow.GetDatabaseEventCount();
                _sessionIndex.ValueLabel.Text = "" + Analytics.Snowplow.GetClientSessionIndexCount();
            });

            try
            {
                _statisticUpdater.Change(1000, Timeout.Infinite);
            }
            catch
            {
                // Potential race condition
            }
        }
    }
}

