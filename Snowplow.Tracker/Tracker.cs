using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SelfDescribingJson = System.Collections.Generic.Dictionary<string, object>;
using Context = System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>;

namespace Snowplow.Tracker
{
    public class Tracker
    {
        private string collectorUri;
        private bool b64 = false; // TODO: make this configurable
        private Dictionary<string, string> standardNvPairs;

        public Tracker(string endpoint, string trackerNamespace = null, string appId = null, bool encodeBase64 = true)
        {
            collectorUri = getCollectorUri(endpoint);
            b64 = encodeBase64;
            standardNvPairs = new Dictionary<string, string>
            {
                { "tv", Version.VERSION },
                { "tna", trackerNamespace },
                { "aid", appId },
                { "p", "pc" }
            };
        }

        private string getCollectorUri(string endpoint)
        {
            return "http://" + endpoint + "/i";
        }

        private Tracker setPlatform(string value)
        {
            standardNvPairs["p"] = value;
            return this;
        }

        private Tracker setUserId(string id)
        {
            standardNvPairs["uid"] = id;
            return this;
        }

        private Tracker setScreenResolution(int width, int height)
        {
            standardNvPairs["res"] = width.ToString() + "x" + height.ToString();
            return this;
        }

        private Tracker setViewport(int width, int height)
        {
            standardNvPairs["vp"] = width.ToString() + "x" + height.ToString();
            return this;
        }

        private Tracker setColorDepth(int depth)
        {
            standardNvPairs["cd"] = depth.ToString();
            return this;
        }

        private Tracker setTimezone(string timezone)
        {
            standardNvPairs["tz"] = timezone;
            return this;
        }

        private Tracker setLang(string lang)
        {
            standardNvPairs["lang"] = lang;
            return this;
        }

        // TODO: allow a configurable timestamp
        private static Int64 getTimestamp(Int64 tstamp)
        {
            if (tstamp == null)
            {
                return (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            }
            else
            {
                return tstamp;
            }
        }

        private void httpGet(Dictionary<string, string> payload)
        {

        }

        private void track(Payload pb)
        {
            httpGet(pb.NvPairs);
        }

        private void completePayload(Payload pb, Context context)
        {
            pb.add("uid", Guid.NewGuid().ToString());
            if (context != null && context.Any())
            {
                var contextEnvelope = new Dictionary<string, object>
                {
                    { "schema", "iglu:com.snowplowanalytics.snowplow/contexts/1-0-0" },
                    { "data", context }
                };
                pb.addJson(contextEnvelope, b64, "cx", "co");
            }
            pb.addDict(standardNvPairs);

            // TODO: remove debug code
            Console.WriteLine("about to display keys");
            foreach (string key in pb.NvPairs.Keys)
            {
                Console.WriteLine(key + ": " + pb.NvPairs[key]);
            }
            Console.WriteLine("finished displaying keys");

            track(pb);
        }

        public Tracker trackPageView(string pageUrl, string page_title = null, string referrer = null, Context context = null)
        {
            Payload pb = new Payload();
            pb.add("e", "pv");
            pb.add("url", pageUrl);
            pb.add("page", page_title);
            pb.add("refr", referrer);
            completePayload(pb, context);
            return this;
        }

        public Tracker trackStructEvent(string category, string action = null, string label = null, string property = null, double? value = null, Context context = null)
        {
            Payload pb = new Payload();
            pb.add("e", "se");
            pb.add("se_ca", category);
            pb.add("se_ac", action);
            pb.add("se_la", label);
            pb.add("se_pr", property);
            pb.add("se_va", value);
            completePayload(pb, context);
            return this;
        }

        public Tracker trackUnstructEvent(SelfDescribingJson eventJson, Context context = null)
        {
            var envelope = new Dictionary<string, object>
            {
                { "schema", "iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0" },
                { "data", eventJson }
            };
            Payload pb = new Payload();
            pb.add("e", "ue");
            pb.addJson(envelope, b64, "ue_pr", "ue_px");
            completePayload(pb, context);
            return this;
        }
        public Tracker trackScreenView(string name = null, string id = null, Context context = null)
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
                { "schema", "iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0" },
                { "data", screenViewProperties }
            };
            Payload pb = new Payload();
            pb.add("e", "ue");
            pb.addJson(envelope, b64, "ue_pr", "ue_px");
            completePayload(pb, context);
            return this;
        }
    }
}
