using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowplow.Tracker.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Snowplow.Tracker.Tests.Storage
{
    [TestClass]
    public class IdGeneratorTest
    {

        [TestMethod]
        public void testSetInitial()
        {
            var k = new IdGenerator();
            Assert.AreEqual(BigInteger.Zero, k.GetAndAdd(0));
            var k2 = new IdGenerator(BigInteger.One);
            Assert.AreEqual(BigInteger.One, k2.GetAndAdd(0));
        }

        [TestMethod]
        public void testGetAndAdd()
        {
            var k = new IdGenerator();
            Assert.AreEqual(BigInteger.One, k.GetAndAdd(1));
            BigInteger add = 1000;
            Assert.AreEqual(add, k.GetAndAdd(999)); 
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Negative numbers are not permitted")]
        public void testSubtract()
        {
            var k = new IdGenerator();
            k.GetAndAdd(-1);
        }

        [TestMethod]
        public void testNoDups()
        {
            var threads = new List<Thread>();
            var id = new IdGenerator();
            var collated = new List<BigInteger>();
            var itperthread = 10000;

            for (int i=0; i<Environment.ProcessorCount; i++)
            {
                var t = new Thread(() => {
                    var items = new List<BigInteger>(itperthread);
                    for (int j=0; j<itperthread; j++)
                    {
                        items.Add(id.GetAndAdd(1));
                    }
                    lock (collated)
                    {
                        collated.AddRange(items);
                    }
                }
                );

                threads.Add(t);
            }

            threads.ForEach((t)=>t.Start());
            threads.ForEach((t) => t.Join());

            Assert.AreEqual(Environment.ProcessorCount * itperthread, collated.Distinct().Count());
        }

    }
}
