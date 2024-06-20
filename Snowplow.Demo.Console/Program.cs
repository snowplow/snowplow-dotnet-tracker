using Snowplow.Tracker;
using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Models.Events;

string collectorHostname = "http://localhost:9090";
int port = 80;

int count = 100;

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

System.Console.WriteLine("Demo app started - sending " + count + " events to " + collectorHostname + " port " + port);

var logger = new ConsoleLogger();

Tracker.Instance.Start(collectorHostname, "snowplow-demo-app.db", l: logger, endpointPort: port);

for (int i = 0; i < count; i++)
{
    Tracker.Instance.Track(new PageView().SetPageUrl("http://helloworld.com/sample/sample.php").Build());
}

Tracker.Instance.Flush();
Tracker.Instance.Stop();

System.Console.WriteLine("Demo app finished");
System.Console.Out.Flush();
