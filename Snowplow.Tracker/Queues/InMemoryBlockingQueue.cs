﻿/*
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Snowplow.Tracker.Models;

namespace Snowplow.Tracker.Queues
{
    public class InMemoryBlockingQueue : IPersistentBlockingQueue
    {
        private readonly object _queueLock = new object();

        private ConcurrentDictionary<Guid, Payload> _collection;

        /// <summary>
        /// Create a persistent blocking queue - store queue across executions
        /// </summary>
        /// <param name="s">Interface to storage</param>
        /// <param name="payloadToString">Serialization method</param>
        public InMemoryBlockingQueue()
        {
             _collection = new ConcurrentDictionary<Guid, Payload>();
        }

        /// <summary>
        /// Put a set of items in the queue. The items will be stored on disk. MT Safe
        /// </summary>
        /// <param name="items">Set of items to add to the queue</param>
        public void Enqueue(List<Payload> items)
        {
            lock (_queueLock)
            {
                foreach (var item in items)
                {
                    _collection.TryAdd(Guid.NewGuid(), item);
                }

                Monitor.PulseAll(_queueLock);
            }
        }

        /// <summary>
        /// Peeks at a range of items from the storage target, if possible. Block for maxWait ms if the queue is empty. MT safe
        /// </summary>
        /// <param name="count">Maximum amount of items to dequeue</param>
        /// <param name="maxWait">Maximum number of milliseconds to block</param>
        /// <returns>A list of items taken from the queue</returns>
        public List<Tuple<string, Payload>> Peek(int count, int maxWait = 300)
        {
            var records = new List<Tuple<string, Payload>>();

            lock (_queueLock)
            {
                if (_collection.IsEmpty)
                {
                    if (!Monitor.Wait(_queueLock, maxWait))
                    {
                        return records;
                    }
                }

                foreach (var item in _collection)
                {
                    records.Add(Tuple.Create(item.Key.ToString(), item.Value));

                    if (records.Count == count)
                    {
                        break;
                    }
                }
            }

            return records;
        }

        /// <summary>
        /// The list of ids to remove from the underlying persistent storage target.
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public bool Remove(List<string> idList)
        {
            foreach (var id in idList)
            {
                _collection.TryRemove(Guid.Parse(id), out _);
            }

            return true;
        }
    }
}
