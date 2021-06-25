/*
 * Help.cs
 * 
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
 * Authors: Joshua Beemster
 * Copyright: Copyright (c) 2021 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using Xamarin.Forms;

namespace Snowplow.Demo.App.Pages
{
    public class Help : ContentPage
    {

        // --- Constructor

        public Help()
        {
            Title = "Help";
            Content = GetAboutView();
        }

        // --- Components

        private ContentView GetAboutView()
        {
            var aboutLabel = new Label
            {
                Text = "The Snowplow Xamarin Tracker Demo is designed as a proof of concept for using the Tracker within the Xamarin environment.\n\n" +
                       "To get started you will first need to configure and start the Tracker on the 'Tracker setup' page.\n\n" +
                       "Once the Tracker is started you can start sending events!  You can track individual event types or simply send a flood of all of the events to your endpoint."
            };

            return new ContentView
            {
                Padding = new Thickness(10, 10, 10, 10),
                Content = aboutLabel
            };
        }

        // --- Lifecycle

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            GC.Collect();
        }
    }
}
