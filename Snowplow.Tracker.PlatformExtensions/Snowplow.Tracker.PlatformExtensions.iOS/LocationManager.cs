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

using System;

using Snowplow.Tracker.Models.Contexts;

using CoreLocation;
using Foundation;
using UIKit;

namespace Snowplow.Tracker.PlatformExtensions
{
    /// <summary>
    /// Manages location information
    /// </summary>
    public class LocationManager : IDisposable
    {
        private CLLocationManager _locMgr;

        /// <summary>
        /// Whether we have authorization to access location
        /// </summary>
        public bool Authorized { get; private set; }  = false;

        /// <summary>
        /// The desired accuracy in metres that we want the location
        /// to be within
        /// </summary>
        public int DesiredAccuracy { get; set; } = 10;

        /// <summary>
        /// The current GeoLocationContext that has been populated by 
        /// the location manager.
        /// </summary>
        public GeoLocationContext GeoLocationContext { get; private set; } = null;

        /// <summary>
        /// Creates a new location manager
        /// </summary>
        public LocationManager()
        {
            _locMgr = new CLLocationManager();
            _locMgr.PausesLocationUpdatesAutomatically = false;
            _locMgr.AuthorizationChanged += (sender, args) => {
                Authorized = args.Status == CLAuthorizationStatus.Authorized;
            };

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _locMgr.RequestAlwaysAuthorization();
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                _locMgr.AllowsBackgroundLocationUpdates = true;
            }
        }

        /// <summary>
        /// Start gathering location information
        /// </summary>
        public void StartLocationUpdates()
        {
            if (CLLocationManager.LocationServicesEnabled)
            {
                _locMgr.DesiredAccuracy = DesiredAccuracy;
                _locMgr.LocationsUpdated += (sender, e) =>
                {
                    LocationUpdated(e.Locations[e.Locations.Length - 1]);
                };
                _locMgr.StartUpdatingLocation();
            }
        }

        /// <summary>
        /// Stop updating location
        /// </summary>
        public void StopLocationUpdates()
        {
            if (CLLocationManager.LocationServicesEnabled)
            {
                _locMgr.StopUpdatingLocation();
            }
        }

        private void LocationUpdated(CLLocation location)
        {
            try
            {
                GeoLocationContext = new GeoLocationContext()
                    .SetLatitude(location.Coordinate.Latitude)
                    .SetLongitude(location.Coordinate.Longitude)
                    .SetAltitude(location.Altitude)
                    .SetLatitudeLongitudeAccuracy(location.HorizontalAccuracy)
                    .SetSpeed(location.Speed)
                    .SetBearing(location.Course)
                    .SetTimestamp(NSDateToUnixEpoch(location.Timestamp))
                    .Build();
            }
            catch
            {
                GeoLocationContext = null;
            }
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private long NSDateToUnixEpoch(NSDate date)
        {
            NSCalendar cal = NSCalendar.CurrentCalendar;
            int year = (int)cal.GetComponentFromDate(NSCalendarUnit.Year, date);
            int month = (int)cal.GetComponentFromDate(NSCalendarUnit.Month, date);
            int day = (int)cal.GetComponentFromDate(NSCalendarUnit.Day, date);
            int hour = (int)cal.GetComponentFromDate(NSCalendarUnit.Hour, date);
            int minute = (int)cal.GetComponentFromDate(NSCalendarUnit.Minute, date);
            int second = (int)cal.GetComponentFromDate(NSCalendarUnit.Second, date);
            int nanosecond = (int)cal.GetComponentFromDate(NSCalendarUnit.Nanosecond, date);

            int millisecond = (nanosecond / 1000000);

            var local = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Local);

            DateTime utc = local.ToUniversalTime();
            return (long) (utc - UnixEpoch).TotalMilliseconds;
        }

        /// <summary>
        /// Disposes of held resources
        /// </summary>
        public void Dispose()
        {
            _locMgr.Dispose();
            _locMgr = null;
        }
    }
}
