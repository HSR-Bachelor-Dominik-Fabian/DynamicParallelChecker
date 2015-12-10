using System.Linq;
using NLog;

namespace DPCLibrary.Algorithm.Models
{
    class ThreadVectorInstance
    {
        private readonly Logger _logger = LogManager.GetLogger("ThreadVectorInstance");
        public string ThreadId { get; }

        public int LockRessource { get; set; }

        public ThreadVectorClock VectorClock { get; }

        private readonly ThreadVectorHistory _threadVectorHistory; 

        public ThreadVectorInstance(string threadId)
        {
            _logger.ConditionalTrace("New VectorInstance with ThreadId: " + threadId);
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
            return new ThreadVectorHistory(_threadVectorHistory.Where(historyEntry => (historyEntry.Key.HappenedBefore(vectorClock) == 0)).ToDictionary(x => x.Key, x => x.Value));
        }

        public void WriteHistory(ThreadEvent threadEvent)
        {
            _logger.ConditionalTrace("Write History: " + threadEvent.Ressource + " with Level " + threadEvent.ThreadEventType);
            _threadVectorHistory.AddEvent(VectorClock, threadEvent);
        }
    }
}
