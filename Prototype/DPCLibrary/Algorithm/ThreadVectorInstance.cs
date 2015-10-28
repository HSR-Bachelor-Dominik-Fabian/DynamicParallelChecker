using System.Linq;
using System.Threading;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorInstance
    {
        public Thread Thread { get; }

        public int LockRessource { get; set; }

        public ThreadVectorClock VectorClock { get; }

        private readonly ThreadVectorHistory _threadVectorHistory; 

        public ThreadVectorInstance(Thread thread)
        {
            Thread = thread;
            VectorClock = new ThreadVectorClock(thread);
            _threadVectorHistory = new ThreadVectorHistory();
            LockRessource = 0;
        }

        public void IncrementClock()
        {
            VectorClock[Thread] += 1;
        }

        public ThreadVectorHistory GetConcurrentHistory(ThreadVectorClock vectorClock)
        {
            return new ThreadVectorHistory(_threadVectorHistory.Where(historyEntry => (historyEntry.Key.HappenedBefore(vectorClock) == 0)).ToDictionary(x => x.Key, x => x.Value));
        }

        public void WriteHistory(ThreadEvent threadEvent)
        {
            _threadVectorHistory.AddEvent(VectorClock, threadEvent);
        }
    }
}
