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

using Snowplow.Tracker;
using SnowplowDemoApp.Pages;
using Xamarin.Forms;

namespace SnowplowDemoApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new AppNavigator("Snowplow Demo"));
        }

        // --- Lifecycle

        protected override void OnStart()
        {
            if (Tracker.Instance.Started)
            {
                Tracker.Instance.SetBackground(false);
            }
        }

        protected override void OnSleep()
        {
            if (Tracker.Instance.Started)
            {
                Tracker.Instance.SetBackground(true);
            }
        }

        protected override void OnResume()
        {
            if (Tracker.Instance.Started)
            {
                Tracker.Instance.SetBackground(false);
            }
        }
    }
}
