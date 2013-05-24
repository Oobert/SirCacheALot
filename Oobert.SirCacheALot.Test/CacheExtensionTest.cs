using System;
using Xunit;
using Oobert.SirCacheALot;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Caching;

namespace Oobert.SirCacheALot.Test
{
    public class CacheExtensionTest
    {
        [Fact]
        public void CacheSyntax()
        {
            TestClass obj = new TestClass();

            var sum = obj.Cache(x => x.Add(2, 3));

            Assert.Equal(5, sum);
        }

        [Fact]
        public void CacheAlternitiveSyntax()
        {
            TestClass obj = new TestClass();

            var sum = CacheExtension.Cache(() => obj.Add(3, 3));

            Assert.Equal(6, sum);
        }

        [Fact]
        public void CacheAlternitiveSyntaxTwo()
        {
            TestClass obj = new TestClass();

            var sum = CacheExtension.Cache(() => TestClass.StaticAdd(4, 3));

            Assert.Equal(7, sum);
        }

        [Fact]
        public void CacheAlternitiveSyntaxThree()
        {
            TestClass obj = new TestClass();

            var sum = obj.Cache(x => x.Add(10, 1), "TenPlusOne");

            Assert.Equal(11, sum);
        }

        [Fact]
        public void CacheAlternitiveSyntaxFour()
        {
            TestClass obj = new TestClass();

            var sum = CacheExtension.Cache(() => obj.Add(12, 2), "TwelvePlusTwo");

            Assert.Equal(14, sum);
        }

        [Fact]
        public void CacheAlternitiveSyntaxFive()
        {
            TestClass obj = new TestClass();

            var sum = CacheExtension.Cache(() => TestClass.StaticAdd(4, 4), "FourPlusFour");

            Assert.Equal(8, sum);
        }

        [Fact]
        public void CacheStoresData()
        {
            TestClass obj = new TestClass();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var sum = obj.Cache(x => x.SlowAdd(1, 2));
            var sum2 = obj.Cache(x => x.SlowAdd(1, 2));

            sw.Stop();

            Assert.Equal(3, sum);
            Assert.Equal(3, sum2);
            Assert.True(sw.ElapsedMilliseconds < 2500);
        }

        [Fact]
        public void CacheAcceptsClassArgumentsWithToStringOverride()
        {
            var int1 = new IntWrapper() { TheInt = 5 };
            var int2 = new IntWrapper() { TheInt = 6 };
            TestClass obj = new TestClass();

            var sum = obj.Cache(x => x.AddIntWrapper(int1, int2));

            Assert.Equal(11, sum);

        }

        [Fact]
        public void SpeedTest()
        {
            TestClass obj = new TestClass();
            int sum;
            int iterations = 10000;
            Stopwatch sw = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();
            Stopwatch sw3 = new Stopwatch();

            sw.Start();

            for (int x = 0; x < iterations; x++)
            {
                sum = obj.Cache(o => o.Add(1, 1));
            }
            sw.Stop();

            sw2.Start();
            for (int x = 0; x < iterations; x++)
            {
                sum = obj.Cache(o => o.Add(1, 2), "OnePlusTwo");
            }
            sw2.Stop();

            sw3.Start();
            for (int x = 0; x < iterations; x++)
            {
                if (MemoryCache.Default.Contains("testHash"))
                {
                    sum = (int)MemoryCache.Default.Get("testHash");
                }
                else
                {
                    sum = 1 + 1;
                    MemoryCache.Default.Set("testHash", sum, DateTimeOffset.Now.AddMilliseconds(2000));
                }
            }
            sw3.Stop();

        }
    }

    public class TestClass
    {
        public static int StaticAdd(int x, int y)
        {
            return x + y;
        }

        public int Add(int x, int y)
        {
            return x + y;
        }

        public int SlowAdd(int x, int y)
        {
            Thread.Sleep(2000);
            return x + y;
        }

        public int AddIntWrapper(IntWrapper x, IntWrapper y)
        {
            return x.TheInt + y.TheInt;
        }
    }

    public class IntWrapper
    {
        public int TheInt{get;set;}

        public override string ToString()
        {
            return TheInt.ToString();
        }
    }
}
