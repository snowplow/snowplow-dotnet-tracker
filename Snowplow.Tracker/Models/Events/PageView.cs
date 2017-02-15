/*
 * PageView.cs
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

namespace Snowplow.Tracker.Models.Events
{
    public class PageView : AbstractEvent<PageView>
    {

        private string pageUrl;
        private string pageTitle;
        private string referrer;

        /// <summary>
        /// Sets the page URL.
        /// </summary>
        /// <returns>The page URL.</returns>
        /// <param name="pageUrl">Page URL.</param>
        public PageView SetPageUrl(string pageUrl) {
            this.pageUrl = pageUrl;
            return this;
        }

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <returns>The page title.</returns>
        /// <param name="pageTitle">Page title.</param>
        public PageView SetPageTitle(string pageTitle) {
            this.pageTitle = pageTitle;
            return this;
        }

        /// <summary>
        /// Sets the referrer.
        /// </summary>
        /// <returns>The referrer.</returns>
        /// <param name="referrer">Referrer.</param>
        public PageView SetReferrer(string referrer) {
            this.referrer = referrer;
            return this;
        }

        public override PageView Self() {
            return this;
        }

        public override PageView Build() {
            Utils.CheckArgument (!string.IsNullOrEmpty(pageUrl), "PageUrl cannot be null or empty.");
            return this;
        }

        // --- Interface Methods

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        /// <returns>The event payload</returns>
        public override IPayload GetPayload() {
            Payload payload = new Payload();
            payload.Add (Constants.EVENT, Constants.EVENT_PAGE_VIEW);
            payload.Add (Constants.PAGE_URL, pageUrl);
            payload.Add (Constants.PAGE_TITLE, pageTitle);
            payload.Add (Constants.PAGE_REFR, referrer);
            return AddDefaultPairs (payload);
        }
    }
}
