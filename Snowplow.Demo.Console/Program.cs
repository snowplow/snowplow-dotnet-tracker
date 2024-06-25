using Snowplow.Tracker;
using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Events;

string collectorHostname = "http://localhost:9090";
int port = 80;

// help the user out a bit with ports
if (Uri.IsWellFormedUriString(collectorHostname, UriKind.Absolute))
{
    Uri tmp;
    if (Uri.TryCreate(collectorHostname, UriKind.Absolute, out tmp))
    {
        if (tmp.Scheme == "http" || tmp.Scheme == "https")
        {
            collectorHostname = tmp.Host;
            port = tmp.Port;
        }
    }
}

Console.WriteLine("Demo app started - sending events to " + collectorHostname + " port " + port);

var logger = new ConsoleLogger();

Tracker.Instance.Start(collectorHostname, "snowplow-demo-app.db", l: logger, endpointPort: port);

// page view
Tracker.Instance.Track(new PageView().SetPageUrl("http://helloworld.com/sample/sample.php").Build());

// mobile screen view
MobileScreenView msv = new MobileScreenView("00000000-0000-0000-0000-000000000000", "name")
    .SetType("type")
    .SetPreviousName("previousName")
    .SetPreviousId("00000000-0000-0000-0000-000000000000")
    .SetPreviousType("previousType")
    .SetTransitionType("transitionType")
    .Build();
Tracker.Instance.Track(msv);

// old screen view
Tracker.Instance.Track(new ScreenView()
    .SetId("example-screen-id")
    .SetName("Example Screen")
    .Build());

// self-describing event
SelfDescribingJson sdj = new SelfDescribingJson("iglu:com.snowplowanalytics.snowplow/timing/jsonschema/1-0-0", new Dictionary<string, object> {
    { "category", "SdjTimingCategory" },
    { "variable", "SdjTimingVariable" },
    { "timing", 0 },
    { "label", "SdjTimingLabel" }
});
Tracker.Instance.Track(new SelfDescribing()
    .SetEventData(sdj)
    .Build());

// timing event
Tracker.Instance.Track(new Timing()
    .SetCategory("category")
    .SetVariable("variable")
    .SetTiming(5)
    .SetLabel("label")
    .Build());

// structured event
Tracker.Instance.Track(new Structured()
    .SetCategory("exampleCategory")
    .SetAction("exampleAction")
    .SetLabel("exampleLabel")
    .SetProperty("exampleProperty")
    .SetValue(17)
    .Build());

// ecommerce transaction
var item1 = new EcommerceTransactionItem().SetSku("pbz0026").SetPrice(20).SetQuantity(1).Build();
var item2 = new EcommerceTransactionItem().SetSku("pbz0038").SetPrice(15).SetQuantity(1).SetName("shirt").SetCategory("clothing").Build();
var items = new List<EcommerceTransactionItem> { item1, item2 };
Tracker.Instance.Track(new EcommerceTransaction()
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

Tracker.Instance.Flush();
Tracker.Instance.Stop();

Console.WriteLine("Demo app finished");
Console.Out.Flush();
