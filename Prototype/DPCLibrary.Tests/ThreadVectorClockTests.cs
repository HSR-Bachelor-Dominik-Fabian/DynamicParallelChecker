using System.Threading;
using DPCLibrary.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DPCLibrary.Tests
{
    [TestClass]
    public class ThreadVectorClockTests
    {
        [TestMethod]
        public void TestConstructor()
        {
            Thread thread = Thread.CurrentThread;
            // ReSharper disable once CollectionNeverUpdated.Local
            ThreadVectorClock clock = new ThreadVectorClock(thread);
            Assert.AreEqual(thread, clock.OwnThreadId);
            int value;
            Assert.IsTrue(clock.TryGetValue(thread,out value));
            Assert.AreEqual(1,value);
        }

        [TestMethod]
        public void TestPositivEquals()
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorClock clock1 = new ThreadVectorClock(thread);
            ThreadVectorClock clock2 = new ThreadVectorClock(thread);
            Assert.AreEqual(clock1,clock2);
        }
        [TestMethod]
        public void TestNegativEquals()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorClock clock1 = new ThreadVectorClock(thread);
            ThreadVectorClock clock2 = new ThreadVectorClock(thread2);
            Assert.AreNotEqual(clock1, clock2);
        }

        [TestMethod]
        public void TestNegativChangedEquals()
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorClock clock1 = new ThreadVectorClock(thread);
            ThreadVectorClock clock2 = new ThreadVectorClock(thread) {[thread] = 2 };
            Assert.AreNotEqual(clock1, clock2);
        }
        [TestMethod]
        public void TestNullEquals()
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorClock clock1 = new ThreadVectorClock(thread);
            Assert.AreNotEqual(null, clock1);
        }
        [TestMethod]
        public void TestNegativEqualsWithEqualVectors()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorClock clock1 = new ThreadVectorClock(thread) { { thread2, 1 } };
            ThreadVectorClock clock2 = new ThreadVectorClock(thread2) { { thread, 1 } };
            Assert.AreNotEqual(clock1, clock2);
        }

        [TestMethod]
        public void TestHappendAfterCompareTo()
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorClock clock1 = new ThreadVectorClock(thread);
            clock1[thread] += 1;
            ThreadVectorClock clock2 = new ThreadVectorClock(thread);
            Assert.AreEqual(1,clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestConcurrentCompareTo()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorClock clock1 = new ThreadVectorClock(thread) { { thread2, 0}};
            ThreadVectorClock clock2 = new ThreadVectorClock(thread2) { { thread, 0 } };
            Assert.AreEqual(0, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestHappendBeforeCompareTo()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorClock clock1 = new ThreadVectorClock(thread) { { thread2, 0 } };
            ThreadVectorClock clock2 = new ThreadVectorClock(thread2) { { thread, 1 } };
            Assert.AreEqual(-1, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestConcurrent2CompareTo()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            Thread thread3 = new Thread(() => { });
            ThreadVectorClock clock1 = new ThreadVectorClock(thread) { { thread2, 0 } };
            ThreadVectorClock clock2 = new ThreadVectorClock(thread2) { { thread3, 1 } };
            Assert.AreEqual(0, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestHappendAfter2CompareTo()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            Thread thread3 = new Thread(() => { });
            ThreadVectorClock clock1 = new ThreadVectorClock(thread) { { thread2, 1 } };
            clock1[thread] += 1;
            ThreadVectorClock clock2 = new ThreadVectorClock(thread2) { { thread3, 1 } };
            Assert.AreEqual(1, clock1.HappenedBefore(clock2));
        }

        [TestMethod]
        public void TestHappendAfter3CompareTo()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            Thread thread3 = new Thread(() => { });
            ThreadVectorClock clock1 = new ThreadVectorClock(thread) { { thread3, 1 } };
            ThreadVectorClock clock2 = new ThreadVectorClock(thread2) { { thread, 1 } };
            clock2[thread] += 1;
            Assert.AreEqual(-1, clock1.HappenedBefore(clock2));
        }
    }
}
