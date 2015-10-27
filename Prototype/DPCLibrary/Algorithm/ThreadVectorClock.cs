using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorClock : Dictionary<Thread, int>
    {
        public Thread OwnThread { get; }

        public ThreadVectorClock(Thread thread)
        {
            OwnThread = thread;    
            Add(thread,1);
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

            if(!other.TryGetValue(OwnThread, out myValueInOther) )
            {
                myValueInOther = 0;
            }
            if (!TryGetValue(other.OwnThread, out otherValueInMe))
            {
                otherValueInMe = 0;
            }

            if (TryGetValue(OwnThread, out myValueInMe) && other.TryGetValue(other.OwnThread, out otherValueInOther))
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
            return compared;
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            var clock = obj as ThreadVectorClock;
            if (clock != null)
            {
                ThreadVectorClock dict2 = clock;
                equals = OwnThread == clock.OwnThread && Keys.Count == dict2.Keys.Count && Keys.All(k => dict2.ContainsKey(k) && Equals(dict2[k], this[k]));
            }
            return equals;
        }

        public override int GetHashCode()
        {
            return (OwnThread.GetHashCode()*17 + Keys.Count.GetHashCode())*17 + Keys.Sum(x => x.GetHashCode());
        }

        public ThreadVectorClock GetCopy()
        {
            ThreadVectorClock newClock = new ThreadVectorClock(OwnThread);
            foreach (var key in Keys)
            {
                if (!key.Equals(OwnThread))
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
    }
}
