using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Storage
{
    class StorageRecord
    {
        public int Id { get; set; }
        public string Item { get; set; }
    }

    public class LiteDBStorage : IStorage, IDisposable
    {
        public int TotalItems { get; private set; }
        private LiteDatabase _db;
        private const string COLLECTION_NAME = "storage";

        public LiteDBStorage(string path)
        {
            _db = new LiteDatabase(path);
            if (_db.CollectionExists(COLLECTION_NAME))
            {
                TotalItems = _db.GetCollection<StorageRecord>(COLLECTION_NAME).Count();
            } else
            {
                TotalItems = 0;
            }
        }

        public void Put(string item)
        {
            var r = new StorageRecord
            {
                Item = item
            };

            var recs = _db.GetCollection<StorageRecord>(COLLECTION_NAME);

            recs.Insert(r);
            TotalItems += 1;
        }

        public List<string> TakeLast(int n)
        {
            var recs = _db.GetCollection<StorageRecord>(COLLECTION_NAME);

            var results = recs.FindAll()
                .OrderByDescending(i=> { return i.Id; } )
                .Take(n)
                .ToList<StorageRecord>();

            foreach (var result in results)
            {
                recs.Delete(result.Id);
                TotalItems -= 1;
            }

            var items = from result in results
                        select result.Item;

            return items.ToList();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
