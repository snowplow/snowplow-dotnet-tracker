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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Models.Contexts;
using System;

namespace Snowplow.Tracker.Tests.Models.Contexts
{
    [TestClass]
    public class GeoLocationContextTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"GeoLocation context requires 'latitude'.")]
        public void testInitGeoLocationContextWithNullLatitude()
        {
            new GeoLocationContext().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), @"GeoLocation context requires 'longitude'.")]
        public void testInitGeoLocationContextWithNullLongitude()
        {
            new GeoLocationContext()
                .SetLatitude(20.0)
                .Build();
        }

        [TestMethod]
        public void testInitGeoLocationContext()
        {
            var glc = new GeoLocationContext()
                .SetLatitude(10.0)
                .SetLongitude(-10.5)
                .SetLatitudeLongitudeAccuracy(2564.734124)
                .SetAltitude(300)
                .SetAltitudeAccuracy(2)
                .SetBearing(20.34)
                .SetSpeed(0.0)
                .SetTimestamp(123456789123)
                .Build();

            Assert.IsNotNull(glc);
            Assert.AreEqual(@"{""schema"":""iglu:com.snowplowanalytics.snowplow/geolocation_context/jsonschema/1-1-0"",""data"":{""latitude"":10.0,""longitude"":-10.5,""latitudeLongitudeAccuracy"":2564.734124,""altitude"":300.0,""altitudeAccuracy"":2.0,""bearing"":20.34,""speed"":0.0,""timestamp"":123456789123}}", glc.GetJson().ToString());
        }
    }
}
