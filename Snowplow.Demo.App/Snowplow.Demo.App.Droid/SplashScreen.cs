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

using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;

namespace Snowplow.Demo.App.Droid
{
    [Activity(Label = "Xamarin Tracker", MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Splash", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
        }
    }
}
