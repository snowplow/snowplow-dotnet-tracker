using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Snowplow.Tracker.Tests.Storage
{
    [TestClass]
    public class LiteDBStorageTest
    {
        private string getTempFile()
        {
            var fn = Path.GetTempFileName();
            File.Delete(fn);
            return fn;
        }

        [TestMethod]
        public void testDbInit()
        {
            var fn = getTempFile();
            int itemCount = -1;

            using (var storage = new LiteDBStorage(fn))
            {
                itemCount = storage.TotalItems;
            }

            File.Delete(fn);
            Assert.AreEqual(0, itemCount);
        }

        [TestMethod]
        public void testDbPutGetOne()
        {
            var fn = getTempFile();
            try
            {
                using (var storage = new LiteDBStorage(fn))
                {
                    Assert.AreEqual(0, storage.TotalItems);

                    var expected = "hello world";
                    storage.Put(expected);

                    Assert.AreEqual(1, storage.TotalItems);

                    var actual = storage.TakeLast(1);

                    Assert.AreEqual(0, storage.TotalItems);

                    Assert.AreEqual(expected, actual[0]);
                } 
            } finally {
                File.Delete(fn);
            }                       
        }

        [TestMethod]
        public void testDbPutMany()
        {
            var fn = getTempFile();
            int insertionCount = 100;
            var expected = new List<string>();
            try
            {
                using (var storage = new LiteDBStorage(fn))
                {
                    Assert.AreEqual(0, storage.TotalItems);


                    for (int i=0; i<insertionCount; i++)
                    {
                        var generated = String.Format("{0}", i);
                        storage.Put(generated);
                        expected.Insert(0, generated);
                    }

                    Assert.AreEqual(insertionCount, storage.TotalItems);



                    var actual = storage.TakeLast(insertionCount);

                    Assert.AreEqual(0, storage.TotalItems);

                    CollectionAssert.AreEqual(expected, actual);
                }
            }
            finally
            {
                File.Delete(fn);
            }
        }

        [TestMethod]
        public void testDbPersistence()
        {
            var fn = getTempFile();
            try
            {
                using (var storage = new LiteDBStorage(fn))
                {
                    Assert.AreEqual(0, storage.TotalItems);

                    var expected = "hello world";
                    storage.Put(expected);

                    Assert.AreEqual(1, storage.TotalItems);
                }

                using (var reopenedStorage = new LiteDBStorage(fn))
                {
                    Assert.AreEqual(1, reopenedStorage.TotalItems);

                    var actual = reopenedStorage.TakeLast(1);

                    Assert.AreEqual(0, reopenedStorage.TotalItems);

                    Assert.AreEqual("hello world", actual[0]);
                }
            }
            finally
            {
                File.Delete(fn);
            }
        }
    }
}
