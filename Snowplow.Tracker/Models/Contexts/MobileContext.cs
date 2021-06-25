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

namespace Snowplow.Tracker.Models.Contexts
{
    public class MobileContext : AbstractContext<MobileContext>
    {

        /// <summary>
        /// Sets the type of the os.
        /// </summary>
        /// <returns>The os type.</returns>
        /// <param name="osType">Os type.</param>
        public MobileContext SetOsType(string osType) {
            this.DoAdd (Constants.PLAT_OS_TYPE, osType);
            return this;
        }

        /// <summary>
        /// Sets the os version.
        /// </summary>
        /// <returns>The os version.</returns>
        /// <param name="osVersion">Os version.</param>
        public MobileContext SetOsVersion(string osVersion) {
            this.DoAdd (Constants.PLAT_OS_VERSION, osVersion);
            return this;
        }

        /// <summary>
        /// Sets the device manufacturer.
        /// </summary>
        /// <returns>The device manufacturer.</returns>
        /// <param name="deviceManufacturer">Device manufacturer.</param>
        public MobileContext SetDeviceManufacturer(string deviceManufacturer) {
            this.DoAdd (Constants.PLAT_DEVICE_MANU, deviceManufacturer);
            return this;
        }

        /// <summary>
        /// Sets the device model.
        /// </summary>
        /// <returns>The device model.</returns>
        /// <param name="deviceModel">Device model.</param>
        public MobileContext SetDeviceModel(string deviceModel) {
            this.DoAdd (Constants.PLAT_DEVICE_MODEL, deviceModel);
            return this;
        }

        /// <summary>
        /// Sets the carrier.
        /// </summary>
        /// <returns>The carrier.</returns>
        /// <param name="carrier">Carrier.</param>
        public MobileContext SetCarrier(string carrier) {
            this.DoAdd (Constants.MOBILE_CARRIER, carrier);
            return this;
        }

        /// <summary>
        /// Sets the type of the network.
        /// </summary>
        /// <returns>The network type.</returns>
        /// <param name="networkType">Network type.</param>
        public MobileContext SetNetworkType(NetworkType networkType) {
            if (networkType != null)
            {
                this.DoAdd (Constants.MOBILE_NET_TYPE, networkType.Value);
            }
            return this;
        }

        /// <summary>
        /// Sets the network technology.
        /// </summary>
        /// <returns>The network technology.</returns>
        /// <param name="networkTechnology">Network technology.</param>
        public MobileContext SetNetworkTechnology(string networkTechnology) {
            this.DoAdd (Constants.MOBILE_NET_TECH, networkTechnology);
            return this;
        }

        /// <summary>
        /// Sets the open idfa.
        /// </summary>
        /// <returns>The open idfa.</returns>
        /// <param name="openIdfa">Open idfa.</param>
        public MobileContext SetOpenIdfa(string openIdfa) {
            this.DoAdd (Constants.MOBILE_OPEN_IDFA, openIdfa);
            return this;
        }

        /// <summary>
        /// Sets the apple idfa.
        /// </summary>
        /// <returns>The apple idfa.</returns>
        /// <param name="appleIdfa">Apple idfa.</param>
        public MobileContext SetAppleIdfa(string appleIdfa) {
            this.DoAdd (Constants.MOBILE_APPLE_IDFA, appleIdfa);
            return this;
        }

        /// <summary>
        /// Sets the apple idfv.
        /// </summary>
        /// <returns>The apple idfv.</returns>
        /// <param name="appleIdfv">Apple idfv.</param>
        public MobileContext SetAppleIdfv(string appleIdfv) {
            this.DoAdd (Constants.MOBILE_APPLE_IDFV, appleIdfv);
            return this;
        }

        /// <summary>
        /// Sets the android idfa.
        /// </summary>
        /// <returns>The android idfa.</returns>
        /// <param name="androidIdfa">Android idfa.</param>
        public MobileContext SetAndroidIdfa(string androidIdfa) {
            this.DoAdd (Constants.MOBILE_ANDROID_IDFA, androidIdfa);
            return this;
        }
        
        public override MobileContext Build() {
            Utils.CheckArgument (this.data.ContainsKey(Constants.PLAT_OS_TYPE), "Mobilec ontext requires 'osType'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.PLAT_OS_VERSION), "Mobile context requires 'osVersion'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.PLAT_DEVICE_MANU), "Mobile context requires 'deviceManufacturer'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.PLAT_DEVICE_MODEL), "Mobile context requires 'deviceModel'.");
            this.schema = Constants.SCHEMA_MOBILE;
            this.context = new SelfDescribingJson (this.schema, this.data);
            return this;
        }
    }
}
