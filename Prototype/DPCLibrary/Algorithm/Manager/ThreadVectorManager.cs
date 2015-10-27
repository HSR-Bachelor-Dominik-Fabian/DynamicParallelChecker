using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DPCLibrary.Algorithm.Manager
{
    sealed class ThreadVectorManager
    {
        private static volatile ThreadVectorManager _instance;
        private static readonly object _syncRoot = new object();

        public static ThreadVectorManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new ThreadVectorManager();
                    }
                }
            }
            return _instance;
        }

        private ThreadVectorManager() { }

        internal static void Reset()
        {
            _instance = null;
        }

        private readonly Dictionary<int, ThreadVectorInstance> _threadVectorPool
            = new Dictionary<int, ThreadVectorInstance>();

        private readonly LockHistory _lockHistory = new LockHistory();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleReadAccess(int threadId, int ressource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, ressource, threadVectorInstance.LockRessource);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleWriteAccess(int threadId, int ressource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Write, ressource, threadVectorInstance.LockRessource);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleLock(int threadId, int lockRessource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            KeyValuePair<int, ThreadVectorClock> lockThreadIdClockPair;
            if (CheckLockHistory(lockRessource, out lockThreadIdClockPair))
            {
                SynchronizeVectorClock(threadId, lockThreadIdClockPair);
            }
            threadVectorInstance.LockRessource = lockRessource;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleUnLock(int threadId, int lockRessource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
            _lockHistory.AddLockEntry(lockRessource, new KeyValuePair<int, ThreadVectorClock>(threadId, threadVectorInstance.VectorClock));
            threadVectorInstance.IncrementClock();
            threadVectorInstance.LockRessource = 0;
        }

        private ThreadVectorInstance GetThreadVectorInstance(int threadId)
        {
            ThreadVectorInstance threadVectorInstance;
            if (!_threadVectorPool.TryGetValue(threadId, out threadVectorInstance))
            {
                threadVectorInstance = new ThreadVectorInstance(threadId);
                _threadVectorPool.Add(threadId, threadVectorInstance);
            }
            return threadVectorInstance;
        }

        private bool CheckLockHistory(int lockRessource, out KeyValuePair<int, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _lockHistory.IsRessourceInLockHistory(lockRessource, out lockThreadIdClockPair);
        }

        private void SynchronizeVectorClock(int ownThreadId, KeyValuePair<int, ThreadVectorClock> lockThreadIdClockPair)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(ownThreadId);
            ThreadVectorClock vectorClock = threadVectorInstance.VectorClock;
            vectorClock.Keys.ToList().ForEach(threadId =>
            {
                if (threadId == ownThreadId)
                {
                    vectorClock[threadId] += 1;
                }
                else if (threadId == lockThreadIdClockPair.Key)
                {
                    vectorClock[threadId] = lockThreadIdClockPair.Value[threadId];
                }
                else if (lockThreadIdClockPair.Value.ContainsKey(threadId))
                {
                    vectorClock[threadId] = Math.Max(vectorClock[threadId], lockThreadIdClockPair.Value[threadId]);
                }
            });
            lockThreadIdClockPair.Value.ToList().ForEach(x => { if(!vectorClock.ContainsKey(x.Key)) vectorClock.Add(x.Key, x.Value);});

            _threadVectorPool[ownThreadId] = threadVectorInstance;
        }


        private void CheckForRaceCondition(ThreadEvent ownThreadEvent, ThreadVectorInstance threadVectorInstance)
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

        private bool IsRaceCondition(ThreadEvent me, ThreadEvent other)
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
