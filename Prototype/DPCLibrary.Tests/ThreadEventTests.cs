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
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 122323);
            Assert.AreEqual(ThreadEvent.EventType.Read, threadEvent.ThreadEventType);
            Assert.AreEqual(122323, threadEvent.Ressource);
        }

        [TestMethod]
        public void TestPositivCompareRessource()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 122323);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Read, 122323);
            Assert.IsTrue(event1.CompareRessource(event2));
        }

        [TestMethod]
        public void TestPositivCompareRessource2()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 122323);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Write, 122323);
            Assert.IsTrue(event1.CompareRessource(event2));
        }

        [TestMethod]
        public void TestNegativRessource()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Write, 1323);
            Assert.IsFalse(event1.CompareRessource(event2));
        }
        [TestMethod]
        public void TestNegativRessource2()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Write, 3123);
            Assert.IsTrue(event1.CompareRessource(event2));
        }
        [TestMethod]
        public void TestNegativRessource3()
        {
            ThreadEvent event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123);
            ThreadEvent event2 = new ThreadEvent(ThreadEvent.EventType.Write, 312323);
            Assert.IsFalse(event1.CompareRessource(event2));
        }
    }
}