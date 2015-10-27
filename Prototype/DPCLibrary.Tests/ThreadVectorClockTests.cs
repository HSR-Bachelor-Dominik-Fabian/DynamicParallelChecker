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
            int threadId = 1;
            // ReSharper disable once CollectionNeverUpdated.Local
            ThreadVectorClock clock = new ThreadVectorClock(threadId);
            Assert.AreEqual(threadId, clock.OwnThreadId);
            int value;
            Assert.IsTrue(clock.TryGetValue(threadId,out value));
            Assert.AreEqual(1,value);
        }

        [TestMethod]
        public void TestPositivEquals()
        {
            int threadId = 1;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId);
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId);
            Assert.AreEqual(clock1,clock2);
        }
        [TestMethod]
        public void TestNegativEquals()
        {
            int threadId = 1;
            int threadId2 = 2;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId);
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId2);
            Assert.AreNotEqual(clock1, clock2);
        }

        [TestMethod]
        public void TestNegativChangedEquals()
        {
            int threadId = 1;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId);
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId) {[threadId] = 2};
            Assert.AreNotEqual(clock1, clock2);
        }
        [TestMethod]
        public void TestNullEquals()
        {
            int threadId = 1;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId);
            Assert.AreNotEqual(null, clock1);
        }
        [TestMethod]
        public void TestNegativEqualsWithEqualVectors()
        {
            int threadId = 1;
            int threadId2 = 2;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId) {{threadId2, 1}};
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId2) {{threadId, 1}};
            Assert.AreNotEqual(clock1, clock2);
        }

        [TestMethod]
        public void TestHappendAfterCompareTo()
        {
            int threadId = 1;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId);
            clock1[threadId] += 1;
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId);
            Assert.AreEqual(1,clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestConcurrentCompareTo()
        {
            int threadId = 1;
            int threadId2 = 2;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId) { { threadId2,0}};
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId2) { { threadId, 0 } };
            Assert.AreEqual(0, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestHappendBeforeCompareTo()
        {
            int threadId = 1;
            int threadId2 = 2;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId) { { threadId2, 0 } };
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId2) { { threadId, 1 } };
            Assert.AreEqual(-1, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestConcurrent2CompareTo()
        {
            int threadId = 1;
            int threadId2 = 2;
            int threadId3 = 3;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId) { { threadId2, 0 } };
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId2) { { threadId3, 1 } };
            Assert.AreEqual(0, clock1.HappenedBefore(clock2));
        }
        [TestMethod]
        public void TestHappendAfter2CompareTo()
        {
            int threadId = 1;
            int threadId2 = 2;
            int threadId3 = 3;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId) { { threadId2, 1 } };
            clock1[threadId] += 1;
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId2) { { threadId3, 1 } };
            Assert.AreEqual(1, clock1.HappenedBefore(clock2));
        }

        [TestMethod]
        public void TestHappendAfter3CompareTo()
        {
            int threadId = 1;
            int threadId2 = 2;
            int threadId3 = 3;
            ThreadVectorClock clock1 = new ThreadVectorClock(threadId) { { threadId3, 1 } };
            ThreadVectorClock clock2 = new ThreadVectorClock(threadId2) { { threadId, 1 } };
            clock2[threadId] += 1;
            Assert.AreEqual(-1, clock1.HappenedBefore(clock2));
        }
    }
}
