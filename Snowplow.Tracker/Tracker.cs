/*
 * Tracker.cs
 * 
 * Copyright (c) 2014-2017 Snowplow Analytics Ltd. All rights reserved.
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
 * Copyright: Copyright (c) 2014-2017 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Endpoints;
using Snowplow.Tracker.Storage;
using Snowplow.Tracker.Queues;
using Snowplow.Tracker.Emitters;
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Contexts;
using Snowplow.Tracker.Models.Events;
using Snowplow.Tracker.Models.Adapters;
using System.Threading.Tasks;

namespace Snowplow.Tracker
{
    public sealed class Tracker
    {
        private static readonly object _createLock = new object();
        private static volatile Tracker _t;

        private readonly object _lock = new object();
        private volatile bool _running = false;
        private bool _synchronous = true;

        public delegate DesktopContext DesktopContextDelegate();
        public delegate MobileContext MobileContextDelegate();
        public delegate GeoLocationContext GeoLocationContextDelegate();

        private Subject _subject;
        private IEmitter _emitter;
        private bool _encodeBase64;
        private IDisposable _storage;
        private ClientSession _clientSession;
        private Dictionary<string, string> _standardNvPairs;
        private DesktopContextDelegate _desktopContextDelegate;
        private MobileContextDelegate _mobileContextDelegate;
        private GeoLocationContextDelegate _geoLocationDelegate;
        private ILogger _logger;

        public bool IsBackground { get; private set; } = false;

        private Tracker() { }

        // --- Static Instance

        /// <summary>
        /// The tracker instance
        /// </summary>
        public static Tracker Instance
        {
            get
            {
                lock (_createLock)
                {
                    if (_t == null)
                    {
                        _t = new Tracker();
                    }
                    return _t;
                }
            }
        }

        // -- Controls

        /// <summary>
        /// If the tracker is started (open for events)
        /// </summary>
        public bool Started
        {
            get
            {
                lock (_lock)
                {
                    return _running;
                }
            }
        }

        /// <summary>
        /// Start a tracker with a default emitter 
        /// </summary>
        /// <param name="endpoint">Hostname of your collector</param>
        /// <param name="dbPath">A filename/path to store queued events in</param>
        /// <param name="method">The method used to send events to a collector. GET or POST</param>
        /// <param name="subject">Information on the user</param>
        /// <param name="clientSession"></param>
        /// <param name="trackerNamespace">Namespace of tracker</param>
        /// <param name="appId">Application ID of tracker</param>
        /// <param name="encodeBase64">Base64 encode collector parameters</param>
        /// <param name="synchronous">Whether to do I/O synchronously</param>
        /// <param name="desktopContextDelegate">Delegate for fetching the desktop context</param>
        /// <param name="mobileContextDelegate">Delegate for fetching the mobile context</param>
        /// <param name="geoLocationContextDelegate">Delegate for fetching the geo-location context</param>
        /// <param name="l">A logger to emit an activity stream to</param>
        public void Start(string endpoint, string dbPath, HttpMethod method = HttpMethod.POST, Subject subject = null, ClientSession clientSession = null, 
            string trackerNamespace = null, string appId = null, bool encodeBase64 = true, bool synchronous = true, DesktopContextDelegate desktopContextDelegate = null,
            MobileContextDelegate mobileContextDelegate = null, GeoLocationContextDelegate geoLocationContextDelegate = null, ILogger l = null, int? endpointPort = null)
        {
            AsyncEmitter emitter;
            lock (_lock)
            {
                var dest = new SnowplowHttpCollectorEndpoint(endpoint, method: method, l: l, port: endpointPort);
                var storage = new LiteDBStorage(dbPath);
                _storage = storage;
                var queue = new PersistentBlockingQueue(storage, new PayloadToJsonString());
                emitter = new AsyncEmitter(dest, queue, l: l);
            }
            Start(emitter, subject, clientSession, trackerNamespace, appId, synchronous, encodeBase64, desktopContextDelegate, mobileContextDelegate, geoLocationContextDelegate, l);
        }

        /// <summary>
        /// Start a tracker with a custom emitter
        /// </summary>
        /// <param name="emitter">The emitter to send events to</param>
        /// <param name="subject">Information on the user</param>
        /// <param name="clientSession">Client sessionization object</param>
        /// <param name="trackerNamespace">Namespace of tracker</param>
        /// <param name="appId">Application ID of tracker</param>
        /// <param name="encodeBase64">Base64 encode collector parameters</param>
        /// <param name="synchronous">Whether to do I/O synchronously</param>
        /// <param name="desktopContextDelegate">Delegate for fetching the desktop context</param>
        /// <param name="mobileContextDelegate">Delegate for fetching the mobile context</param>
        /// <param name="geoLocationContextDelegate">Delegate for fetching the geo-location context</param>
        /// <param name="l">A logger to emit an activity stream to</param>
        public void Start(IEmitter emitter, Subject subject = null, ClientSession clientSession = null, string trackerNamespace = null, string appId = null, 
            bool encodeBase64 = true, bool synchronous = true, DesktopContextDelegate desktopContextDelegate = null, MobileContextDelegate mobileContextDelegate = null, 
            GeoLocationContextDelegate geoLocationContextDelegate = null, ILogger l = null)
        {
            lock (_lock)
            {
                if (_running)
                {
                    throw new InvalidOperationException("Cannot start - already started");
                }

                _emitter = emitter;
                _emitter.Start();

                if (clientSession != null)
                {
                    _clientSession = clientSession;
                    _clientSession.StartChecker();
                }

                _subject = subject ?? new Subject();
                _encodeBase64 = encodeBase64;
                _logger = l ?? new NoLogging();

                _standardNvPairs = new Dictionary<string, string>
                {
                    { Constants.TRACKER_VERSION, Version.VERSION },
                    { Constants.NAMESPACE, trackerNamespace },
                    { Constants.APP_ID, appId }
                };

                _synchronous = synchronous;
                _desktopContextDelegate = desktopContextDelegate;
                _mobileContextDelegate = mobileContextDelegate;
                _geoLocationDelegate = geoLocationContextDelegate;
                _running = true;
                _logger.Info("Tracker started");
            }
        }

        /// <summary>
        /// Stop the tracker processing new events
        /// </summary>
        public void Stop()
        {
            lock (_createLock)
            {
                lock (_lock)
                {
                    if (_running)
                    {
                        _t = null;

                        _emitter.Close();
                        _emitter = null;

                        if (_storage != null)
                        {
                            _storage.Dispose();
                            _storage = null;
                        }

                        if (_clientSession != null)
                        {
                            _clientSession.Dispose();
                            _clientSession = null;
                        }

                        _running = false;
                        _logger.Info("Tracker stopped");
                        _logger = null;
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that the Tracker has been started
        /// to prevent improper use.
        /// </summary>
        private void ensureTrackerStarted()
        {
            if (!_running)
            {
                throw new NotSupportedException("Cannot track - tracker is not started. Please use Tracker.Start prior to use.");
            }
        }

        /// <summary>
        /// Manually flush all emitters to which the tracker sends events
        /// </summary>
        /// <param name="sync">Whether the flush should be synchronous</param>
        /// <returns>this</returns>
        public Tracker Flush()
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _emitter.Flush();
            }
            return this;
        }

        // --- Thread-safe ClientSession setter methods

        public void SetBackground(bool isBackground)
        {
            lock (_lock)
            {
                ensureTrackerStarted();

                if (_clientSession != null)
                {
                    _clientSession.SetBackground(isBackground);
                }

                IsBackground = isBackground;
            }
        }

        // --- Thread-safe Subject setter methods

        public Tracker SetPlatform(Platform value)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetPlatform(value);
            }
            return this;
        }

        public Tracker SetUserId(string id)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetUserId(id);
            }
            return this;
        }

        public Tracker SetScreenResolution(int width, int height)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetScreenResolution(width, height);
            }
            return this;
        }

        public Tracker SetViewport(int width, int height)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetViewport(width, height);
            }
            return this;
        }

        public Tracker SetColorDepth(int depth)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetColorDepth(depth);
            }
            return this;
        }

        public Tracker SetTimezone(string timezone)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetTimezone(timezone);
            }
            return this;
        }

        public Tracker SetLang(string lang)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetLang(lang);
            }
            return this;
        }

        public Tracker SetIpAddress(string ipAddress)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetIpAddress(ipAddress);
            }
            return this;
        }

        public Tracker SetUseragent(string useragent)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetUseragent(useragent);
            }
            return this;
        }

        public Tracker SetDomainUserId(string domainUserId)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetDomainUserId(domainUserId);
            }
            return this;
        }

        public Tracker SetNetworkUserId(string networkUserId)
        {
            lock (_lock)
            {
                ensureTrackerStarted();
                _subject.SetNetworkUserId(networkUserId);
            }
            return this;
        }

        // --- Event Tracking

        /// <summary>
        /// Tracks an event; will throw an exception if the 
        /// Track has not yet been started
        /// </summary>
        /// <param name="newEvent">The event which will be tracked</param>
        /// <param name="eventSubject">An optional Subject for this specific event</param>
        public void Track(IEvent newEvent, Subject eventSubject = null)
        {
            ensureTrackerStarted();
            ProcessEvent(newEvent, eventSubject);
        }

        /// <summary>
        /// Figures out what kind of event is being tracked and
        /// then processes it accordingly.
        /// </summary>
        /// <param name="newEvent">The event which will be tracked</param>
        /// <param name="eventSubject">An optional Subject for this specific event</param>
        private void ProcessEvent(IEvent newEvent, Subject eventSubject)
        {
            List<IContext> contexts = newEvent.GetContexts();
            string eventId = newEvent.GetEventId();
            Type eType = newEvent.GetType();

            if (eType == typeof(PageView) || eType == typeof(Structured))
            {
                CompleteAndTrackPayload((Payload)newEvent.GetPayload(), contexts, eventId, eventSubject);
            }
            else if (eType == typeof(EcommerceTransaction))
            {
                CompleteAndTrackPayload((Payload)newEvent.GetPayload(), contexts, eventId, eventSubject);
                EcommerceTransaction ecommerceTransaction = (EcommerceTransaction)newEvent;
                foreach (EcommerceTransactionItem item in ecommerceTransaction.GetItems())
                {
                    item.SetItemId(ecommerceTransaction.GetOrderId());
                    item.SetCurrency(ecommerceTransaction.GetCurrency());
                    item.SetDeviceCreatedTimestamp(ecommerceTransaction.GetDeviceCreatedTimestamp());
                    item.SetTrueTimestamp(ecommerceTransaction.GetTrueTimestamp());
                    CompleteAndTrackPayload((Payload)item.GetPayload(), item.GetContexts(), item.GetEventId(), eventSubject);
                }
            }
            else if (eType == typeof(SelfDescribing))
            {
                SelfDescribing selfDescribing = (SelfDescribing)newEvent;
                selfDescribing.SetBase64Encode(_encodeBase64);
                CompleteAndTrackPayload((Payload)selfDescribing.GetPayload(), contexts, eventId, eventSubject);
            }
            else if (eType == typeof(ScreenView) || eType == typeof(Timing))
            {
                ProcessEvent(new SelfDescribing()
                           .SetEventData((SelfDescribingJson)newEvent.GetPayload())
                           .SetCustomContext(newEvent.GetContexts())
                           .SetDeviceCreatedTimestamp(newEvent.GetDeviceCreatedTimestamp())
                           .SetTrueTimestamp(newEvent.GetTrueTimestamp())
                           .SetEventId(newEvent.GetEventId())
                           .Build(), eventSubject);
            }
        }

        /// <summary>
        /// Adds the standard NV Pairs to the payload and stitches any available
        /// contexts to the final payload.
        /// </summary>
        /// <param name="payload">The basee event payload</param>
        /// <param name="contexts">The contexts array</param>
        /// <param name="eventId">The event ID</param>
        /// <param name="eventSubject">The Subject for this event</param>
        private void CompleteAndTrackPayload(Payload payload, List<IContext> contexts, string eventId, Subject eventSubject)
        {
            payload.AddDict(_standardNvPairs);

            // Add the subject data if available
            if (eventSubject != null)
            {
                payload.AddDict(eventSubject.Payload.Payload);
            }
            else if (_subject != null)
            {
                payload.AddDict(_subject.Payload.Payload);
            }

            // Add the session context if available
            if (_clientSession != null)
            {
                contexts.Add(_clientSession.GetSessionContext(eventId));
            }

            // Add the desktop context if available
            if (_desktopContextDelegate != null)
            {
                DesktopContext desktopContext = _desktopContextDelegate.Invoke();
                if (desktopContext != null)
                {
                    contexts.Add(desktopContext);
                }
            }

            // Add the mobile context if available
            if (_mobileContextDelegate != null)
            {
                MobileContext mobileContext = _mobileContextDelegate.Invoke();
                if (mobileContext != null)
                {
                    contexts.Add(mobileContext);
                }
            }

            // Add the geo-location context if available
            if (_geoLocationDelegate != null)
            {
                GeoLocationContext geoLocationContext = _geoLocationDelegate.Invoke();
                if (geoLocationContext != null)
                {
                    contexts.Add(geoLocationContext);
                }
            }

            // Build the final context and it to the payload
            if (contexts != null && contexts.Any())
            {
                var contextArray = new List<Dictionary<string, object>>();
                foreach (IContext context in contexts)
                {
                    contextArray.Add(context.GetJson().Payload);
                }
                var contextEnvelope = new SelfDescribingJson(Constants.SCHEMA_CONTEXTS, contextArray);
                payload.AddJson(contextEnvelope.Payload, _encodeBase64, Constants.CONTEXT_ENCODED, Constants.CONTEXT);
            }

            // Send the payload to the emitter
            if (_synchronous) {
                _emitter.Input(payload);
            }
            else {
                Task.Factory.StartNew(() => _emitter.Input(payload));
            }
        }

        // --- Setters

        /// <summary>
        /// Set the subject of the events fired by the tracker
        /// </summary>
        /// <param name="subject">Subject to track</param>
        /// <returns>this</returns>
        public Tracker SetSubject(Subject subject)
        {
            lock (_lock)
            {
                _subject = subject;
            }
            return this;
        }

        /// <summary>
        /// Set the appid of the events fired by the tracker
        /// </summary>
        /// <param name="appid">AppId to track</param>
        /// <returns>this</returns>
        public Tracker SetAppId(string appId)
        {
            lock (_lock)
            {
                _standardNvPairs[Constants.APP_ID] = appId;
            }
            return this;
        }

    }
}
