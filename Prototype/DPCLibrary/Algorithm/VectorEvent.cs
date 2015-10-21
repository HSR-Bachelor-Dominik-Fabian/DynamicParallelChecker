using System;
using System.Collections.Generic;

namespace DPCLibrary.Algorithm
{
    class VectorEvent : IComparable<ThreadEvent>
    {
        public ThreadVectorClock VectorClock { get; set; }

        private List<ThreadEvent> _events; 

        public VectorEvent()
        {
            VectorClock = new ThreadVectorClock();
            _events = new List<ThreadEvent>();
        }

        public void AddEvent(ThreadEvent threadEvent)
        {
            _events.Add(threadEvent); 
        }

        public int CompareTo(ThreadEvent other)
        {
            throw new NotImplementedException();
        }
    }
}
