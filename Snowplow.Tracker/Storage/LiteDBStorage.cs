/*
 * LiteDBStorage.cs
 * 
 * Copyright (c) 2014-2017 Snowplow Analytics Ltd. All rights reserved.
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
 * Copyright: Copyright (c) 2014-2017 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowplow.Tracker.Storage
{
    public class LiteDBStorage : IStorage, IDisposable
    {
        /// <summary>
        /// The total number of items in the database currently
        /// </summary>
        public int TotalItems { get; private set; }
        private LiteDatabase _db;
        private const string COLLECTION_NAME = "storage";

        private object _dbAccess = new object();

        /// <summary>
        /// Create a new Storage wrappper using LiteDB
        /// </summary>
        /// <param name="path">Filename of database file (doesn't need to exist)</param>
        public LiteDBStorage(string path)
        {
            _db = new LiteDatabase(path);
            if (_db.CollectionExists(COLLECTION_NAME))
            {
                TotalItems = _db.GetCollection<StorageRecord>(COLLECTION_NAME).Count();
            }
            else
            {
                TotalItems = 0;
            }
        }

        /// <summary>
        /// Put an item in the database
        /// </summary>
        /// <param name="item">The item to put in the database</param>
        public void Put(string item)
        {
            var r = new StorageRecord
            {
                Item = item
            };

            lock (_dbAccess)
            {
                var recs = _db.GetCollection<StorageRecord>(COLLECTION_NAME);

                recs.Insert(r);
                TotalItems += 1;
            }
        }

        /// <summary>
        /// Take the last N items added to the database (by insertion order)
        /// </summary>
        /// <param name="n">Number of items to take</param>
        /// <returns>A list of items retrieved from the database</returns>
        public List<StorageRecord> TakeLast(int n)
        {
            lock (_dbAccess)
            {
                var recs = _db.GetCollection<StorageRecord>(COLLECTION_NAME);

                return recs.FindAll()
                    .OrderByDescending(i => { return i.Id; })
                    .Take(n)
                    .ToList<StorageRecord>();
            }
        }

        /// <summary>
        /// Attempts to delete a list of events.
        /// </summary>
        /// <param name="idList"></param>
        public bool Delete(List<long> idList)
        {
            lock (_dbAccess)
            {
                var recs = _db.GetCollection<StorageRecord>(COLLECTION_NAME);
                int failedDeletions = 0;

                foreach (var id in idList)
                {
                    if (recs.Delete(id))
                    {
                        TotalItems -= 1;
                    }
                    else
                    {
                        failedDeletions++;
                    }
                }

                return failedDeletions == 0;
            }
        }

        /// <summary>
        /// Cleanup DB
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup DB
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                }
            }
        }

        ~LiteDBStorage()
        {
            Dispose();
        }

    }
}
