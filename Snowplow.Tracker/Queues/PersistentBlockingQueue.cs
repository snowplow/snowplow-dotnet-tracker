/*
 * PersistentBlockingQueue.cs
 * 
 * Copyright (c) 2021 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Fred Blundun, Ed Lewis, Joshua Beemster
 * Copyright: Copyright (c) 2021 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Snowplow.Tracker.Models;
using Snowplow.Tracker.Models.Adapters;
using Snowplow.Tracker.Storage;

namespace Snowplow.Tracker.Queues
{
    public class PersistentBlockingQueue : IPersistentBlockingQueue
    {
        private IStorage _storage;
        private IPayloadToString _payloadToString;

        private readonly object _queueLock = new Object();

        /// <summary>
        /// Create a persistent blocking queue - store queue across executions
        /// </summary>
        /// <param name="s">Interface to storage</param>
        /// <param name="payloadToString">Serialization method</param>
        public PersistentBlockingQueue(IStorage s, IPayloadToString payloadToString)
        {
            _storage = s;
            _payloadToString = payloadToString;
        }

        /// <summary>
        /// Put a set of items in the queue. The items will be stored on disk. MT Safe
        /// </summary>
        /// <param name="items">Set of items to add to the queue</param>
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

        /// <summary>
        /// Peeks at a range of items from the storage target, if possible. Block for maxWait ms if the queue is empty. MT safe
        /// </summary>
        /// <param name="count">Maximum amount of items to dequeue</param>
        /// <param name="maxWait">Maximum number of milliseconds to block</param>
        /// <returns>A list of items taken from the queue</returns>
        public List<Tuple<long, Payload>> Peek(int count, int maxWait = 300)
        {
            lock (_queueLock)
            {
                while (_storage.TotalItems == 0)
                {
                    if (!Monitor.Wait(_queueLock, maxWait))
                    {
                        return new List<Tuple<long, Payload>>();
                    }
                }

                var records = _storage.TakeLast(count);
                var deserializedRecords = new List<Tuple<long, Payload>>();

                foreach (StorageRecord record in records)
                {
                    deserializedRecords.Add(Tuple.Create(
                        record.Id, _payloadToString.FromString(record.Item)
                    ));
                }

                return deserializedRecords;
            }
        }

        /// <summary>
        /// The list of ids to remove from the underlying persistent storage target.
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public bool Remove(List<long> idList)
        {
            lock (_queueLock)
            {
                var result = _storage.Delete(idList);
                return result;
            }
        }
    }
}
