using System.Collections.Generic;
using System.Linq;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorHistory : Dictionary<ThreadVectorClock,List<ThreadEvent>>
    {
        /// <summary>
        /// Adds new VectorEvent to History. If an Event is in the History with the same Clock it's merged
        /// </summary>
        /// <param name="clock">Clock of the Event</param>
        /// <param name="threadEvent">Event that has to be saved</param>
        public void AddEvent(ThreadVectorClock clock, ThreadEvent threadEvent )
        {
            List<ThreadEvent> events;
            if (!TryGetValue(clock, out events))
            {
                Add(clock, new List<ThreadEvent> {threadEvent});
            }
            else
            {
                ThreadEvent foundEvent = events.SingleOrDefault(x => x.CompareRessourceAndLock(threadEvent));
                if (foundEvent != null && foundEvent.ThreadEventType < threadEvent.ThreadEventType)
                {
                    foundEvent.ThreadEventType = threadEvent.ThreadEventType;
                }
                else if (foundEvent == null)
                {
                    events.Add(threadEvent);
                }
            }
        }
    }
}
