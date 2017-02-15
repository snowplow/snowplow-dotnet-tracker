/*
 * CustomKeyValueCell.cs
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

using Xamarin.Forms;

namespace Snowplow.Demo.App.Pages.Components
{
    class KeyValueCell : ViewCell
    {
        public Label ValueLabel { get; set; }

        /// <summary>
        /// Creates a cell with a key label and a value label.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public KeyValueCell(string key, string value)
        {
            var keyLabel = new Label
            {
                Text = key,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            };

            ValueLabel = new Label
            {
                Text = value,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            };

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Padding = new Thickness(16, 6, 8, 6),
                Children =
                {
                    keyLabel,
                    ValueLabel
                }
            };

            View = stackLayout;
        }
    }
}
