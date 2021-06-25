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

using Snowplow.Tracker.Models.Contexts;

using UIKit;
using CoreTelephony;

using System;
using System.IO;

namespace Snowplow.Tracker.PlatformExtensions
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class SnowplowTrackerPlatformExtendedImplementation : BaseSnowplowTrackerPlatformExtended
    {
        private LocationManager _locationManager;

        /// <summary>
        /// Attempts to build the GeoLocation Context.
        /// </summary>
        /// <returns>The geo-location context or null</returns>
        public override GeoLocationContext GetGeoLocationContext()
        {
            if (_locationManager == null)
            {
                _locationManager = new LocationManager();
                _locationManager.StartLocationUpdates();
            }
            return _locationManager.GeoLocationContext;
        }

        /// <summary>
        /// Stops the location updater service if it is running.
        /// </summary>
        public override void StopLocationUpdates()
        {
            if (_locationManager != null)
            {
                _locationManager.StopLocationUpdates();
                _locationManager.Dispose();
                _locationManager = null;
            }
        }

        /// <summary>
        /// Returns the path to a valid internal folder
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>the file path</returns>
        public override string GetLocalFilePath(string filename)
        {
            string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

            if (!Directory.Exists(libFolder))
            {
                Directory.CreateDirectory(libFolder);
            }

            return Path.Combine(libFolder, filename);
        }

        /// <summary>
        /// Will check whether or not the device has an active internet
        /// connection.
        /// </summary>
        /// <returns>The state of the connection</returns>
        public override bool IsDeviceOnline()
        {
            switch (Reachability.InternetConnectionStatus())
            {
                case NetworkStatus.NotReachable:
                    return false;
                default: return true;
            }
        }

        /// <summary>
        /// Returns the os type.
        /// </summary>
        /// <returns></returns>
        public override string GetOsType()
        {
            return "ios";
        }

        /// <summary>
        /// Returns the version of the OS.
        /// </summary>
        /// <returns></returns>
        public override string GetOsVersion()
        {
            return UIDevice.CurrentDevice.SystemVersion;
        }

        /// <summary>
        /// Returns the device manufacturer.
        /// </summary>
        /// <returns></returns>
        public override string GetDeviceManufacturer()
        {
            return "Apple Inc.";
        }

        /// <summary>
        /// Returns the device model.
        /// </summary>
        /// <returns></returns>
        public override string GetDeviceModel()
        {
            return UIDevice.CurrentDevice.Model;
        }

        /// <summary>
        /// Returns the name of the service provider.
        /// </summary>
        /// <returns></returns>
        public override string GetCarrier()
        {
            try
            {
                var networkInfo = new CTTelephonyNetworkInfo();
                var carrier = networkInfo.SubscriberCellularProvider;
                return carrier.CarrierName;
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
        public override NetworkType GetNetworkType()
        {
            switch (Reachability.InternetConnectionStatus())
            {
                case NetworkStatus.NotReachable:
                    return NetworkType.Offline;
                case NetworkStatus.ReachableViaCarrierDataNetwork:
                    return NetworkType.Mobile;
                case NetworkStatus.ReachableViaWiFiNetwork:
                    return NetworkType.Wifi;
                default: return null;
            }
        }

        /// <summary>
        /// If the mobile is connected to a mobile network then it will
        /// return the network technology being used.
        /// </summary>
        /// <returns></returns>
        public override string GetNetworkTechnology()
        {
            try
            {
                var networkInfo = new CTTelephonyNetworkInfo();
                return networkInfo.CurrentRadioAccessTechnology;
            }
            catch
            {
                return "";
            }
        }
    }
}
