using System.Collections.Generic;
using System.Linq;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorInstance
    {
        public int ThreadId { get; }

        public long LockRessource { get; set; }

        public ThreadVectorClock VectorClock { get; }

        private List<VectorEvent> _threadVectorHistory; 

        public ThreadVectorInstance(int threadId)
        {
            ThreadId = threadId;
            VectorClock = new ThreadVectorClock();
            _threadVectorHistory = new List<VectorEvent>();
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

        public List<VectorEvent> GetHistoryOlderThan(ThreadVectorClock vectorClock)
        {
            //return _threadVectorHistory.Where(vectorEvent => (vectorEvent.VectorClock.CompareTo(vectorClock) == 0));
            return null;
        }

        public void WriteHistory(ThreadEvent threadEvent)
        {
            
        }
    }
}
