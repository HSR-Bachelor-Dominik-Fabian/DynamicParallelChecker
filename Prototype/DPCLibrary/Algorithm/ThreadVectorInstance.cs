using System.Collections.Generic;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorInstance
    {
        private readonly int _threadId;

        private ThreadVectorClock _vectorClock;

        private ThreadVectorHistory _threadVectorHistory; 

        public ThreadVectorInstance(int threadId)
        {
            _threadId = threadId;
            _vectorClock = new ThreadVectorClock(threadId);
            _threadVectorHistory = new ThreadVectorHistory();
            
        }
    }
}
