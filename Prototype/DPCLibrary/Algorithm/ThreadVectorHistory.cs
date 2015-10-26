using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorHistory:IEnumerable<KeyValuePair<ThreadVectorClock,List<ThreadEvent>>>
    {
        private readonly Dictionary<ThreadVectorClock, List<ThreadEvent>> _dictionary;

        public ThreadVectorHistory()
        {
            _dictionary = new Dictionary<ThreadVectorClock, List<ThreadEvent>>();
        }

        public ThreadVectorHistory(Dictionary<ThreadVectorClock, List<ThreadEvent>> dict)
        {
            _dictionary = dict;
        }

        public Dictionary<ThreadVectorClock, List<ThreadEvent>>.KeyCollection Keys => _dictionary.Keys;
        public Dictionary<ThreadVectorClock, List<ThreadEvent>>.ValueCollection Values => _dictionary.Values;

        public List<ThreadEvent> this[ThreadVectorClock clock]
        {
            get
    {
                return _dictionary.Single(x => x.Key.Equals(clock)).Value;
            }
            set
            {
                _dictionary[_dictionary.Single(x => x.Key.Equals(clock)).Key] = value;
            }
        } 
        
        /// <summary>
        /// Adds new VectorEvent to History. If an Event is in the History with the same Clock it's merged
        /// </summary>
        /// <param name="clock">Clock of the Event</param>
        /// <param name="threadEvent">Event that has to be saved</param>
        public void AddEvent(ThreadVectorClock clock, ThreadEvent threadEvent )
        {
            List<ThreadEvent> events;
            if (!_dictionary.TryGetValue(clock, out events))
            {
                _dictionary.Add(clock, new List<ThreadEvent> {threadEvent});
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

        public IEnumerator<KeyValuePair<ThreadVectorClock, List<ThreadEvent>>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
            }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}
