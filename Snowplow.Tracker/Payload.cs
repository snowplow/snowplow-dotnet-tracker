using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowplow.Tracker
{
    public class Payload
    {
        private Dictionary<string, string> nvPairs;
        public Payload()
        {
            nvPairs = new Dictionary<string, string>();
        }

        public void add(string name, string value)
        {
            nvPairs.Add(name, value);
        }

        public void addDict(Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> nvPair in dict)
            {
                add(nvPair.Key, nvPair.Value);
            }
        }

        public Dictionary<string, string> NvPairs
        {
            get
            {
                return nvPairs;
            }
        }

    }
}
