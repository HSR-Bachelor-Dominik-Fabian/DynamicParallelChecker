using System.Linq;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorInstance
    {
        public int ThreadId { get; }

        public int LockRessource { get; set; }

        public ThreadVectorClock VectorClock { get; }

        private readonly ThreadVectorHistory _threadVectorHistory; 

        public ThreadVectorInstance(int threadId)
        {
            ThreadId = threadId;
            VectorClock = new ThreadVectorClock(threadId);
            _threadVectorHistory = new ThreadVectorHistory();
            LockRessource = 0;
        }

        public void IncrementClock()
        {
            VectorClock[ThreadId] += 1;
        }

        public ThreadVectorHistory GetConcurrentHistory(ThreadVectorClock vectorClock)
        {
            return new ThreadVectorHistory(_threadVectorHistory.Where(historyEntry => (historyEntry.Key.CompareTo(vectorClock) == 0)).ToDictionary(x => x.Key, x => x.Value));
        }

        public void WriteHistory(ThreadEvent threadEvent)
        {
            _threadVectorHistory.AddEvent(VectorClock, threadEvent);
        }
    }
}
