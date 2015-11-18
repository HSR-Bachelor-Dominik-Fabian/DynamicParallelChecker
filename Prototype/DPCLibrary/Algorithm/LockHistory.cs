using System.Collections.Generic;
using NLog;

namespace DPCLibrary.Algorithm
{
    class LockHistory
    {
        private readonly Dictionary<int, KeyValuePair<string, ThreadVectorClock>> _history;
        private readonly Logger _logger = LogManager.GetLogger("LockHistory");
        public LockHistory()
        {
            _history = new Dictionary<int, KeyValuePair<string, ThreadVectorClock>>();
        }

        public bool IsRessourceInLockHistory(int lockRessource, out KeyValuePair<string, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _history.TryGetValue(lockRessource, out lockThreadIdClockPair);
        }

        public void AddLockEntry(int lockRessource, KeyValuePair<string, ThreadVectorClock> lockThreadIdClockPair)
        {
            if (_history.ContainsKey(lockRessource))
            {
                _logger.ConditionalTrace("Update Lock Entry on Ressource " + lockRessource + ", with Values: {" + lockThreadIdClockPair.Key +" : " + lockThreadIdClockPair.Value + "}");
                _history[lockRessource] = new KeyValuePair<string, ThreadVectorClock>(lockThreadIdClockPair.Key, lockThreadIdClockPair.Value.GetCopy());
            }
            else
            {
                _logger.ConditionalTrace("Added Lock Entry on Ressource " + lockRessource + ", with Values: {" + lockThreadIdClockPair.Key + " : " + lockThreadIdClockPair.Value + "}");
                _history.Add(lockRessource, new KeyValuePair<string, ThreadVectorClock>(lockThreadIdClockPair.Key, lockThreadIdClockPair.Value.GetCopy()));
            }
        }
    }
}
