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
    public class DesktopContext : AbstractContext<DesktopContext>
    {

        /// <summary>
        /// Sets the type of the OS.
        /// </summary>
        /// <returns>The os type.</returns>
        /// <param name="osType">Os type.</param>
        public DesktopContext SetOsType(string osType) {
            this.DoAdd (Constants.PLAT_OS_TYPE, osType);
            return this;
        }

        /// <summary>
        /// Sets the OS version.
        /// </summary>
        /// <returns>The os version.</returns>
        /// <param name="osVersion">Os version.</param>
        public DesktopContext SetOsVersion(string osVersion) {
            this.DoAdd (Constants.PLAT_OS_VERSION, osVersion);
            return this;
        }

        /// <summary>
        /// Sets the OS service pack.
        /// </summary>
        /// <returns>The os service pack.</returns>
        /// <param name="osServicePack">Os service pack.</param>
        public DesktopContext SetOsServicePack(string osServicePack) {
            this.DoAdd (Constants.DESKTOP_SERVICE_PACK, osServicePack);
            return this;
        }

        /// <summary>
        /// Sets if the OS is 64 bit.
        /// </summary>
        /// <returns>The os is64 bit.</returns>
        /// <param name="osIs64Bit">If set to <c>true</c> os is64 bit.</param>
        public DesktopContext SetOsIs64Bit(bool osIs64Bit) {
            this.DoAdd (Constants.DESKTOP_IS_64_BIT, osIs64Bit);
            return this;
        }

        /// <summary>
        /// Sets the device manufacturer.
        /// </summary>
        /// <returns>The device manufacturer.</returns>
        /// <param name="deviceManufacturer">Device manufacturer.</param>
        public DesktopContext SetDeviceManufacturer(string deviceManufacturer) {
            this.DoAdd (Constants.PLAT_DEVICE_MANU, deviceManufacturer);
            return this;
        }

        /// <summary>
        /// Sets the device model.
        /// </summary>
        /// <returns>The device model.</returns>
        /// <param name="deviceModel">Device model.</param>
        public DesktopContext SetDeviceModel(string deviceModel) {
            this.DoAdd (Constants.PLAT_DEVICE_MODEL, deviceModel);
            return this;
        }

        /// <summary>
        /// Sets the device processor count.
        /// </summary>
        /// <returns>The device processor count.</returns>
        /// <param name="processorCount">Processor count.</param>
        public DesktopContext SetDeviceProcessorCount(int processorCount) {
            this.DoAdd (Constants.DESKTOP_PROC_COUNT, processorCount);
            return this;
        }

        public override DesktopContext Build() {
            Utils.CheckArgument (this.data.ContainsKey(Constants.PLAT_OS_TYPE), "Desktop Context requires 'osType'.");
            Utils.CheckArgument (this.data.ContainsKey(Constants.PLAT_OS_VERSION), "Desktop Context requires 'osVersion'.");
            this.schema = Constants.SCHEMA_DESKTOP;
            this.context = new SelfDescribingJson (this.schema, this.data);
            return this;
        }
    }
}
