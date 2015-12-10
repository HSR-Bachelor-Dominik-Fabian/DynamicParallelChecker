using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using DPCLibrary.Algorithm.Models;
using NLog;

namespace DPCLibrary.Algorithm
{
    sealed class ThreadVectorFacade
    {
        private static volatile ThreadVectorFacade _instance;
        private static readonly object _syncRoot = new object();
        
        public static ThreadVectorFacade GetInstance()
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new ThreadVectorFacade();
                    }
                }
            }
            return _instance;
        }

        private ThreadVectorFacade(){}

        internal static void Reset()
        {
            _instance = null;
        }

        private readonly Dictionary<string, ThreadVectorInstance> _threadVectorPool
            = new Dictionary<string, ThreadVectorInstance>();

        private readonly LockHistory _lockHistory = new LockHistory();

        private readonly Logger _logger = LogManager.GetLogger("ThreadVectorFacade");

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleReadAccess(string thread, int ressource, int rowNumber, string methodName)
        {
            _logger.ConditionalDebug("ReadAccess: " + thread + " on Ressource: " + ressource + "on Line:" + rowNumber);
            var threadVectorInstance = GetThreadVectorInstance(thread);
            var threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, ressource, rowNumber, methodName);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance, rowNumber, methodName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleWriteAccess(string thread, int ressource, int rowNumber, string methodName)
        {
            _logger.ConditionalDebug("WriteAccess: " + thread + " on Ressource: " + ressource + "on Line:" + rowNumber);
            var threadVectorInstance = GetThreadVectorInstance(thread);
            var threadEvent = new ThreadEvent(ThreadEvent.EventType.Write, ressource,rowNumber, methodName);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance, rowNumber, methodName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleLock(string thread, int lockRessource)
        {
            _logger.ConditionalDebug("Lock: " + thread + " on Ressource: " + lockRessource);
            KeyValuePair<ThreadId, ThreadVectorClock> lockThreadIdClockPair;
            if (CheckLockHistory(lockRessource, out lockThreadIdClockPair))
            {
                SynchronizeVectorClock(thread, lockThreadIdClockPair);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleUnLock(string thread, int lockRessource)
        {
            _logger.ConditionalDebug("Unlock: " + thread + " on Ressource: " + lockRessource);
            var threadVectorInstance = GetThreadVectorInstance(thread);
            _lockHistory.AddLockEntry(lockRessource, new KeyValuePair<ThreadId, ThreadVectorClock>(thread, threadVectorInstance.VectorClock));
            threadVectorInstance.IncrementClock();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleThreadStart(string newThread, string currentThread)
        {
            _logger.ConditionalDebug("NewThread: " + newThread + " started from " + currentThread);
            var currentThreadInstance = GetThreadVectorInstance(currentThread);
            var threadIdClockPair = new KeyValuePair<ThreadId, ThreadVectorClock>(currentThread, currentThreadInstance.VectorClock);
            SynchronizeVectorClock(newThread, threadIdClockPair);
            currentThreadInstance.IncrementClock();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleThreadJoin(string joinedThread, string currentThread)
        {
            _logger.ConditionalDebug("Thread joined: " + joinedThread + " from Thread " + currentThread);
            var joinedThreadVectorInstance = GetThreadVectorInstance(joinedThread);
            var threadIdClockPair = new KeyValuePair<ThreadId, ThreadVectorClock>(joinedThread, joinedThreadVectorInstance.VectorClock); 
            SynchronizeVectorClock(currentThread, threadIdClockPair);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleStartTask(string task, string currentTaskOrThread)
        {
            _logger.ConditionalDebug("New Task started: " + task + " from Thread " + Thread.CurrentThread.ManagedThreadId);
            var currentTaskOrThreadVectorInstance = GetThreadVectorInstance(currentTaskOrThread);
            var threadIdClockPair = new KeyValuePair<ThreadId, ThreadVectorClock>(currentTaskOrThread, currentTaskOrThreadVectorInstance.VectorClock);
            SynchronizeVectorClock(task, threadIdClockPair);
            currentTaskOrThreadVectorInstance.IncrementClock();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleTaskWait(string task, string currentTaskOrThread)
        {
            _logger.ConditionalDebug("Wait for task: " + task + " from Thread " + Thread.CurrentThread.ManagedThreadId);
            var waitedTaskOrThreadVectorInstance = GetThreadVectorInstance(task);
            var threadIdClockPair = new KeyValuePair<ThreadId, ThreadVectorClock>(task, waitedTaskOrThreadVectorInstance.VectorClock);
            SynchronizeVectorClock(currentTaskOrThread, threadIdClockPair);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleTaskRun(string task, string currentTaskOrThread)
        {
            _logger.ConditionalDebug("Run task: " + task + " from Thread " + Thread.CurrentThread.ManagedThreadId);
            var currentTakOrThreadVectorInstance = GetThreadVectorInstance(currentTaskOrThread);
            var threadIdClockPair = new KeyValuePair<ThreadId, ThreadVectorClock>(currentTaskOrThread, currentTakOrThreadVectorInstance.VectorClock);
            SynchronizeVectorClock(task, threadIdClockPair);
            currentTakOrThreadVectorInstance.IncrementClock();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleStartNewTask(string task, string currentTaskOrThread)
        {
            _logger.ConditionalDebug("StartNew task: " + task + " from Thread " + Thread.CurrentThread.ManagedThreadId);
            var currentTakOrThreadVectorInstance = GetThreadVectorInstance(currentTaskOrThread);
            var threadIdClockPair = new KeyValuePair<ThreadId, ThreadVectorClock>(currentTaskOrThread, currentTakOrThreadVectorInstance.VectorClock);
            SynchronizeVectorClock(task, threadIdClockPair);
            currentTakOrThreadVectorInstance.IncrementClock();
        }

        private ThreadVectorInstance GetThreadVectorInstance(string thread)
        {
            ThreadVectorInstance threadVectorInstance;
            if (!_threadVectorPool.TryGetValue(thread, out threadVectorInstance))
            {
                threadVectorInstance = new ThreadVectorInstance(thread);
                _threadVectorPool.Add(thread, threadVectorInstance);
            }
            return threadVectorInstance;
        }

        private bool CheckLockHistory(int lockRessource, out KeyValuePair<ThreadId, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _lockHistory.IsRessourceInLockHistory(lockRessource, out lockThreadIdClockPair);
        }

        private void SynchronizeVectorClock(string ownThread, KeyValuePair<ThreadId, ThreadVectorClock> lockThreadIdClockPair)
        {
            var threadVectorInstance = GetThreadVectorInstance(ownThread);
            var vectorClock = threadVectorInstance.VectorClock;
            vectorClock.Keys.ToList().ForEach(thread =>
            {
                if (thread.Equals(ownThread))
                {
                    vectorClock[thread] += 1;
                }
                else if (thread.Equals(lockThreadIdClockPair.Key.Identifier))
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
            var instances = 
                (_threadVectorPool.Values.Where(instance => instance.ThreadId != threadVectorInstance.ThreadId)).ToList();
            foreach (var instance in instances) {
                foreach (
                    var concurrentEvents in
                        instance.GetConcurrentHistory(threadVectorInstance.VectorClock).Values)
                {
                    foreach (var threadEvent in concurrentEvents)
                    {
                        if (IsRaceCondition(ownThreadEvent, threadEvent))
                        {
                            var info = new LogEventInfo {Level = LogLevel.Error};
                            info.Properties["RowCount"] = rowNumber;
                            info.Properties["MethodName"] = methodName;
                            info.Properties["ConflictMethodName"] = threadEvent.MethodName;
                            info.Properties["ConflictRow"] = threadEvent.Row;
                            info.Message =
                                $"RaceCondition detected... Ressource: {ownThreadEvent.Ressource} -> Thread: {threadVectorInstance.ThreadId} to Thread: {instance.ThreadId}";
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
