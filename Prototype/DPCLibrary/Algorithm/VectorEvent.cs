using System;
using System.Collections.Generic;
using System.Linq;

namespace DPCLibrary.Algorithm
{
    class VectorEvent
    {
        public ThreadVectorClock VectorClock { get;  }

        private List<ThreadEvent> _events; 

        public VectorEvent(ThreadVectorClock clock)
        {
            VectorClock = clock;
            _events = new List<ThreadEvent>();
        }

        public void AddEvent(ThreadEvent threadEvent)
        {
            _events.Add(threadEvent); 
        }
    }
}
