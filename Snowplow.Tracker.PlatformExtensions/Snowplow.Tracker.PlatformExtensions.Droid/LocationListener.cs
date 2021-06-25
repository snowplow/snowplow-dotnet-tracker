/*
 * LocationListener.cs
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

using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Locations;

using Snowplow.Tracker.Models.Contexts;

namespace Snowplow.Tracker.PlatformExtensions
{
    /// <summary>
    /// Wrapper for the Android Location Manager
    /// </summary>
    public class LocationListener : Java.Lang.Object, ILocationListener
    {
        private LocationManager _locMgr = null;
        private string _locProvider = string.Empty;
        private GeoLocationContext _geoLocationContext = null;

        /// <summary>
        /// How much time must pass to refresh
        /// </summary>
        public int LocationRefreshTime = 1000 * 60; // 1 minute

        /// <summary>
        /// How much you have to move to incur a refresh
        /// </summary>
        public int LocationRefreshDistance = 10;    // 10 metres

        /// <summary>
        /// Initializes 
        /// </summary>
        public LocationListener()
        {
            _locMgr = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
        }

        /// <summary>
        /// The current GeoLocationContext that has been populated by 
        /// the location manager.
        /// 
        /// If this is null then we fallback to grabbing the last known location.
        /// </summary>
        public GeoLocationContext GetGeoLocationContext()
        {
            if (_geoLocationContext == null && _locProvider != null && _locProvider != string.Empty)
            {
                OnLocationChanged(_locMgr.GetLastKnownLocation(_locProvider));
            }
            return _geoLocationContext;
        }

        /// <summary>
        /// Start gathering location information
        /// </summary>
        public void StartLocationUpdates()
        {
            SetProvider();
            if (_locProvider != null && _locProvider != string.Empty)
            {
                _locMgr.RequestLocationUpdates(_locProvider, LocationRefreshTime, LocationRefreshDistance, this);
            }
        }

        /// <summary>
        /// Stops the location updating
        /// </summary>
        public void StopLocationUpdates()
        {
            _locMgr.RemoveUpdates(this);
        }

        /// <summary>
        /// Updates the set provider.
        /// </summary>
        private void SetProvider()
        {
            if (_locMgr.IsProviderEnabled(LocationManager.GpsProvider))
            {
                _locProvider = LocationManager.GpsProvider;
            }
            else if (_locMgr.IsProviderEnabled(LocationManager.NetworkProvider))
            {
                _locProvider = LocationManager.NetworkProvider;
            }
            else
            {
                IList<string> locationProviders = _locMgr.GetProviders(true);
                if (locationProviders.Any())
                {
                    _locProvider = locationProviders.First();
                }
                else
                {
                    _locProvider = string.Empty;
                }
            }
        }

        // --- ILocationListener Interface

        /// <summary>
        /// Updates the stored GeoLocation context
        /// </summary>
        /// <param name="location"></param>
        public void OnLocationChanged(Location location)
        {
            try
            {
                _geoLocationContext = new GeoLocationContext()
                    .SetLatitude(location.Latitude)
                    .SetLongitude(location.Longitude)
                    .SetAltitude(location.Altitude)
                    .SetLatitudeLongitudeAccuracy(location.Accuracy)
                    .SetSpeed(location.Speed)
                    .SetBearing(location.Bearing)
                    .SetTimestamp(location.Time)
                    .Build();
            }
            catch
            {
                _geoLocationContext = null;
            }
        }

        /// <summary>
        /// If a provider is disabled will reset and restart
        /// the gathering of location information.
        /// </summary>
        /// <param name="provider"></param>
        public void OnProviderDisabled(string provider)
        {
            StopLocationUpdates();
            StartLocationUpdates();
        }

        /// <summary>
        /// if a provider is enabled will reset and restart
        /// the gathering of location information.
        /// </summary>
        /// <param name="provider"></param>
        public void OnProviderEnabled(string provider)
        {
            StopLocationUpdates();
            StartLocationUpdates();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="status"></param>
        /// <param name="extras"></param>
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            StopLocationUpdates();
            StartLocationUpdates();
        }
    }
}
