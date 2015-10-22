using DPCLibrary.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DPCLibrary.Tests
{
    [TestClass]
    public class ThreadEventTests
    {
        [TestMethod]
        public void TestConstructor()
        {
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 122323, 1233233);
            Assert.AreEqual(ThreadEvent.EventType.Read, threadEvent.ThreadEventType);
            Assert.AreEqual(122323, threadEvent.Ressource);
            Assert.AreEqual(1233233, threadEvent.LockRessource);
        }

        [TestMethod]
        public void TestPositivCompareRessourceAndLock()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 122323, 1233233);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Read, 122323, 1233233);
            Assert.IsTrue(event1.CompareRessourceAndLock(event2));
        }
        [TestMethod]
        public void TestPositivCompareRessourceAndLock2()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 122323, 1233233);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Write, 122323, 1233233);
            Assert.IsTrue(event1.CompareRessourceAndLock(event2));
        }
        [TestMethod]
        public void TestNegativRessourceAndLock()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123, 1233233);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Write, 1323, 1233233);
            Assert.IsFalse(event1.CompareRessourceAndLock(event2));
        }
        [TestMethod]
        public void TestNegativRessourceAndLock2()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123, 1233233);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Write, 3123, 123);
            Assert.IsFalse(event1.CompareRessourceAndLock(event2));
        }
        [TestMethod]
        public void TestNegativRessourceAndLock3()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123, 1233233);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Write, 312323, 123);
            Assert.IsFalse(event1.CompareRessourceAndLock(event2));
        }
    }
}