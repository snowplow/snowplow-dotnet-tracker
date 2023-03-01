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

using Xamarin.Forms;

namespace Snowplow.Demo.App.Pages.Components
{
    class ActionImageCell : ViewCell
    {

        /// <summary>
        /// Creates a cell with a single small label and a right aligned
        /// image.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="imagePath"></param>
        public ActionImageCell(string label, ImageSource imageSource)
        {
            var navLabel = new Label
            {
                Text = label,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.FillAndExpand,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            };

            var navImage = new Image
            {
                Aspect = Aspect.AspectFit,
                Source = imageSource,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
            };

            var gridLayout = new Grid
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(16, 6, 8, 6)
            };
            gridLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            gridLayout.Children.Add(navLabel);
            gridLayout.Children.Add(navImage, 1, 0);

            View = gridLayout;
        }
    }
}
