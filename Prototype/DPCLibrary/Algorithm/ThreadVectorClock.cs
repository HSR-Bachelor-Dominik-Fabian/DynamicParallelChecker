using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorClock : Dictionary<string, int>
    {
        private readonly Logger _logger = LogManager.GetLogger("ThreadVectorClock");
        public string OwnThreadId { get; }

        public ThreadVectorClock(string threadId)
        {
            OwnThreadId = threadId;    
            Add(threadId,1);
        }

        /// <summary>
        /// Happens before Algorithmus. If the Clock is Concurrent or Equal return equals 0, else -1 or 1
        /// </summary>
        /// <param name="other"></param>
        /// <returns>0 for Concurrent, 1 for 'this happend after other' and -1 for 'this happend before other'</returns>
        public int HappenedBefore(ThreadVectorClock other)
        {
            Debug.Assert(!Equals(other),"Logical Error: There shouldn't be two Equal Vectors to compare. For Equals comparison use Equals");
            int compared = 0;
            int myValueInOther, otherValueInMe, myValueInMe, otherValueInOther;

            if(!other.TryGetValue(OwnThreadId, out myValueInOther) )
            {
                myValueInOther = 0;
            }
            if (!TryGetValue(other.OwnThreadId, out otherValueInMe))
            {
                otherValueInMe = 0;
            }

            if (TryGetValue(OwnThreadId, out myValueInMe) && other.TryGetValue(other.OwnThreadId, out otherValueInOther))
            {
                if (myValueInMe >= myValueInOther && otherValueInMe >= otherValueInOther)
                {
                    compared = 1;
                }
                else if (myValueInOther >= myValueInMe && otherValueInOther >= otherValueInMe)
                {
                    compared = -1;
                }
            }
            _logger.ConditionalTrace("(this) " + ToString() + " comparison to (other) " + other + "Result = " + compared);
            return compared;
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            var clock = obj as ThreadVectorClock;
            if (clock != null)
            {
                ThreadVectorClock dict2 = clock;
                equals = OwnThreadId.Equals(clock.OwnThreadId) && Keys.Count == dict2.Keys.Count && Keys.All(k => dict2.ContainsKey(k) && Equals(dict2[k], this[k]));
            }
            return equals;
        }

        public override int GetHashCode()
        {
            return (OwnThreadId.GetHashCode()*17 + Keys.Count.GetHashCode())*17 + Keys.Sum(x => x.GetHashCode());
        }

        public ThreadVectorClock GetCopy()
        {
            ThreadVectorClock newClock = new ThreadVectorClock(OwnThreadId);
            foreach (var key in Keys)
            {
                if (!key.Equals(OwnThreadId))
                {
                    newClock.Add(key, this[key]);
                }
                else
                {
                    newClock[key] = this[key];
                }
            }
            return newClock;
        }

        public override string ToString()
        {
            string output = string.Empty;
            output += "{";
            output = this.Aggregate(output, (current, entry) => current + ("{" + entry.Key + " : " + entry.Value + "}"));
            output += "}";
            return output;
        }
    }
}
