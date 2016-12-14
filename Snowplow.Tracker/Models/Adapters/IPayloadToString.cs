using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Models.Adapters
{
    public interface IPayloadToString
    {
        string ToString(Payload p);
        Payload FromString(string p);
    }
}
