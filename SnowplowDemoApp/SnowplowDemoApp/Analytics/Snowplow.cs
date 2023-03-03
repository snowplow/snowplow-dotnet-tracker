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

using SnowplowCore = Snowplow.Tracker;
using System;
using Snowplow.Tracker.Emitters;
using Snowplow.Tracker.Endpoints;
using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Models.Events;
using Snowplow.Tracker.PlatformExtensions;
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Storage;
using static Snowplow.Tracker.Tracker;
using System.Collections.Generic;

namespace SnowplowDemoApp.Analytics
{
    public static class Snowplow
    {
        private static readonly string KEY_USER_ID = "userId";

        private static readonly string _trackerNamespace = "SnowplowXamarinTrackerDemo";
        private static readonly string _appId = "DemoID";
        private static ClientSession _clientSession;
        private static LiteDBStorage _storage;

        public static int SessionMadeCount { get; private set; }
        public static int SessionSuccessCount { get; private set; }
        public static int SessionFailureCount { get; private set; }

        /// <summary>
        /// Inits the Snowplow Tracker; after this point it can be accessed globally.
        /// </summary>
        /// <param name="emitterUri">The emitter URI</param>
        /// <param name="protocol">The protocol to use</param>
        /// <param name="port">The port the collector is on</param>
        /// <param name="method">The method to use</param>
        /// <param name="useClientSession">Whether to enable client session</param>
        /// <param name="useMobileContext">Whether to enable mobile contexts</param>
        /// <param name="useGeoLocationContext">Whether to enable geo-location contexts</param>
        public static void Init(
            string emitterUri,
            HttpProtocol protocol = HttpProtocol.HTTP,
            int? port = null,
            HttpMethod method = HttpMethod.GET,
            bool useClientSession = false,
            bool useMobileContext = false,
            bool useGeoLocationContext = false)
        {
            var logger = new ConsoleLogger();

            var dest = new SnowplowHttpCollectorEndpoint(emitterUri, method: method, port: port, protocol: protocol, l: logger);

            // Note: Maintain reference to Storage as this will need to be disposed of manually
            _storage = new LiteDBStorage(SnowplowTrackerPlatformExtension.Current.GetLocalFilePath("events.db"));
            var queue = new PersistentBlockingQueue(_storage, new PayloadToJsonString());

            // Note: When using GET requests the sendLimit equals the number of concurrent requests - to many of these will break your application!
            var sendLimit = method == HttpMethod.GET ? 10 : 100;

            // Note: To make the tracker more battery friendly and less likely to drain batteries there are two settings to take note of here:
            //       1. The stopPollIntervalMs: Controls how often we look to the database for more events
            //       2. The deviceOnlineMethod: Is run before going to the database or attempting to send events, this will prevent any I/O from
            //          occurring unless you have an active network connection
            var emitter = new AsyncEmitter(dest, queue, sendLimit: sendLimit, stopPollIntervalMs: 1000, sendSuccessMethod: EventSuccessCallback,
                deviceOnlineMethod: () => true, l: logger);

            var userId = Utils.PropertyManager.GetStringValue(KEY_USER_ID, SnowplowCore.Utils.GetGUID());
            Utils.PropertyManager.SaveKeyValue(KEY_USER_ID, userId);

            var subject = new Subject()
                .SetPlatform(Platform.Mob)
                .SetUserId(userId)
                .SetLang("en");

            if (useClientSession)
            {
                _clientSession = new ClientSession(SnowplowTrackerPlatformExtension.Current.GetLocalFilePath("client_session.dict"), l: logger);
            }

            // Note: You can either attach contexts to each event individually or for the more common contexts such as Desktop, Mobile and GeoLocation
            //       you can pass a delegate method which will then be called for each event automatically.

            MobileContextDelegate mobileContextDelegate = null;
            if (useMobileContext)
            {
                mobileContextDelegate = SnowplowTrackerPlatformExtension.Current.GetMobileContext;
            }

            GeoLocationContextDelegate geoLocationContextDelegate = null;
            if (useMobileContext)
            {
                geoLocationContextDelegate = SnowplowTrackerPlatformExtension.Current.GetGeoLocationContext;
            }

            // Attach the created objects and begin all required background threads!
            Instance.Start(emitter: emitter, subject: subject, clientSession: _clientSession, trackerNamespace: _trackerNamespace,
                appId: _appId, encodeBase64: false, synchronous: false, mobileContextDelegate: mobileContextDelegate,
                geoLocationContextDelegate: geoLocationContextDelegate, l: logger);

            // Reset session counters
            SessionMadeCount = 0;
            SessionSuccessCount = 0;
            SessionFailureCount = 0;
        }

