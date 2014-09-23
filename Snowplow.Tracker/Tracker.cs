/*
 * Tracker.cs
 * 
 * Copyright (c) 2014 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Fred Blundun
 * Copyright: Copyright (c) 2014 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using SelfDescribingJson = System.Collections.Generic.Dictionary<string, object>;
using Context = System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>;

namespace Snowplow.Tracker
{
    public class Tracker
    {
        private Subject subject;
        private List<IEmitter> emitters;
        private bool encodeBase64;
        private Dictionary<string, string> standardNvPairs;

        public Tracker(List<IEmitter> emitters, Subject subject = null, string trackerNamespace = null, string appId = null, bool encodeBase64 = true)
        {
            this.emitters = emitters;
            this.subject = subject ?? new Subject();
            this.encodeBase64 = encodeBase64;
            standardNvPairs = new Dictionary<string, string>
            {
                { "tv", Version.VERSION },
                { "tna", trackerNamespace },
                { "aid", appId }
            };
        }

        // Overload method initializing a tracker with a single IEmitter rather than a list
        public Tracker(IEmitter endpoint, Subject subject = null, string trackerNamespace = null, string appId = null, bool encodeBase64 = true)
            : this(new List<IEmitter> { endpoint }, subject, trackerNamespace, appId, encodeBase64) { }

        public Tracker SetPlatform(Platform value)
        {
            subject.SetPlatform(value);
            return this;
        }

        public Tracker SetUserId(string id)
        {
            subject.SetUserId(id);
            return this;
        }

        public Tracker SetScreenResolution(int width, int height)
        {
            subject.SetScreenResolution(width, height);
            return this;
        }

        public Tracker SetViewport(int width, int height)
        {
            subject.SetViewport(width, height);
            return this;
        }

        public Tracker SetColorDepth(int depth)
        {
            subject.SetColorDepth(depth);
            return this;
        }

        public Tracker SetTimezone(string timezone)
        {
            subject.SetTimezone(timezone);
            return this;
        }

        public Tracker SetLang(string lang)
        {
            subject.SetLang(lang);
            return this;
        }

        private static Int64 GetTimestamp(Int64? tstamp)
        {
            return tstamp ?? (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }

        private void Track(Payload pb)
        {
            foreach (IEmitter emitter in emitters)
            {
                emitter.Input(pb.NvPairs);
            }
        }

        private void CompletePayload(Payload pb, Context context, Int64? tstamp)
        {
            pb.Add("dtm", GetTimestamp(tstamp));
            pb.Add("eid", Guid.NewGuid().ToString());
            if (context != null && context.Any())
            {
                var contextEnvelope = new Dictionary<string, object>
                {
                    { "schema", "iglu:com.snowplowanalytics.snowplow/contexts/1-0-0" },
                    { "data", context }
                };
                pb.AddJson(contextEnvelope, encodeBase64, "cx", "co");
            }
            pb.AddDict(standardNvPairs);
            pb.AddDict(subject.nvPairs);

            Track(pb);
        }

        public Tracker TrackPageView(string pageUrl, string pageTitle = null, string referrer = null, Context context = null, Int64? tstamp = null)
        {
            Payload pb = new Payload();
            pb.Add("e", "pv");
            pb.Add("url", pageUrl);
            pb.Add("page", pageTitle);
            pb.Add("refr", referrer);
            CompletePayload(pb, context, tstamp);
            return this;
        }

        private void TrackEcommerceTransactionItem(string orderId, string currency, TransactionItem item, Context context, Int64? tstamp)
        {
            Payload pb = new Payload();
            pb.Add("e", "ti");
            pb.Add("ti_id", orderId);
            pb.Add("ti_cu", currency);
            pb.Add("ti_sk", item.sku);
            pb.Add("ti_pr", item.price);
            pb.Add("ti_qu", item.quantity);
            pb.Add("ti_nm", item.name);
            pb.Add("ti_ca", item.category);
            CompletePayload(pb, context, tstamp);
        }

        public Tracker TrackEcommerceTransaction(string orderId, double totalValue, string affiliation = null, double? taxValue = null, double? shipping = null, string city = null, string state = null, string country = null, string currency = null, List<TransactionItem> items = null, Context context = null, Int64? tstamp = null)
        {
            Payload pb = new Payload();
            pb.Add("e", "tr");
            pb.Add("tr_id", orderId);
            pb.Add("tr_tt", totalValue);
            pb.Add("tr_af", affiliation);
            pb.Add("tr_tx", taxValue);
            pb.Add("tr_sh", shipping);
            pb.Add("tr_ci", city);
            pb.Add("tr_st", state);
            pb.Add("tr_co", country);
            pb.Add("tr_cu", currency);
            CompletePayload(pb, context, tstamp);

            if (items != null)
            {
                foreach (TransactionItem item in items)
                {
                    TrackEcommerceTransactionItem(orderId, currency, item, context, tstamp);
                }
            }

            return this;
        }

        public Tracker TrackStructEvent(string category, string action, string label = null, string property = null, double? value = null, Context context = null, Int64? tstamp = null)
        {
            Payload pb = new Payload();
            pb.Add("e", "se");
            pb.Add("se_ca", category);
            pb.Add("se_ac", action);
            pb.Add("se_la", label);
            pb.Add("se_pr", property);
            pb.Add("se_va", value);
            CompletePayload(pb, context, tstamp);
            return this;
        }

        public Tracker TrackUnstructEvent(SelfDescribingJson eventJson, Context context = null, Int64? tstamp = null)
        {
            var envelope = new Dictionary<string, object>
            {
                { "schema", "iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0" },
                { "data", eventJson }
            };
            Payload pb = new Payload();
            pb.Add("e", "ue");
            pb.AddJson(envelope, encodeBase64, "ue_px", "ue_pr");
            CompletePayload(pb, context, tstamp);
            return this;
        }

        public Tracker TrackScreenView(string name = null, string id = null, Context context = null, Int64? tstamp = null)
        {
            var screenViewProperties = new Dictionary<string, string>();
            if (name != null)
            {
                screenViewProperties["name"] = name;
            }
            if (id != null)
            {
                screenViewProperties["id"] = id;
            }
            var envelope = new Dictionary<string, object>
            {
                { "schema", "iglu:com.snowplowanalytics.snowplow/screen_view/jsonschema/1-0-0" },
                { "data", screenViewProperties }
            };
            TrackUnstructEvent(envelope, context, tstamp);
            return this;
        }

        public Tracker Flush(bool sync = false)
        {
            foreach (IEmitter emitter in emitters)
            {
                emitter.Flush(sync);
            }
            return this;
        }

        public Tracker SetSubject(Subject subject)
        {
            this.subject = subject;
            return this;
        }

        public Tracker AddEmitter(Emitter emitter)
        {
            emitters.Add(emitter);
            return this;
        }
    }
}
