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
            ThreadVectorInstance instance = new ThreadVectorInstance(1);
            Assert.AreEqual(1,instance.ThreadId);
            Assert.AreEqual(0, instance.LockRessource);
        }
        [TestMethod]
        public void TestIncrementClock()
        {
            ThreadVectorInstance instance = new ThreadVectorInstance(1);
            instance.IncrementClock();
            Assert.AreEqual(2, instance.VectorClock[1]);
        }

        [TestMethod]
        public void TestIncrementMultipleClock()
        {
            ThreadVectorInstance instance = new ThreadVectorInstance(1);
            instance.VectorClock.Add(2,1);
            instance.IncrementClock();
            Assert.AreEqual(2, instance.VectorClock[1]);
            Assert.AreEqual(1, instance.VectorClock[2]);
        }

        [TestMethod]
        public void TestWriteHistory()
        {
            ThreadVectorInstance instance = new ThreadVectorInstance(1);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read,123,332));
            ThreadVectorHistory dict = instance.GetConcurrentHistory(new ThreadVectorClock(2));
            Assert.AreEqual(1,dict[new ThreadVectorClock(1)].Count);
        }

        [TestMethod]
        public void TestWriteHistory2()
        {
            ThreadVectorInstance instance = new ThreadVectorInstance(1);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123, 332));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read,3232,1223));
            ThreadVectorHistory dict = instance.GetConcurrentHistory(new ThreadVectorClock(2));
            Assert.AreEqual(2, dict[new ThreadVectorClock(1)].Count);
        }

        [TestMethod]
        public void TestWriteHistory3()
        {
            ThreadVectorInstance instance = new ThreadVectorInstance(1);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123, 332));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123, 332));
            ThreadVectorHistory dict = instance.GetConcurrentHistory(new ThreadVectorClock(2));
            Assert.AreEqual(1, dict[new ThreadVectorClock(1)].Count);
        }

        [TestMethod]
        public void TestGetConcurrentHistory()
        {
            ThreadVectorInstance instance = new ThreadVectorInstance(1);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123, 332));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123, 332));
            ThreadVectorHistory dict = instance.GetConcurrentHistory(new ThreadVectorClock(2));
            Assert.AreEqual(1, dict[new ThreadVectorClock(1)].Count);
        }

        [TestMethod]
        public void TestGetConcurrentHistoryNegativ()
        {
            ThreadVectorInstance instance = new ThreadVectorInstance(1);
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Read, 123, 332));
            instance.WriteHistory(new ThreadEvent(ThreadEvent.EventType.Write, 123, 332));
            var clock2 = new ThreadVectorClock(2) {{1, 2}};
            ThreadVectorHistory dict = instance.GetConcurrentHistory(clock2);
            Assert.AreEqual(0, dict.Keys.Count);
        }
    }
}
