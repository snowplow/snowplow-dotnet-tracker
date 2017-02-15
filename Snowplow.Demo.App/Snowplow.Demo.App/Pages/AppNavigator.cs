/*
 * AppNavigator.cs
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
    public class AppNavigator : ContentPage
    {

        // --- Constructor

        public AppNavigator(string title)
        {
            Title = title;
            Content = GetNavigation();
        }

        // --- Components

        private StackLayout GetNavigation()
        {
            var icNavImage = IconLoader.Load(Utils.Icon.Nav);

            var settingsPageNav = new ActionImageCell("Tracker setup", icNavImage);
            settingsPageNav.Tapped += OnSettingsClicked;
            var singleEventsPageNav = new ActionImageCell("Single event tracking", icNavImage);
            singleEventsPageNav.Tapped += OnSingleEventsClicked;
            var floodEventsPageNav = new ActionImageCell("Flood event tracking", icNavImage);
            floodEventsPageNav.Tapped += OnFloodEventsClicked;
            var helpPageNav = new ActionImageCell("Help", icNavImage);
            helpPageNav.Tapped += OnHelpClicked;

            var navigationTable = new TableView
            {
                Intent = TableIntent.Settings,
                Root = new TableRoot
                {
                    new TableSection("Settings")
                    {
                        settingsPageNav
                    },
                    new TableSection("Tracking")
                    {
                        singleEventsPageNav,
                        floodEventsPageNav
                    },
                    new TableSection("Misc")
                    {
                        helpPageNav
                    }
                }
            };

            return new StackLayout
            {
                Children = { navigationTable },
                VerticalOptions = LayoutOptions.FillAndExpand
            };
        }

        // --- Navigation

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PushAsync(new TrackerSetup());
        }

        private async void OnSingleEventsClicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PushAsync(new SingleEventTracking());
        }

        private async void OnFloodEventsClicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PushAsync(new FloodTracking());
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PushAsync(new Help());
        }

        // --- Lifecycle

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            GC.Collect();
        }
    }
}
