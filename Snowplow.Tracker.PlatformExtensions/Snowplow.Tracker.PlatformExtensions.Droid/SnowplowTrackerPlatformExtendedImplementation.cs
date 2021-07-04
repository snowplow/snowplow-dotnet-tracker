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

using Snowplow.Tracker.PlatformExtensions.Abstractions;

using System.IO;

using Android.OS;
using Android.Telephony;
using Android.Net;

using Snowplow.Tracker.Models.Contexts;

namespace Snowplow.Tracker.PlatformExtensions
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class SnowplowTrackerPlatformExtendedImplementation : BaseSnowplowTrackerPlatformExtended
    {
        private LocationListener _locationListener = null;

        /// <summary>
        /// Attempts to build the GeoLocation Context.
        /// </summary>
        /// <returns>The geo-location context or null</returns>
        public override GeoLocationContext GetGeoLocationContext()
        {
            if (_locationListener == null)
            {
                _locationListener = new LocationListener();
                _locationListener.StartLocationUpdates();
            }
            return _locationListener.GetGeoLocationContext();
        }

        /// <summary>
        /// Stops the location updater service if it is running.
        /// </summary>
        public override void StopLocationUpdates()
        {
            if (_locationListener != null)
            {
                _locationListener.StopLocationUpdates();
                _locationListener.Dispose();
                _locationListener = null;
            }
        }

        /// <summary>
        /// Returns the path to a valid internal folder
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>the file path</returns>
        public override string GetLocalFilePath(string filename)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            return Path.Combine(path, filename);
        }

        /// <summary>
        /// Will check whether or not the device has an active internet
        /// connection.
        /// </summary>
        /// <returns>The state of the connection</returns>
        public override bool IsDeviceOnline()
        {
            try
            {
                var ni = GetNetworkInfo();
                return ni != null && ni.IsConnectedOrConnecting;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the os type.
        /// </summary>
        /// <returns></returns>
        public override string GetOsType()
        {
            return "android";
        }

        /// <summary>
        /// Returns the version of the OS.
        /// </summary>
        /// <returns></returns>
        public override string GetOsVersion()
        {
            return Build.VERSION.Release;
        }

        /// <summary>
        /// Returns the device manufacturer.
        /// </summary>
        /// <returns></returns>
        public override string GetDeviceManufacturer()
        {
            return Build.Manufacturer;
        }

        /// <summary>
        /// Returns the device model.
        /// </summary>
        /// <returns></returns>
        public override string GetDeviceModel()
        {
            return Build.Model;
        }

        /// <summary>
        /// Returns the name of the service provider.
        /// </summary>
        /// <returns></returns>
        public override string GetCarrier()
        {
            try 
            {
                var tm = (TelephonyManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.TelephonyService);
                return tm != null ? tm.NetworkOperatorName : "";
            }
            catch 
            {
                return "";
            }
        }

        /// <summary>
        /// Returns the type of network the mobile is connected to.
        /// </summary>
        /// <returns></returns>
        public override Models.Contexts.NetworkType GetNetworkType()
        {
            var ni = GetNetworkInfo();
            Models.Contexts.NetworkType networkType = Snowplow.Tracker.Models.Contexts.NetworkType.Offline;

            if (ni != null)
            {
                var maybeNetworkType = ni.TypeName.ToLower();
                switch (maybeNetworkType)
                {
                    case "mobile":
                        networkType = Models.Contexts.NetworkType.Mobile;
                        break;
                    case "wifi":
                        networkType = Models.Contexts.NetworkType.Wifi;
                        break;
                    default: break;
                }
            }

            return networkType;
        }

        /// <summary>
        /// If the mobile is connected to a mobile network then it will
        /// return the network technology being used.
        /// </summary>
        /// <returns></returns>
        public override string GetNetworkTechnology()
        {
            var ni = GetNetworkInfo();
            string networkTechnology = "";

            if (ni != null)
            {
                if (ni.TypeName.ToLower() == "mobile")
                {
                    networkTechnology = ni.SubtypeName;
                }
            }

            return networkTechnology;
        }

        // --- Helpers

        /// <summary>
        /// Fetches the network information object
        /// </summary>
        /// <returns>gets network info or null</returns>
        private NetworkInfo GetNetworkInfo()
        {
            try
            {
                var cm = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
                return cm.ActiveNetworkInfo;
            }
            catch
            {
                return null;
            }
        }
    }
}
