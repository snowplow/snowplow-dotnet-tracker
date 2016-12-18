using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Queues
{
    public interface IPersistentBlockingQueue
    {
        List<Payload> Dequeue(int maxWait);
        void Enqueue(List<Payload> items);
    }
}
