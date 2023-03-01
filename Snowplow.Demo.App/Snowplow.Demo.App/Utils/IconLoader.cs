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

namespace Snowplow.Demo.App.Utils
{
    public enum Icon
    {
        Nav,
        Send
    };

    public static class IconLoader
    {

        /// <summary>
        /// Loads an icon from file.
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource Load(Icon icon)
        {
            string iconPath = null;

            switch (icon)
            {
                case Icon.Nav: iconPath = "ic_chevron_right.png";
                    break;
                case Icon.Send: iconPath = "ic_send.png";
                    break;
                default: break;
            }

            return ImageSource.FromFile(iconPath);
        }
    }
}
