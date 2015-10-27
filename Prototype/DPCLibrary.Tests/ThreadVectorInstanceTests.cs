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
            Thread thread = Thread.CurrentThread;
            ThreadVectorInstance instance = new ThreadVectorInstance(thread);
            Assert.AreEqual(thread,instance.Thread);
            Assert.AreEqual(0, instance.LockRessource);
        }
        [TestMethod]
        public void TestIncrementClock()
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorInstance instance = new ThreadVectorInstance(thread);
            instance.IncrementClock();
            Assert.AreEqual(2, instance.VectorClock[thread]);
        }

        [TestMethod]
        public void TestIncrementMultipleClock()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorInstance instance = new ThreadVectorInstance(thread);
            instance.VectorClock.Add(thread2, 1);
            instance.IncrementClock();
            Assert.AreEqual(2, instance.VectorClock[thread]);
            Assert.AreEqual(1, instance.VectorClock[thread2]);
        }

        [TestMethod]
        public void TestWriteHistory()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorInstance instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read,123));
            ThreadVectorHistory dict = instance.GetConcurrentHistory(new ThreadVectorClock(thread2));
            Assert.AreEqual(1,dict[new ThreadVectorClock(thread)].Count);
        }

        [TestMethod]
        public void TestWriteHistory2()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorInstance instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read,3232));
            ThreadVectorHistory dict = instance.GetConcurrentHistory(new ThreadVectorClock(thread2));
            Assert.AreEqual(2, dict[new ThreadVectorClock(thread)].Count);
        }

        [TestMethod]
        public void TestWriteHistory3()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorInstance instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123));
            ThreadVectorHistory dict = instance.GetConcurrentHistory(new ThreadVectorClock(thread2));
            Assert.AreEqual(1, dict[new ThreadVectorClock(thread)].Count);
        }

        [TestMethod]
        public void TestGetConcurrentHistory()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorInstance instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123));
            ThreadVectorHistory dict = instance.GetConcurrentHistory(new ThreadVectorClock(thread2));
            Assert.AreEqual(1, dict[new ThreadVectorClock(thread)].Count);
        }

        [TestMethod]
        public void TestGetConcurrentHistoryNegativ()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            ThreadVectorInstance instance = new ThreadVectorInstance(thread);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123));
            var clock2 = new ThreadVectorClock(thread2) {{thread, 2}};
            ThreadVectorHistory dict = instance.GetConcurrentHistory(clock2);
            Assert.AreEqual(0, dict.Keys.Count);
        }
    }
}
