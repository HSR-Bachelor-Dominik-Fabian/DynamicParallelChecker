﻿using System.Collections.Generic;
using System.Linq;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorInstance
    {
        public int ThreadId { get; }

        public long LockRessource { get; set; }

        public ThreadVectorClock VectorClock { get; }

        private readonly ThreadVectorHistory _threadVectorHistory; 

        public ThreadVectorInstance(int threadId)
        {
            ThreadId = threadId;
            VectorClock = new ThreadVectorClock(threadId);
            _threadVectorHistory = new ThreadVectorHistory();
            LockRessource = 0L;
        }

        public void IncrementClock()
        {
            VectorClock[ThreadId] += 1;
        }

        public ThreadVectorHistory GetConcurrentHistory(ThreadVectorClock vectorClock)
        {
            return (ThreadVectorHistory) _threadVectorHistory.Where(historyEntry => (historyEntry.Key.CompareTo(vectorClock) == 0));
        }

        public void WriteHistory(ThreadEvent threadEvent)
        {
            _threadVectorHistory.AddEvent(VectorClock, threadEvent);
        }
    }
}