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
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            // ReSharper disable once CollectionNeverUpdated.Local
            var clock = new ThreadVectorClock(thread);
            Assert.AreEqual(thread, clock.OwnThreadId);
            int value;
            Assert.IsTrue(clock.TryGetValue(thread,out value));
            Assert.AreEqual(1,value);
        }

        [TestMethod]
        public void TestPositivEquals()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            var clock1 = new ThreadVectorClock(thread);
            var clock2 = new ThreadVectorClock(thread);
            Assert.AreEqual(clock1,clock2);
        }
        [TestMethod]
        public void TestNegativEquals()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var clock1 = new ThreadVectorClock(thread);
            var clock2 = new ThreadVectorClock(thread2);
            Assert.AreNotEqual(clock1, clock2);
        }

        [TestMethod]
        public void TestNegativChangedEquals()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            var clock1 = new ThreadVectorClock(thread);
            var clock2 = new ThreadVectorClock(thread) {[thread] = 2 };
            Assert.AreNotEqual(clock1, clock2);
        }
        [TestMethod]
        public void TestNullEquals()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            var clock1 = new ThreadVectorClock(thread);
            Assert.AreNotEqual(null, clock1);
        }
        [TestMethod]
        public void TestNegativEqualsWithEqualVectors()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var clock1 = new ThreadVectorClock(thread) { { thread2, 1 } };
            var clock2 = new ThreadVectorClock(thread2) { { thread, 1 } };
            Assert.AreNotEqual(clock1, clock2);
        }

        [TestMethod]
        public void TestHappendAfterCompareTo()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            var clock1 = new ThreadVectorClock(thread);
            clock1[thread] += 1;
            var clock2 = new ThreadVectorClock(thread);
            Assert.AreEqual(1,clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestConcurrentCompareTo()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var clock1 = new ThreadVectorClock(thread) { { thread2, 0}};
            var clock2 = new ThreadVectorClock(thread2) { { thread, 0 } };
            Assert.AreEqual(0, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestHappendBeforeCompareTo()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var clock1 = new ThreadVectorClock(thread) { { thread2, 0 } };
            var clock2 = new ThreadVectorClock(thread2) { { thread, 1 } };
            Assert.AreEqual(-1, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestConcurrent2CompareTo()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            string thread3 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 2}";
            var clock1 = new ThreadVectorClock(thread) { { thread2, 0 } };
            var clock2 = new ThreadVectorClock(thread2) { { thread3, 1 } };
            Assert.AreEqual(0, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestHappendAfter2CompareTo()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            string thread3 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 2}";
            var clock1 = new ThreadVectorClock(thread) { { thread2, 1 } };
            clock1[thread] += 1;
            var clock2 = new ThreadVectorClock(thread2) { { thread3, 1 } };
            Assert.AreEqual(1, clock1.HappenedBefore(clock2));
        }

        [TestMethod]
        public void TestHappendAfter3CompareTo()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            string thread3 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 2}";
            var clock1 = new ThreadVectorClock(thread) { { thread3, 1 } };
            var clock2 = new ThreadVectorClock(thread2) { { thread, 1 } };
            clock2[thread] += 1;
            Assert.AreEqual(-1, clock1.HappenedBefore(clock2));
        }
    }
}
