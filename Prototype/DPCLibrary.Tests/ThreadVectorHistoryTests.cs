using DPCLibrary.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DPCLibrary.Tests
{
    [TestClass]
    public class ThreadVectorHistoryTests
    {
        [TestMethod]
        public void TestAddEvent()
        {
            ThreadVectorClock clock = new ThreadVectorClock(1);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);
            ThreadVectorHistory history = new ThreadVectorHistory();
            history.AddEvent(clock, threadEvent);
            Assert.AreEqual(threadEvent, history[clock][0]);
        }

        [TestMethod]
        public void TestAddEventMultiple()
        {
            ThreadVectorClock clock = new ThreadVectorClock(1);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);
            ThreadEvent threadEvent2 = new ThreadEvent(ThreadEvent.EventType.Read, 132, 2223);
            ThreadVectorHistory history = new ThreadVectorHistory();
            history.AddEvent(clock, threadEvent);
            history.AddEvent(clock,threadEvent2);
            Assert.AreEqual(2, history[clock].Count);
            Assert.AreEqual(threadEvent, history[clock][0]);
            Assert.AreEqual(threadEvent2, history[clock][1]);
        }

        [TestMethod]
        public void TestAddEventSameRessource()
        {
            ThreadVectorClock clock = new ThreadVectorClock(1);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Write, 1232132, 2223);
            ThreadEvent threadEvent2 = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);
            ThreadVectorHistory history = new ThreadVectorHistory();
            history.AddEvent(clock, threadEvent);
            history.AddEvent(clock, threadEvent2);
            Assert.AreEqual(1, history[clock].Count);
            Assert.AreEqual(ThreadEvent.EventType.Write, history[clock][0].ThreadEventType);
        }
        [TestMethod]
        public void TestAddEventSameRessource2()
        {
            ThreadVectorClock clock = new ThreadVectorClock(1);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);
            ThreadEvent threadEvent2 = new ThreadEvent(ThreadEvent.EventType.Write, 1232132, 2223);
            ThreadVectorHistory history = new ThreadVectorHistory();
            history.AddEvent(clock, threadEvent);
            history.AddEvent(clock, threadEvent2);
            Assert.AreEqual(1, history[clock].Count);
            Assert.AreEqual(ThreadEvent.EventType.Write, history[clock][0].ThreadEventType);
        }
    }
}
