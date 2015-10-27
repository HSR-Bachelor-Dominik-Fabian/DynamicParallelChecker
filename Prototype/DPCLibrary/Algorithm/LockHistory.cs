using System.Collections.Generic;
using System.Threading;

namespace DPCLibrary.Algorithm
{
    class LockHistory
    {
        private readonly Dictionary<int, KeyValuePair<Thread, ThreadVectorClock>> _history;
        
        public LockHistory()
        {
            _history = new Dictionary<int, KeyValuePair<Thread, ThreadVectorClock>>();
        }

        public bool IsRessourceInLockHistory(int lockRessource, out KeyValuePair<Thread, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _history.TryGetValue(lockRessource, out lockThreadIdClockPair);
        }

        public void AddLockEntry(int lockRessource, KeyValuePair<Thread, ThreadVectorClock> lockThreadIdClockPair)
        {
            if (_history.ContainsKey(lockRessource))
            {
                _history[lockRessource] = new KeyValuePair<Thread, ThreadVectorClock>(lockThreadIdClockPair.Key, lockThreadIdClockPair.Value.GetCopy());
            }
            else
            {
                _history.Add(lockRessource, new KeyValuePair<Thread, ThreadVectorClock>(lockThreadIdClockPair.Key, lockThreadIdClockPair.Value.GetCopy()));
            }
        }
    }
}
