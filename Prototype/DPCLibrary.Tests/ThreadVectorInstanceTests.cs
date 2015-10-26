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

        //TODO:Dominik: More Tests
    }
}
