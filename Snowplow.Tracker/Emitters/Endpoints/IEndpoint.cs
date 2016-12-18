using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Emitters.Endpoints
{
    public interface IEndpoint
    {
        bool Send(Payload p);
    }
}