        /// <summary>
        /// Halts the Tracker
        /// </summary>
        public static void Shutdown()
        {
            // Note: This will also stop the ClientSession and Emitter objects for you!
            Instance.Stop();

            // Note: Dispose of Storage to remove lock on database file!
            if (_storage != null)
            {
                _storage.Dispose();
                _storage = null;
            }

            if (_clientSession != null)
            {
                _clientSession = null;
            }

            SnowplowTrackerPlatformExtension.Current.StopLocationUpdates();

            // Reset session counters
            SessionMadeCount = 0;
            SessionSuccessCount = 0;
            SessionFailureCount = 0;
        }

        /// <summary>
        /// Returns the current session index
        /// </summary>
        /// <returns>the session index</returns>
        public static int GetClientSessionIndexCount()
        {
            return _clientSession != null ? _clientSession.SessionIndex : -1;
        }

        /// <summary>
        /// Returns the current database event count
        /// </summary>
        /// <returns>the current count of events</returns>
        public static int GetDatabaseEventCount()
        {
            return _storage != null ? _storage.TotalItems : -1;
        }

        // --- Callbacks

        /// <summary>
        /// Called after each batch of events has finished processing
        /// </summary>
        /// <param name="successCount">The success count</param>
        /// <param name="failureCount">The failure count</param>
        public static void EventSuccessCallback(int successCount, int failureCount)
        {
            SessionSuccessCount += successCount;
            SessionFailureCount += failureCount;
        }

        // --- Tracking Functions

        /// <summary>
        /// Tracks an example SelfDescribing event
        /// </summary>
        public static void TrackSelfDescribing()
        {
            SelfDescribingJson sdj = new SelfDescribingJson("iglu:com.snowplowanalytics.snowplow/timing/jsonschema/1-0-0", new Dictionary<string, object> {
                { "category", "SdjTimingCategory" },
                { "variable", "SdjTimingVariable" },
                { "timing", 0 },
                { "label", "SdjTimingLabel" }
            });
            Instance.Track(new SelfDescribing()
                .SetEventData(sdj)
                .Build());
            SessionMadeCount++;
        }

        /// <summary>
        /// Tracks an example page view event
        /// </summary>
        public static void TrackPageView()
        {
            Instance.Track(new PageView()
                .SetPageUrl("http://example.page.com")
                .SetReferrer("http://example.referrer.com")
                .SetPageTitle("Example Page")
                .Build());
            SessionMadeCount++;
        }

        /// <summary>
        /// Tracks an example screen view event
        /// </summary>
        public static void TrackScreenView()
        {
            Instance.Track(new ScreenView()
                .SetId("example-screen-id")
                .SetName("Example Screen")
                .Build());
            SessionMadeCount++;
        }

        /// <summary>
        /// Tracks an example user timing event
        /// </summary>
        public static void TrackTiming()
        {
            Instance.Track(new Timing()
                .SetCategory("category")
                .SetVariable("variable")
                .SetTiming(5)
                .SetLabel("label")
                .Build());
            SessionMadeCount++;
        }

        /// <summary>
        /// Tracks an example structured event
        /// </summary>
        public static void TrackStructEvent()
        {
            Instance.Track(new Structured()
                .SetCategory("exampleCategory")
                .SetAction("exampleAction")
                .SetLabel("exampleLabel")
                .SetProperty("exampleProperty")
                .SetValue(17)
                .Build());
            SessionMadeCount++;
        }

        /// <summary>
        /// Tracks an example ecommerce transaction with two items
        /// </summary>
        public static void TrackEcommerceTransaction()
        {
            var item1 = new EcommerceTransactionItem().SetSku("pbz0026").SetPrice(20).SetQuantity(1).Build();
            var item2 = new EcommerceTransactionItem().SetSku("pbz0038").SetPrice(15).SetQuantity(1).SetName("shirt").SetCategory("clothing").Build();
            var items = new List<EcommerceTransactionItem> { item1, item2 };
            Instance.Track(new EcommerceTransaction()
                .SetOrderId("6a8078be")
                .SetTotalValue(35)
                .SetAffiliation("affiliation")
                .SetTaxValue(3)
                .SetShipping(0)
                .SetCity("Phoenix")
                .SetState("Arizona")
                .SetCountry("US")
                .SetCurrency("USD")
                .SetItems(items)
                .Build());
            SessionMadeCount += 3;
        }
    }
}

