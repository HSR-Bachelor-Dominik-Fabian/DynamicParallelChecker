using System.Collections.Generic;

namespace DPCLibrary.Algorithm.Manager
{
    class ThreadVectorManager
    {

        private static readonly Dictionary<int, ThreadVectorInstance> _threadVectorPool
            = new Dictionary<int, ThreadVectorInstance>();

        private static readonly LockHistory _lockHistory = new LockHistory();

        public static void HandleReadAccess(int threadId, long ressource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, ressource, threadVectorInstance.LockRessource);
            threadVectorInstance.WriteHistory(threadEvent);
        }

        public static void HandleWriteAccess(int threadId, long ressource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Write, ressource, threadVectorInstance.LockRessource);
            threadVectorInstance.WriteHistory(threadEvent);
        }

        public static void HandleLock(int threadId, long lockRessource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            KeyValuePair<int, ThreadVectorClock> lockThreadIdClockPair;
            if (CheckLockHistory(lockRessource, out lockThreadIdClockPair))
            {
                SynchronizeVectorClock(threadId, lockThreadIdClockPair);
            }
            threadVectorInstance.LockRessource = lockRessource;
        }

        public static void HandleUnLock(int threadId, long lockRessource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            _lockHistory.AddLockEntry(lockRessource, new KeyValuePair<int, ThreadVectorClock>(threadId, threadVectorInstance.VectorClock));
            threadVectorInstance.IncrementClock();
            threadVectorInstance.LockRessource = 0L;
        }

        private static ThreadVectorInstance GetThreadVectorInstance(int threadId)
        {
            ThreadVectorInstance threadVectorInstance;
            if (!_threadVectorPool.TryGetValue(threadId, out threadVectorInstance))
            {
                threadVectorInstance = new ThreadVectorInstance(threadId);
                _threadVectorPool.Add(threadId, threadVectorInstance);
            }
            return threadVectorInstance;
        }

        private static bool CheckLockHistory(long lockRessource, out KeyValuePair<int, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _lockHistory.IsRessourceInLockHistory(lockRessource, out lockThreadIdClockPair);
        }

        private static void SynchronizeVectorClock(int ownThreadId, KeyValuePair<int, ThreadVectorClock> lockThreadIdClockPair)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(ownThreadId);

            // TODO:Fabian sync

            _threadVectorPool.Add(ownThreadId, threadVectorInstance);
        }
    }
}
