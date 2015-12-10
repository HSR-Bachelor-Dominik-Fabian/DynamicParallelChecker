using System.Threading;
using DPCLibrary.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DPCLibrary.Tests
{
    [TestClass]
    public class ThreadVectorInstanceTests
    {
        [TestMethod]
        public void TestConstructor()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            var instance = new ThreadVectorInstance(thread);
            Assert.AreEqual(thread,instance.ThreadId);
            Assert.AreEqual(0, instance.LockRessource);
        }
        [TestMethod]
        public void TestIncrementClock()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            var instance = new ThreadVectorInstance(thread);
            instance.IncrementClock();
            Assert.AreEqual(2, instance.VectorClock[thread]);
        }

        [TestMethod]
        public void TestIncrementMultipleClock()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var instance = new ThreadVectorInstance(thread);
            instance.VectorClock.Add(thread2, 1);
            instance.IncrementClock();
            Assert.AreEqual(2, instance.VectorClock[thread]);
            Assert.AreEqual(1, instance.VectorClock[thread2]);
        }

        [TestMethod]
        public void TestWriteHistory()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read,123, 2, "TestMethodName"));
            var dict = instance.GetConcurrentHistory(new ThreadVectorClock(thread2));
            Assert.AreEqual(1,dict[new ThreadVectorClock(thread)].Count);
        }

        [TestMethod]
        public void TestWriteHistory2()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123, 2, "TestMethodName"));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read,3232, 2, "TestMethodName"));
            var dict = instance.GetConcurrentHistory(new ThreadVectorClock(thread2));
            Assert.AreEqual(2, dict[new ThreadVectorClock(thread)].Count);
        }

        [TestMethod]
        public void TestWriteHistory3()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123, 2, "TestMethodName"));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123, 2, "TestMethodName"));
            var dict = instance.GetConcurrentHistory(new ThreadVectorClock(thread2));
            Assert.AreEqual(1, dict[new ThreadVectorClock(thread)].Count);
        }

        [TestMethod]
        public void TestGetConcurrentHistory()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123, 2, "TestMethodName"));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123, 2, "TestMethodName"));
            var dict = instance.GetConcurrentHistory(new ThreadVectorClock(thread2));
            Assert.AreEqual(1, dict[new ThreadVectorClock(thread)].Count);
        }

        [TestMethod]
        public void TestGetConcurrentHistoryNegativ()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            var instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123, 2, "TestMethodName"));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123, 2, "TestMethodName"));
            var clock2 = new ThreadVectorClock(thread2) {{thread, 2}};
            var dict = instance.GetConcurrentHistory(clock2);
            Assert.AreEqual(0, dict.Keys.Count);
        }
    }
}
