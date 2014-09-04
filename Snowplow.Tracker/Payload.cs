using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Snowplow.Tracker
{
    public class Payload
    {
        private Dictionary<string, string> nvPairs;
        public Payload()
        {
            nvPairs = new Dictionary<string, string>();
        }

        public void add(string name, double? value)
        {
            add(name, value.ToString());
        }

        public void add(string name, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                nvPairs.Add(name, value);
            }
        }

        public void addDict(Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> nvPair in dict)
            {
                add(nvPair.Key, nvPair.Value);
            }
        }

        public void addJson(Dictionary <string, object> jsonDict, bool encodeBase64, string typeWhenEncoded, string typeWhenNotEncoded)
        {
            if (jsonDict != null && jsonDict.Count > 0)
            {
                string jsonString = JsonConvert.SerializeObject(jsonDict);
                if (encodeBase64)
                {
                    byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
                    string encodedDict = System.Convert.ToBase64String(plainTextBytes);
                    add(typeWhenEncoded, encodedDict);
                }
                else
                {
                    add(typeWhenNotEncoded, jsonString);
                }
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
