using System;
using System.Collections;
using System.Collections.Generic;
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

        [TestMethod]
        public void TestEnumrator()
        {
            ThreadVectorClock clock = new ThreadVectorClock(1);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);
            ThreadEvent threadEvent2 = new ThreadEvent(ThreadEvent.EventType.Read, 132, 2223);
            ThreadVectorHistory history = new ThreadVectorHistory();
            history.AddEvent(clock, threadEvent);
            history.AddEvent(clock, threadEvent2);
            IEnumerable enumerable = history;
            IEnumerator enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual(2, history[clock].Count);
            KeyValuePair<ThreadVectorClock, List<ThreadEvent>> keyValuePair =
                (KeyValuePair<ThreadVectorClock, List<ThreadEvent>>) enumerator.Current;
            Assert.AreEqual(clock, keyValuePair.Key);
            Assert.AreEqual(threadEvent, keyValuePair.Value[0]);
            Assert.AreEqual(threadEvent2, keyValuePair.Value[1]);
        }

        [TestMethod]
        public void HistoryEventSetPositiv()
        {
            ThreadVectorClock clock = new ThreadVectorClock(1);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);
            ThreadEvent threadEvent2 = new ThreadEvent(ThreadEvent.EventType.Read, 132, 2223);
            ThreadEvent threadEvent3 = new ThreadEvent(ThreadEvent.EventType.Read, 132, 2223);
            ThreadVectorHistory history = new ThreadVectorHistory();
            history.AddEvent(clock, threadEvent);
            history.AddEvent(clock, threadEvent2);
            history[clock] = new List<ThreadEvent>{ threadEvent3};

            Assert.AreEqual(threadEvent3, history[clock][0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void HistoryEventSetNegativ()
        {
            ThreadVectorClock clock = new ThreadVectorClock(1);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);

            // ReSharper disable once UnusedVariable
            var history = new ThreadVectorHistory {[clock] = new List<ThreadEvent> {threadEvent}};
        }

        [TestMethod]
        public void HistoryKeyCollection()
        {
            ThreadVectorClock clock = new ThreadVectorClock(1);
            ThreadVectorClock clock2 = new ThreadVectorClock(2);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);
            ThreadEvent threadEvent2 = new ThreadEvent(ThreadEvent.EventType.Read, 1232132, 2223);
            ThreadVectorHistory history = new ThreadVectorHistory();
            history.AddEvent(clock, threadEvent);
            history.AddEvent(clock2, threadEvent2);

            CollectionAssert.Contains(history.Keys, clock);
            CollectionAssert.Contains(history.Keys, clock2);
        }
    }
}
