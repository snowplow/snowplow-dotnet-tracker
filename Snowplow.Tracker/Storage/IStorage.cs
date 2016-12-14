using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Storage
{
    public interface IStorage
    {
        int TotalItems
        {
            get;
        }

        void Put(string item);
        List<string> TakeLast(int n);
    }
}
