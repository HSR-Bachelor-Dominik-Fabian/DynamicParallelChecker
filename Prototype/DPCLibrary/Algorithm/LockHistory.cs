using System.Collections.Generic;

namespace DPCLibrary.Algorithm
{
    class LockHistory
    {
        private readonly Dictionary<int, KeyValuePair<int, ThreadVectorClock>> _history;
        
        public LockHistory()
        {
            _history = new Dictionary<int, KeyValuePair<int, ThreadVectorClock>>();
        }

        public bool IsRessourceInLockHistory(int lockRessource, out KeyValuePair<int, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _history.TryGetValue(lockRessource, out lockThreadIdClockPair);
        }

        public void AddLockEntry(int lockRessource, KeyValuePair<int, ThreadVectorClock> lockThreadIdClockPair)
        {
            if (_history.ContainsKey(lockRessource))
            {
                _history[lockRessource] = new KeyValuePair<int, ThreadVectorClock>(lockThreadIdClockPair.Key, lockThreadIdClockPair.Value.GetCopy());
            }
            else
            {
                _history.Add(lockRessource, new KeyValuePair<int, ThreadVectorClock>(lockThreadIdClockPair.Key, lockThreadIdClockPair.Value.GetCopy()));
            }
        }
    }
}
