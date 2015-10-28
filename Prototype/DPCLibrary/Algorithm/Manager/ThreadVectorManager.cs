using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NLog;

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

        private ThreadVectorManager(){}

        internal static void Reset()
        {
            _instance = null;
        }

        private readonly Dictionary<Thread, ThreadVectorInstance> _threadVectorPool
            = new Dictionary<Thread, ThreadVectorInstance>();

        private readonly LockHistory _lockHistory = new LockHistory();

        private readonly Logger _logger = LogManager.GetLogger("ThreadVectorManager");

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleReadAccess(Thread thread, int ressource)
        {
            _logger.ConditionalDebug("ReadAccess: " + thread.ManagedThreadId + " on Ressource: " + ressource);
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(thread);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, ressource);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleWriteAccess(Thread thread, int ressource)
        {
            _logger.ConditionalDebug("WriteAccess: " + thread.ManagedThreadId + " on Ressource: " + ressource);
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(thread);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Write, ressource);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleLock(Thread thread, int lockRessource)
        {
            _logger.ConditionalDebug("Lock: " + thread.ManagedThreadId + " on Ressource: " + lockRessource);
            KeyValuePair<Thread, ThreadVectorClock> lockThreadIdClockPair;
            if (CheckLockHistory(lockRessource, out lockThreadIdClockPair))
            {
                SynchronizeVectorClock(thread, lockThreadIdClockPair);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleUnLock(Thread thread, int lockRessource)
        {
            _logger.ConditionalDebug("Unlock: " + thread.ManagedThreadId + " on Ressource: " + lockRessource);
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(thread);
            _lockHistory.AddLockEntry(lockRessource, new KeyValuePair<Thread, ThreadVectorClock>(thread, threadVectorInstance.VectorClock));
            threadVectorInstance.IncrementClock();
        }

        private ThreadVectorInstance GetThreadVectorInstance(Thread thread)
        {
            ThreadVectorInstance threadVectorInstance;
            if (!_threadVectorPool.TryGetValue(thread, out threadVectorInstance))
            {
                threadVectorInstance = new ThreadVectorInstance(thread);
                _threadVectorPool.Add(thread, threadVectorInstance);
            }
            return threadVectorInstance;
        }

        private bool CheckLockHistory(int lockRessource, out KeyValuePair<Thread, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _lockHistory.IsRessourceInLockHistory(lockRessource, out lockThreadIdClockPair);
        }

        private void SynchronizeVectorClock(Thread ownThread, KeyValuePair<Thread, ThreadVectorClock> lockThreadIdClockPair)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(ownThread);
            ThreadVectorClock vectorClock = threadVectorInstance.VectorClock;
            vectorClock.Keys.ToList().ForEach(thread =>
            {
                if (thread.Equals(ownThread))
                {
                    vectorClock[thread] += 1;
                }
                else if (thread == lockThreadIdClockPair.Key)
                {
                    vectorClock[thread] = lockThreadIdClockPair.Value[thread];
                }
                else if (lockThreadIdClockPair.Value.ContainsKey(thread))
                {
                    vectorClock[thread] = Math.Max(vectorClock[thread], lockThreadIdClockPair.Value[thread]);
                }
            });
            lockThreadIdClockPair.Value.ToList().ForEach(x => { if(!vectorClock.ContainsKey(x.Key)) vectorClock.Add(x.Key, x.Value);});

            _threadVectorPool[ownThread] = threadVectorInstance;
        }


        private void CheckForRaceCondition(ThreadEvent ownThreadEvent, ThreadVectorInstance threadVectorInstance)
        {
            List<ThreadVectorInstance> instances = 
                (_threadVectorPool.Values.Where(instance => instance.Thread != threadVectorInstance.Thread)).ToList();
            foreach (ThreadVectorInstance instance in instances) {
                foreach (
                    List<ThreadEvent> concurrentEvents in
                        instance.GetConcurrentHistory(threadVectorInstance.VectorClock).Values)
                {
                    foreach (ThreadEvent threadEvent in concurrentEvents)
                    {
                        if (IsRaceCondition(ownThreadEvent, threadEvent))
                        {
                            //Console.WriteLine("RaceCondition detected... Ressource: " + ownThreadEvent.Ressource + ", in Thread: " + threadVectorInstance.Thread.ManagedThreadId);
                            
                            _logger.Error("RaceCondition detected... Ressource: " + ownThreadEvent.Ressource + ", in Thread: " + threadVectorInstance.Thread.ManagedThreadId);
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
