using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorHistory:IEnumerable<KeyValuePair<ThreadVectorClock,List<ThreadEvent>>>
    {
        private readonly Logger _logger = LogManager.GetLogger("ThreadVectorHistory");
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
            _logger.ConditionalTrace("Try to Add Event...");
            List<ThreadEvent> events;
            if (!_dictionary.TryGetValue(clock, out events))
            {
                _logger.ConditionalTrace("First Event added at clock: " + clock);
                _dictionary.Add(clock.GetCopy(), new List<ThreadEvent> {threadEvent});
            }
            else
            {
                var foundEvent = events.SingleOrDefault(x => x.CompareRessource(threadEvent));
                if (foundEvent != null && foundEvent.ThreadEventType < threadEvent.ThreadEventType)
                {
                    _logger.ConditionalTrace("Event updated");
                    foundEvent.ThreadEventType = threadEvent.ThreadEventType;
                    foundEvent.Row = threadEvent.Row;
                    foundEvent.MethodName = threadEvent.MethodName;
                }
                else if (foundEvent == null)
                {
                    _logger.ConditionalTrace("Event added to others at clock: " + clock);
                    events.Add(threadEvent);
                }
            }
            _logger.ConditionalTrace("Try to Add Event... Done");
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
