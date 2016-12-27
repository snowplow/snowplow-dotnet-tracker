/*
 * Events.cs
 * 
 * Copyright (c) 2016 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Joshua Beemster
 * Copyright: Copyright (c) 2016 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using Snowplow.Demo.App.Pages.Components;
using Snowplow.Demo.App.Utils;

using System;

using Xamarin.Forms;

namespace Snowplow.Demo.App.Pages
{
    public class SingleEventTracking : ContentPage
    {

        private StatisticsTableSection _statisticsTableSection;

        public SingleEventTracking()
        {
            Title = "Single event tracking";
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

            var icSendImage = IconLoader.Load(Utils.Icon.Send);

            var selfDescribingCell = new ActionImageCell("TrackSelfDescribing", icSendImage);
            selfDescribingCell.Tapped += (s, e) => { TrackEvent(0); };
            var pageViewCell = new ActionImageCell("TrackPageView", icSendImage);
            pageViewCell.Tapped += (s, e) => { TrackEvent(1); };
            var screenViewCell = new ActionImageCell("TrackScreenView", icSendImage);
            screenViewCell.Tapped += (s, e) => { TrackEvent(2); };
            var timingCell = new ActionImageCell("TrackTiming", icSendImage);
            timingCell.Tapped += (s, e) => { TrackEvent(3); };
            var structCell = new ActionImageCell("TrackStructEvent", icSendImage);
            structCell.Tapped += (s, e) => { TrackEvent(4); };
            var ecommerceTransactionCell = new ActionImageCell("TrackEcommerceTransaction", icSendImage);
            ecommerceTransactionCell.Tapped += (s, e) => { TrackEvent(5); };

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
                    new TableSection("Event Track Functions")
                    {
                        selfDescribingCell,
                        pageViewCell,
                        screenViewCell,
                        timingCell,
                        structCell,
                        ecommerceTransactionCell
                    },
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
        /// Tracks an event defined by an index
        /// </summary>
        /// <param name="index"></param>
        private async void TrackEvent(int index)
        {
            try
            {
                switch(index)
                {
                    case 0: Analytics.Snowplow.TrackSelfDescribing(); break;
                    case 1: Analytics.Snowplow.TrackPageView(); break;
                    case 2: Analytics.Snowplow.TrackScreenView(); break;
                    case 3: Analytics.Snowplow.TrackTiming(); break;
                    case 4: Analytics.Snowplow.TrackStructEvent(); break;
                    case 5: Analytics.Snowplow.TrackEcommerceTransaction(); break;
                    default: throw new ArgumentException("Event tracking index not yet supported!");
                }

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
