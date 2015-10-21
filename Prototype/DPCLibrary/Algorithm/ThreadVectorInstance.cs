using System.Collections.Generic;
using System.Linq;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorInstance
    {
        public int ThreadId { get; }

        public long LockRessource { get; set; }

        public ThreadVectorClock VectorClock { get; }

        private ThreadVectorHistory _threadVectorHistory; 

        public ThreadVectorInstance(int threadId)
        {
            ThreadId = threadId;
            VectorClock = new ThreadVectorClock(threadId);
            _threadVectorHistory = new ThreadVectorHistory();
            LockRessource = 0L;
        }

        public void IncrementClock()
        {
            int ownClock;
            if (VectorClock.TryGetValue(ThreadId, out ownClock))
            {
                ownClock += 1;
                VectorClock.Add(ThreadId, ownClock);
            }
        }

        public ThreadVectorHistory GetConcurrentHistory(ThreadVectorClock vectorClock)
        {
            return (ThreadVectorHistory) _threadVectorHistory.Where(vectorEvent => (vectorEvent.VectorClock.CompareTo(vectorClock) == 0));
        }

        public void WriteHistory(ThreadEvent threadEvent)
        {
            //_threadVectorHistory.Add(VectorClock, threadEvent);
        }
    }
}
