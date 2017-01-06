/*
 * ISnowplowTrackerPlatformExtended.cs
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

using Snowplow.Tracker.Models.Contexts;

namespace Snowplow.Tracker.PlatformExtensions.Abstractions
{
    /// <summary>
    /// Snowplow Xamarin functions
    /// </summary>
    public interface ISnowplowTrackerPlatformExtended
    {
        /// <summary>
        /// Attempts to build the Mobile Context.
        /// </summary>
        /// <returns>The mobile context or null</returns>
        MobileContext GetMobileContext();

        /// <summary>
        /// Attempts to build the GeoLocation Context.
        /// </summary>
        /// <returns>The geo-location context or null</returns>
        GeoLocationContext GetGeoLocationContext();

        /// <summary>
        /// Stops the location updater service if it is running.
        /// </summary>
        void StopLocationUpdates();

        /// <summary>
        /// Returns a platform safe path for storing internal files.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>The filename appended to the folder path</returns>
        string GetLocalFilePath(string filename);

        /// <summary>
        /// Will check whether or not the device has an active internet
        /// connection.
        /// </summary>
        /// <returns>The state of the connection</returns>
        bool IsDeviceOnline();

        /// <returns>The OS type</returns>
        string GetOsType();

        /// <returns>The OS version</returns>
        string GetOsVersion();

        /// <returns>The device manufacturer</returns>
        string GetDeviceManufacturer();

        /// <returns>The device model</returns>
        string GetDeviceModel();

        /// <returns>The mobile carrier</returns>
        string GetCarrier();

        /// <returns>The current network type in use</returns>
        NetworkType GetNetworkType();

        /// <returns>The current network technology in use</returns>
        string GetNetworkTechnology();
    }
}
