using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
        public void HandleReadAccess(Thread thread, int ressource, int rowNumber, string methodName)
        {
            _logger.ConditionalDebug("ReadAccess: " + thread.ManagedThreadId + " on Ressource: " + ressource + "on Line:" + rowNumber);
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(thread);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, ressource, rowNumber, methodName);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance, rowNumber, methodName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleWriteAccess(Thread thread, int ressource, int rowNumber, string methodName)
        {
            _logger.ConditionalDebug("WriteAccess: " + thread.ManagedThreadId + " on Ressource: " + ressource + "on Line:" + rowNumber);
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(thread);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Write, ressource,rowNumber, methodName);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance, rowNumber, methodName);
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


        private void CheckForRaceCondition(ThreadEvent ownThreadEvent, ThreadVectorInstance threadVectorInstance, int rowNumber, string methodName)
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
                            LogEventInfo info = new LogEventInfo {Level = LogLevel.Error};
                            info.Properties["RowCount"] = rowNumber;
                            info.Properties["MethodName"] = methodName;
                            info.Properties["ConflictMethodName"] = threadEvent.MethodName;
                            info.Properties["ConflictRow"] = threadEvent.Row;
                            info.Message =
                                $"RaceCondition detected... Ressource: {ownThreadEvent.Ressource} -> Thread: {threadVectorInstance.Thread.ManagedThreadId} to Thread: {instance.Thread.ManagedThreadId}";
                            _logger.Log(info);
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
