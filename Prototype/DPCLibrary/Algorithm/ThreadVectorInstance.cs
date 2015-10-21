using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorInstance
    {
        private readonly int _threadId;

        private ThreadVectorClock _vectorClock;

        private List<VectorEvent> _threadVectorHistory; 

        public ThreadVectorInstance(int threadId)
        {
            _threadId = threadId;
            _vectorClock = new ThreadVectorClock();
            _threadVectorHistory = new List<VectorEvent>();
        }
    }
}
