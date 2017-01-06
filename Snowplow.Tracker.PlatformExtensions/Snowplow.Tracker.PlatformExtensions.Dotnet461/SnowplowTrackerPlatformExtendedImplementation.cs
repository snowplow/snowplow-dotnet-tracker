/*
 * SnowplowTrackerPlatformExtendedImplementation.cs
 * 
 * Copyright (c) 2017 Snowplow Analytics Ltd. All rights reserved.
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
 * Copyright: Copyright (c) 2017 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using Snowplow.Tracker.PlatformExtensions.Abstractions;

using Snowplow.Tracker.Models.Contexts;

namespace Snowplow.Tracker.PlatformExtensions
{
    public class SnowplowTrackerPlatformExtendedImplementation : BaseSnowplowTrackerPlatformExtended
    {
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        /// <summary>
        /// Attempts to build the GeoLocation Context.
        /// </summary>
        /// <returns>The geo-location context or null</returns>
        public override GeoLocationContext GetGeoLocationContext()
        {
            return null;
        }

        /// <summary>
        /// Stops the location updater service if it is running.
        /// </summary>
        public override void StopLocationUpdates()
        {

        }

        /// <summary>
        /// Returns a platform safe path for storing internal files.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>The filename appended to the folder path</returns>
        public override string GetLocalFilePath(string filename)
        {
            return "";
        }

        /// <summary>
        /// Will check whether or not the device has an active internet
        /// connection.
        /// </summary>
        /// <returns>The state of the connection</returns>
        public override bool IsDeviceOnline()
        {
            int desc;
            return InternetGetConnectedState(out desc, 0);
        }

        /// <returns>The OS type</returns>
        public override string GetOsType()
        {
            return "";
        }

        /// <returns>The OS version</returns>
        public override string GetOsVersion()
        {
            return "";
        }

        /// <returns>The device manufacturer</returns>
        public override string GetDeviceManufacturer()
        {
            return "";
        }

        /// <returns>The device model</returns>
        public override string GetDeviceModel()
        {
            return "";
        }

        /// <returns>The mobile carrier</returns>
        public override string GetCarrier()
        {
            return "";
        }

        /// <returns>The current network type in use</returns>
        public override NetworkType GetNetworkType()
        {
            return null;
        }

        /// <returns>The current network technology in use</returns>
        public override string GetNetworkTechnology()
        {
            return "";
        }
    }
}
