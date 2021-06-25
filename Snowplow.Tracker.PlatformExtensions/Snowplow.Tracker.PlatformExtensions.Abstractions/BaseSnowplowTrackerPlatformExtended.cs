/*
 * BaseSnowplowTrackerPlatformExtended.cs
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

using Snowplow.Tracker.Models.Contexts;

namespace Snowplow.Tracker.PlatformExtensions.Abstractions
{
    /// <summary>
    /// Base implementation of the Snowplow Xamarin functions
    /// </summary>
    public abstract class BaseSnowplowTrackerPlatformExtended : ISnowplowTrackerPlatformExtended
    {
        /// <summary>
        /// Attempts to build the Mobile Context.
        /// </summary>
        /// <returns>The mobile context or null</returns>
        public MobileContext GetMobileContext()
        {
            try
            {
                return new MobileContext()
                    .SetOsType(GetOsType())
                    .SetOsVersion(GetOsVersion())
                    .SetDeviceManufacturer(GetDeviceManufacturer())
                    .SetDeviceModel(GetDeviceModel())
                    .SetCarrier(GetCarrier())
                    .SetNetworkType(GetNetworkType())
                    .SetNetworkTechnology(GetNetworkTechnology())
                    .Build();
            }
            catch
            {
                return null;
            }
        }

        // --- Abstract

        /// <summary>
        /// Attempts to build the GeoLocation Context.
        /// </summary>
        /// <returns>The geo-location context or null</returns>
        public abstract GeoLocationContext GetGeoLocationContext();

        /// <summary>
        /// Stops the location updater service if it is running.
        /// </summary>
        public abstract void StopLocationUpdates();

        /// <summary>
        /// Returns a platform safe path for storing internal files.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>The filename appended to the folder path</returns>
        public abstract string GetLocalFilePath(string filename);

        /// <summary>
        /// Will check whether or not the device has an active internet
        /// connection.
        /// </summary>
        /// <returns>The state of the connection</returns>
        public abstract bool IsDeviceOnline();

        /// <returns>The OS type</returns>
        public abstract string GetOsType();

        /// <returns>The OS version</returns>
        public abstract string GetOsVersion();

        /// <returns>The device manufacturer</returns>
        public abstract string GetDeviceManufacturer();

        /// <returns>The device model</returns>
        public abstract string GetDeviceModel();

        /// <returns>The mobile carrier</returns>
        public abstract string GetCarrier();

        /// <returns>The current network type in use</returns>
        public abstract NetworkType GetNetworkType();

        /// <returns>The current network technology in use</returns>
        public abstract string GetNetworkTechnology();
    }
}
