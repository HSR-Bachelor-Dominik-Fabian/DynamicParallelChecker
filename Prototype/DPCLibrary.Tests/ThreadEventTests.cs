using DPCLibrary.Algorithm.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DPCLibrary.Tests
{
    [TestClass]
    public class ThreadEventTests
    {
        [TestMethod]
        public void TestConstructor()
        {
            var threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 122323,2, "TestMethodName");
            Assert.AreEqual(ThreadEvent.EventType.Read, threadEvent.ThreadEventType);
            Assert.AreEqual(122323, threadEvent.Ressource);
        }

        [TestMethod]
        public void TestPositivCompareRessource()
        {
            var event1 = new ThreadEvent(ThreadEvent.EventType.Read, 122323, 2, "TestMethodName");
            var event2 = new ThreadEvent(ThreadEvent.EventType.Read, 122323, 2, "TestMethodName");
            Assert.IsTrue(event1.CompareRessource(event2));
        }

        [TestMethod]
        public void TestPositivCompareRessource2()
        {
            var event1 = new ThreadEvent(ThreadEvent.EventType.Read, 122323, 2, "TestMethodName");
            var event2 = new ThreadEvent(ThreadEvent.EventType.Write, 122323, 2, "TestMethodName");
            Assert.IsTrue(event1.CompareRessource(event2));
        }

        [TestMethod]
        public void TestNegativRessource()
        {
            var event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123, 2, "TestMethodName");
            var event2 = new ThreadEvent(ThreadEvent.EventType.Write, 1323, 2, "TestMethodName");
            Assert.IsFalse(event1.CompareRessource(event2));
        }
        [TestMethod]
        public void TestNegativRessource2()
        {
            var event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123, 2, "TestMethodName");
            var event2 = new ThreadEvent(ThreadEvent.EventType.Write, 3123, 2, "TestMethodName");
            Assert.IsTrue(event1.CompareRessource(event2));
        }
        [TestMethod]
        public void TestNegativRessource3()
        {
            var event1 = new ThreadEvent(ThreadEvent.EventType.Read, 3123, 2, "TestMethodName");
            var event2 = new ThreadEvent(ThreadEvent.EventType.Write, 312323, 2, "TestMethodName");
            Assert.IsFalse(event1.CompareRessource(event2));
        }
    }
}