/*
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
 */

using System;
using System.Collections.Generic;
using Snowplow.Tracker.Storage;
using System.Messaging;

namespace Snowplow.Tracker.PlatformExtensions
{
    /// <summary>
    /// Constructs an MSMQ Storage Target
    /// </summary>
    public class MSMQStorage : IStorage, IDisposable
    {
        private MessageQueue _messageQueue;
        private string _queueName = @".\Private$\SnowplowTracker";
        private const string _defaultQueueDesc = @"Snowplow dotnet tracker message queue";

        /// <summary>
        /// Builds a new MSMQ Target
        /// </summary>
        /// <param name="queueName">Name of the queue</param>
        public MSMQStorage(string queueName)
        {
            _queueName = queueName;
            if (!MessageQueue.Exists(_queueName))
            {
                MessageQueue.Create(_queueName);
            }
            _messageQueue = new MessageQueue(_queueName);
            _messageQueue.Label = _defaultQueueDesc;
        }

        /// <summary>
        /// Fetches the total items in the queue
        /// </summary>
        public int TotalItems
        {
            get
            {
                int count = 0;
                var enumerator = _messageQueue.GetMessageEnumerator2();
                while (enumerator.MoveNext())
                    count++;

                return count;
            }
        }

        /// <summary>
        /// Removes all entries from the queue
        /// </summary>
        public void Purge()
        {
            _messageQueue.Purge();
        }

        /// <summary>
        /// Removes a list of entries from the queue
        /// </summary>
        /// <param name="idList">The id list</param>
        /// <returns>Whether the removal was a success</returns>
        public bool Delete(List<string> idList)
        {
            int count = 0;
            foreach (var id in idList)
            {
                try
                {
                    _messageQueue.ReceiveById(id);
                    count++;
                }
                catch (System.InvalidOperationException)
                {

                }

            }
            return count == idList.Count;
        }

        /// <summary>
        /// Adds an entry to the queue
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Put(string item)
        {
            _messageQueue.Send(item);
        }

        /// <summary>
        /// Grabs a range of items from the queue
        /// </summary>
        /// <param name="n">The count to get</param>
        /// <returns>The list of records</returns>
        public List<StorageRecord> TakeLast(int n)
        {
            var lst = new List<StorageRecord>(n);

            using (var enumerator = _messageQueue.GetMessageEnumerator2())
            {
                for (int i = 0; i < n && enumerator.MoveNext(); i++)
                {

                    var item = enumerator.Current;
                    var wrapped = new StorageRecord
                    {
                        Id = item.Id,
                        Item = item.Body.ToString()
                    };

                    lst.Insert(0, wrapped);
                }
            }

            return lst;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _messageQueue.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MSMQStorage() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
