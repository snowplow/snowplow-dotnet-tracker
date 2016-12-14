using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Queues
{
    public class PersistentBlockingQueue : IPersistentBlockingQueue
    {

        private IStorage _storage;
        private IPayloadToString _payloadToString;

        private readonly object _queueLock = new Object();

        public PersistentBlockingQueue(IStorage s, IPayloadToString payloadToString)
        {
            _storage = s;
            _payloadToString = payloadToString;
        }

        public void Enqueue(List<Payload> items)
        {
            lock (_queueLock)
            {
                bool waiting = _storage.TotalItems == 0;

                foreach (var item in items)
                {
                    string serialized = _payloadToString.ToString(item);
                    _storage.Put(serialized);
                }

                if (waiting)
                {
                    Monitor.PulseAll(_queueLock);
                }
            }
        }

        public List<Payload> Dequeue()
        {
            lock (_queueLock)
            {
                while(_storage.TotalItems == 0)
                {
                    Monitor.Wait(_queueLock);
                }

                var items = _storage.TakeLast(1);

                var q = from item in items
                        select _payloadToString.FromString(item);

                return q.ToList<Payload>();
            }
        }
    }
}
