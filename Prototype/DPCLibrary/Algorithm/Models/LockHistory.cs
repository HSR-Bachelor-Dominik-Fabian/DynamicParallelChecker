using System.Collections.Generic;
using NLog;

namespace DPCLibrary.Algorithm.Models
{
    class LockHistory
    {
        private readonly Dictionary<LockRessource, KeyValuePair<ThreadId, ThreadVectorClock>> _history;
        private readonly Logger _logger = LogManager.GetLogger("LockHistory");
        public LockHistory()
        {
            _history = new Dictionary<LockRessource, KeyValuePair<ThreadId, ThreadVectorClock>>();
        }

        public bool IsRessourceInLockHistory(LockRessource lockRessource, out KeyValuePair<ThreadId, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _history.TryGetValue(lockRessource, out lockThreadIdClockPair);
        }

        public void AddLockEntry(LockRessource lockRessource, KeyValuePair<ThreadId, ThreadVectorClock> lockThreadIdClockPair)
        {
            if (_history.ContainsKey(lockRessource))
            {
                _logger.ConditionalTrace("Update Lock Entry on Ressource " + lockRessource + ", with Values: {" + lockThreadIdClockPair.Key +" : " + lockThreadIdClockPair.Value + "}");
                _history[lockRessource] = new KeyValuePair<ThreadId, ThreadVectorClock>(lockThreadIdClockPair.Key, lockThreadIdClockPair.Value.GetCopy());
            }
            else
            {
                _logger.ConditionalTrace("Added Lock Entry on Ressource " + lockRessource + ", with Values: {" + lockThreadIdClockPair.Key + " : " + lockThreadIdClockPair.Value + "}");
                _history.Add(lockRessource, new KeyValuePair<ThreadId, ThreadVectorClock>(lockThreadIdClockPair.Key, lockThreadIdClockPair.Value.GetCopy()));
            }
        }
    }
}
