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

using Snowplow.Demo.App.Pages.Components;
using Snowplow.Demo.App.Utils;

using System;

using Xamarin.Forms;

namespace Snowplow.Demo.App.Pages
{
    public class FloodTracking : ContentPage
    {
        private StatisticsTableSection _statisticsTableSection;

        public FloodTracking()
        {
            Title = "Flood tracking";
            Content = new StackLayout
            {
                Children = {
                    BuildTrackFunctionsView()
                }
            };
        }

        // --- Components

        /// <summary>
        /// Constructs the Table responsible for sending events and
        /// showing statistics about the Tracker
        /// </summary>
        /// <returns></returns>
        private StackLayout BuildTrackFunctionsView()
        {
            // --- Send Buttons

            var floodCell = new ActionImageCell("Send all event types", IconLoader.Load(Utils.Icon.Send));
            floodCell.Tapped += (s, e) => { TrackEvents(); };

            // --- Statistics

            _statisticsTableSection = new StatisticsTableSection();
            _statisticsTableSection.StartUpdater();
            _statisticsTableSection.UpdateStatistics();

            // --- Assemblew View

            var settingsTable = new TableView
            {
                Intent = TableIntent.Settings,
                Root = new TableRoot
                {
                    new TableSection("Track Functions") { floodCell },
                    _statisticsTableSection.Section
                }
            };

            return new StackLayout
            {
                Children = { settingsTable },
                VerticalOptions = LayoutOptions.FillAndExpand
            };
        }

        /// <summary>
        /// Tracks all event types
        /// </summary>
        private async void TrackEvents()
        {
            try
            {
                Analytics.Snowplow.TrackSelfDescribing();
                Analytics.Snowplow.TrackPageView();
                Analytics.Snowplow.TrackScreenView();
                Analytics.Snowplow.TrackTiming();
                Analytics.Snowplow.TrackStructEvent();
                Analytics.Snowplow.TrackEcommerceTransaction();

                _statisticsTableSection.UpdateStatistics();
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "OK");
            }
        }

        // --- Lifecycle

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _statisticsTableSection.StopUpdater();
            GC.Collect();
        }
    }
}
