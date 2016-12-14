using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Models.Adapters
{
    public class PayloadToJsonString : IPayloadToString
    {
        public Payload FromString(string p)
        {
            try
            {
                var s = JsonConvert.DeserializeObject<Payload>(p);
                return s;
            }
            catch (Exception e) {
                throw new ArgumentException(String.Format(@"Invalid JSON: ""{0}""", p), e);
            }           
        }

        public string ToString(Payload p)
        {
            return JsonConvert.SerializeObject(p);
        }
    }
}
