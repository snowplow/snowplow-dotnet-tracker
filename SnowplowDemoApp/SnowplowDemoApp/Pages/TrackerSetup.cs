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

using System;
using SnowplowDemoApp.Utils;
using Snowplow.Tracker;
using Snowplow.Tracker.Endpoints;
using Snowplow.Tracker.Models.Contexts;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SnowplowDemoApp.Pages
{
    public class TrackerSetup : ContentPage
    {
        // --- Constants

        private string KEY_EMITTER_URI = "emitterUri";
        private string KEY_EMITTER_PORT = "emitterPort";
        private string KEY_HTTP_METHOD = "httpMethod";
        private string KEY_HTTP_PROTOCOL = "httpProtocol";
        private string KEY_CLIENT_SESSION = "clientSession";
        private string KEY_MOBILE_CONTEXT = "mobileContext";
        private string KEY_GEO_LOCATION_CONTEXT = "geoLocationContext";

        // --- UI Elements

        private EntryCell _emitterUriEntryCell;
        private EntryCell _emitterPortEntryCell;
        private SwitchCell _getOrPostSwitchCell;
        private SwitchCell _httpOrHttpsSwitchCell;

        private SwitchCell _clientSessionSwitchCell;
        private SwitchCell _mobileContextSwitchCell;
        private SwitchCell _geoLocationContextSwitchCell;

        private SwitchCell _startStopTrackerSwitchCell;

        // --- Constructor

        public TrackerSetup()
        {
            Title = "Tracker setup";
            Content = new StackLayout
            {
                Children = {
                    BuildView()
                }
            };
        }

        // --- Components

        /// <summary>
        /// Constructs the Settings page view
        /// </summary>
        /// <returns></returns>
        private StackLayout BuildView()
        {
            // Make elements
            _emitterUriEntryCell = new EntryCell { Label = "Endpoint:", Placeholder = "Enter endpoint here..." };
            _emitterPortEntryCell = new EntryCell { Label = "Port:", Placeholder = "Enter port here...", Keyboard = Keyboard.Numeric };
            _getOrPostSwitchCell = new SwitchCell { };
            _httpOrHttpsSwitchCell = new SwitchCell { };
            _clientSessionSwitchCell = new SwitchCell { };
            _mobileContextSwitchCell = new SwitchCell { };
            _geoLocationContextSwitchCell = new SwitchCell { };
            _startStopTrackerSwitchCell = new SwitchCell { };

            // Get values from key-value store
            _emitterUriEntryCell.Text = PropertyManager.GetStringValue(KEY_EMITTER_URI);
            _emitterPortEntryCell.Text = PropertyManager.GetStringValue(KEY_EMITTER_PORT);
            _getOrPostSwitchCell.On = PropertyManager.GetBoolValue(KEY_HTTP_METHOD);
            _httpOrHttpsSwitchCell.On = PropertyManager.GetBoolValue(KEY_HTTP_PROTOCOL);
            _clientSessionSwitchCell.On = PropertyManager.GetBoolValue(KEY_CLIENT_SESSION);
            _mobileContextSwitchCell.On = PropertyManager.GetBoolValue(KEY_MOBILE_CONTEXT);
            _geoLocationContextSwitchCell.On = PropertyManager.GetBoolValue(KEY_GEO_LOCATION_CONTEXT);

            // Update labels
            OnGetOrPostSwitchChanged(null, null);
            OnHttpOrHttpsSwitchChanged(null, null);
            OnClientSessionSwitchChanged(null, null);
            OnMobileContextSwitchChanged(null, null);
            OnGeoLocationContextSwitchChanged(null, null);

            _startStopTrackerSwitchCell.On = Tracker.Instance.Started;
            UpdateTrackerStartStopSettings();

            // --- Change Listeners

            _getOrPostSwitchCell.OnChanged += OnGetOrPostSwitchChanged;
            _httpOrHttpsSwitchCell.OnChanged += OnHttpOrHttpsSwitchChanged;
            _clientSessionSwitchCell.OnChanged += OnClientSessionSwitchChanged;
            _mobileContextSwitchCell.OnChanged += OnMobileContextSwitchChanged;
            _geoLocationContextSwitchCell.OnChanged += OnGeoLocationContextSwitchChanged;
            _startStopTrackerSwitchCell.OnChanged += OnTrackerStartStopSwitchChanged;

            // --- Assemblew View

            var settingsTable = new TableView
            {
                Intent = TableIntent.Settings,
                Root = new TableRoot
                {
                    new TableSection("Controls")
                    {
                        _startStopTrackerSwitchCell
                    },
                    new TableSection("Contexts")
                    {
                        _clientSessionSwitchCell,
                        _mobileContextSwitchCell,
                        _geoLocationContextSwitchCell
                    },
                    new TableSection("Emitter")
                    {
                        _emitterUriEntryCell,
                        _emitterPortEntryCell,
                        _getOrPostSwitchCell,
                        _httpOrHttpsSwitchCell
                    }
                }
            };

            return new StackLayout
            {
                Children = { settingsTable },
                VerticalOptions = LayoutOptions.FillAndExpand
            };
        }

        // --- Interaction

        /// <summary>
        /// Updates the method label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGetOrPostSwitchChanged(object sender, EventArgs e)
        {
            _getOrPostSwitchCell.Text = "Method: " + (_getOrPostSwitchCell.On ? "POST" : "GET");
        }

        /// <summary>
        /// Updates the protocol label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHttpOrHttpsSwitchChanged(object sender, EventArgs e)
        {
            _httpOrHttpsSwitchCell.Text = "Protocol: " + (_httpOrHttpsSwitchCell.On ? "HTTPS" : "HTTP");
        }

        /// <summary>
        /// Updates the client session label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClientSessionSwitchChanged(object sender, EventArgs e)
        {
            _clientSessionSwitchCell.Text = "Client session: " + (_clientSessionSwitchCell.On ? "ON" : "OFF");
        }

        /// <summary>
        /// Updates the mobile context label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMobileContextSwitchChanged(object sender, EventArgs e)
        {
            _mobileContextSwitchCell.Text = "Mobile context: " + (_mobileContextSwitchCell.On ? "ON" : "OFF");
        }

        /// <summary>
        /// Updates the geo location context label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGeoLocationContextSwitchChanged(object sender, EventArgs e)
        {
            _geoLocationContextSwitchCell.Text = "Geo-Location context: " + (_geoLocationContextSwitchCell.On ? "ON" : "OFF");
        }

        /// <summary>
        /// Parses the settings and starts the Tracker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTrackerStartStopSwitchChanged(object sender, EventArgs e)
        {
            if (_startStopTrackerSwitchCell.On)
            {
                if (Tracker.Instance.Started)
                {
                    await DisplayAlert("Info", "Tracker is already running, to update settings please stop and re-start", "OK");
                    return;
                }

                try
                {
                    string emitterUri = _emitterUriEntryCell.Text;
                    if (String.IsNullOrEmpty(emitterUri))
                    {
                        throw new ArgumentException("The endpoint cannot be empty");
                    }

                    int? emitterPort = null;
                    try
                    {
                        emitterPort = Int32.Parse(_emitterPortEntryCell.Text);
                    }
                    catch (Exception fe)
                    {
                        Console.WriteLine(fe);
                    }

                    HttpMethod httpMethod = _getOrPostSwitchCell.On ? HttpMethod.POST : HttpMethod.GET;
                    HttpProtocol httpProtocol = _httpOrHttpsSwitchCell.On ? HttpProtocol.HTTPS : HttpProtocol.HTTP;

                    Analytics.Snowplow.Init(emitterUri, httpProtocol, emitterPort, httpMethod,
                        _clientSessionSwitchCell.On, _mobileContextSwitchCell.On, _geoLocationContextSwitchCell.On);

                    PropertyManager.SaveKeyValue(KEY_EMITTER_URI, emitterUri);
                    PropertyManager.SaveKeyValue(KEY_EMITTER_PORT, _emitterPortEntryCell.Text);
                    PropertyManager.SaveKeyValue(KEY_HTTP_METHOD, _getOrPostSwitchCell.On);
                    PropertyManager.SaveKeyValue(KEY_HTTP_PROTOCOL, _httpOrHttpsSwitchCell.On);
                    PropertyManager.SaveKeyValue(KEY_CLIENT_SESSION, _clientSessionSwitchCell.On);
                    PropertyManager.SaveKeyValue(KEY_MOBILE_CONTEXT, _mobileContextSwitchCell.On);
                    PropertyManager.SaveKeyValue(KEY_GEO_LOCATION_CONTEXT, _geoLocationContextSwitchCell.On);

                    await DisplayAlert("Success", "Tracker started successfully - time to send some events!", "OK");

                    if (_mobileContextSwitchCell.On || _geoLocationContextSwitchCell.On)
                    {
                        await CheckAndRequestLocationPermission();
                    }
                }
                catch (Exception ae)
                {
                    await DisplayAlert("Invalid arguments", ae.Message, "OK");
                }
            }
            else
            {
                Analytics.Snowplow.Shutdown();
            }

            UpdateTrackerStartStopSettings();
        }

        /// <summary>
        /// Updates the tracker start stop switch
        /// </summary>
        private void UpdateTrackerStartStopSettings()
        {
            var started = Tracker.Instance.Started;
            _startStopTrackerSwitchCell.Text = "Tracker running: " + started;
            _startStopTrackerSwitchCell.On = started;

            // Disable/Enable based on started status
            _emitterUriEntryCell.IsEnabled = !started;
            _emitterPortEntryCell.IsEnabled = !started;
            _getOrPostSwitchCell.IsEnabled = !started;
            _httpOrHttpsSwitchCell.IsEnabled = !started;
            _clientSessionSwitchCell.IsEnabled = !started;
            _mobileContextSwitchCell.IsEnabled = !started;
            _geoLocationContextSwitchCell.IsEnabled = !started;
        }

        // --- Lifecycle

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            GC.Collect();
        }

        // --- Permissions

        public async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return status;
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            return status;
        }
    }
}

