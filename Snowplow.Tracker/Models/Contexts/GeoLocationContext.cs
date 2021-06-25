/*
 * GeoLocationContext.cs
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
 * Authors: Fred Blundun, Ed Lewis, Joshua Beemster
 * Copyright: Copyright (c) 2021 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;

namespace Snowplow.Tracker.Models.Contexts
{
    public class GeoLocationContext : AbstractContext<GeoLocationContext>
    {

        /// <summary>
        /// Sets the latitude.
        /// </summary>
        /// <returns>The latitude.</returns>
        /// <param name="latitude">Latitude.</param>
        public GeoLocationContext SetLatitude(double latitude) {
            this.DoAdd (Constants.GEO_LAT, latitude);
            return this;
        }

        /// <summary>
        /// Sets the longitude.
        /// </summary>
        /// <returns>The longitude.</returns>
        /// <param name="longitude">Longitude.</param>
        public GeoLocationContext SetLongitude(double longitude) {
            this.DoAdd (Constants.GEO_LONG, longitude);
            return this;
        }

        /// <summary>
        /// Sets the latitude longitude accuracy.
        /// </summary>
        /// <returns>The latitude longitude accuracy.</returns>
        /// <param name="latitudeLongitudeAccuracy">Latitude longitude accuracy.</param>
        public GeoLocationContext SetLatitudeLongitudeAccuracy(double latitudeLongitudeAccuracy) {
            this.DoAdd (Constants.GEO_LAT_LONG_ACC, latitudeLongitudeAccuracy);
            return this;
        }

        /// <summary>
        /// Sets the altitude.
        /// </summary>
        /// <returns>The altitude.</returns>
        /// <param name="altitude">Altitude.</param>
        public GeoLocationContext SetAltitude(double altitude) {
            this.DoAdd (Constants.GEO_ALT, altitude);
            return this;
        }

        /// <summary>
        /// Sets the altitude accuracy.
        /// </summary>
        /// <returns>The altitude accuracy.</returns>
        /// <param name="altitudeAccuracy">Altitude accuracy.</param>
        public GeoLocationContext SetAltitudeAccuracy(double altitudeAccuracy) {
            this.DoAdd (Constants.GEO_ALT_ACC, altitudeAccuracy);
            return this;
        }

        /// <summary>
        /// Sets the bearing.
        /// </summary>
        /// <returns>The bearing.</returns>
        /// <param name="bearing">Bearing.</param>
        public GeoLocationContext SetBearing(double bearing) {
            this.DoAdd (Constants.GEO_BEARING, bearing);
            return this;
        }

        /// <summary>
        /// Sets the speed.
        /// </summary>
        /// <returns>The speed.</returns>
        /// <param name="speed">Speed.</param>
        public GeoLocationContext SetSpeed(double speed) {
            this.DoAdd (Constants.GEO_SPEED, speed);
            return this;
        }

        /// <summary>
        /// Sets the timestamp.
        /// </summary>
        /// <returns>The timestamp.</returns>
        /// <param name="timestamp">Timestamp.</param>
        public GeoLocationContext SetTimestamp(long timestamp) {
            this.DoAdd (Constants.GEO_TIMESTAMP, timestamp);
            return this;
        }
        
        public override GeoLocationContext Build() {
            Utils.CheckArgument (this.data.ContainsKey(Constants.GEO_LAT), "GeoLocation context requires 'latitude'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.GEO_LONG), "GeoLocation context requires 'longitude'.");
            this.schema = Constants.SCHEMA_GEO_LOCATION;
            this.context = new SelfDescribingJson (this.schema, this.data);
            return this;
        }
    }
}
