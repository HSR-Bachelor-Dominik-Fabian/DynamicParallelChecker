using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPCLibrary.Algorithm.Manager
{
    class ThreadVectorManager
    {

        private static Dictionary<int, ThreadVectorInstance> _threadVectorPool
            = new Dictionary<int, ThreadVectorInstance>();

        private static LockHistory _lockHistory = new LockHistory();

        public static void HandleReadAccess(int threadId, long ressource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, ressource, threadVectorInstance.LockRessource);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance);
        }

        public static void HandleWriteAccess(int threadId, long ressource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Write, ressource, threadVectorInstance.LockRessource);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance);
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
            ThreadVectorClock vectorClock = threadVectorInstance.VectorClock;
            foreach (int threadId in vectorClock.Keys)
            {
                if (threadId == ownThreadId)
                {
                    vectorClock[threadId] += 1;
                }
                else if (threadId == lockThreadIdClockPair.Key)
                {
                    vectorClock[threadId] = lockThreadIdClockPair.Value[threadId];
                }
                else
                {
                    vectorClock[threadId] = Math.Max(vectorClock[threadId], lockThreadIdClockPair.Value[threadId]);
                }
            }
            lockThreadIdClockPair.Value.ToList().ForEach(x => { if(!vectorClock.ContainsKey(x.Key)) vectorClock.Add(x.Key, x.Value);});

            _threadVectorPool[ownThreadId] = threadVectorInstance;
        }

        private static void CheckForRaceCondition(ThreadEvent ownThreadEvent, ThreadVectorInstance threadVectorInstance)
        {
            List<ThreadVectorInstance> instances = 
                (_threadVectorPool.Values.Where(instance => instance.ThreadId != threadVectorInstance.ThreadId)).ToList();
            foreach (ThreadVectorInstance instance in instances) {
                foreach (
                    List<ThreadEvent> concurrentEvents in
                        instance.GetConcurrentHistory(threadVectorInstance.VectorClock).Values)
                {
                    foreach (ThreadEvent threadEvent in concurrentEvents)
                    {
                        if (IsRaceCondition(ownThreadEvent, threadEvent))
                        {
                            Console.WriteLine("RaceCondition detected... Ressource: " + ownThreadEvent.Ressource + ", in Thread: " + threadVectorInstance.ThreadId);
                            // TODO:Fabian show message
                        }
                    }
                }
            }
        }

        private static bool IsRaceCondition(ThreadEvent me, ThreadEvent other)
        {
            if (other.Ressource == me.Ressource)
            {
                if (me.ThreadEventType.Equals(ThreadEvent.EventType.Write) 
                    || other.ThreadEventType.Equals(ThreadEvent.EventType.Write))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
