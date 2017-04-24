using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Snowplow.Tracker.Storage
{
    public class IdGenerator
    {
        BigInteger _value;
        private object locker = new object();

        public IdGenerator(BigInteger value)
        {
            _value = value;
        }

        public IdGenerator()
        {
            _value = BigInteger.Zero;
        }

        public BigInteger GetAndAdd(int value)
        {
            if (value<0)
            {
                throw new ArgumentException("Negative numbers are not permitted");
            }
            lock (locker)
            {
                _value += value;
                return _value;
            }
        }
    }
}
