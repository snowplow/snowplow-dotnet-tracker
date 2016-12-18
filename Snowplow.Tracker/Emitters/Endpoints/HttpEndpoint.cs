using Snowplow.Tracker.Emitters.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Emitters
{
    public class HttpEndpoint : IEndpoint
    {
        public HttpEndpoint(string endpoint,
                            HttpProtocol protocol = HttpProtocol.HTTP,
                            int? port = null,
                            HttpMethod method = HttpMethod.GET)
        {

        }

        public bool Send(Payload p)
        {
            throw new NotImplementedException();
        }
    }
}
