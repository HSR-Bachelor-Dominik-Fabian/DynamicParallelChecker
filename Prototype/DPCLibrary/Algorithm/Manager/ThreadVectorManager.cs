using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly Dictionary<string, ThreadVectorInstance> _threadVectorPool
            = new Dictionary<string, ThreadVectorInstance>();

        private readonly LockHistory _lockHistory = new LockHistory();

        private readonly Logger _logger = LogManager.GetLogger("ThreadVectorManager");

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleReadAccess(string thread, int ressource, int rowNumber, string methodName)
        {
            _logger.ConditionalDebug("ReadAccess: " + thread + " on Ressource: " + ressource + "on Line:" + rowNumber);
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(thread);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Read, ressource, rowNumber, methodName);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance, rowNumber, methodName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleWriteAccess(string thread, int ressource, int rowNumber, string methodName)
        {
            _logger.ConditionalDebug("WriteAccess: " + thread + " on Ressource: " + ressource + "on Line:" + rowNumber);
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(thread);
            ThreadEvent threadEvent = new ThreadEvent(ThreadEvent.EventType.Write, ressource,rowNumber, methodName);
            threadVectorInstance.WriteHistory(threadEvent);
            CheckForRaceCondition(threadEvent, threadVectorInstance, rowNumber, methodName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleLock(string thread, int lockRessource)
        {
            _logger.ConditionalDebug("Lock: " + thread + " on Ressource: " + lockRessource);
            KeyValuePair<string, ThreadVectorClock> lockThreadIdClockPair;
            if (CheckLockHistory(lockRessource, out lockThreadIdClockPair))
            {
                SynchronizeVectorClock(thread, lockThreadIdClockPair);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleUnLock(string thread, int lockRessource)
        {
            _logger.ConditionalDebug("Unlock: " + thread + " on Ressource: " + lockRessource);
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(thread);
            _lockHistory.AddLockEntry(lockRessource, new KeyValuePair<string, ThreadVectorClock>(thread, threadVectorInstance.VectorClock));
            threadVectorInstance.IncrementClock();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleThreadStart(string newThread, string currentThread)
        {
            _logger.ConditionalDebug("NewThread: " + newThread + " started from " + currentThread);
            ThreadVectorInstance currentThreadInstance = GetThreadVectorInstance(currentThread);
            KeyValuePair<string, ThreadVectorClock> threadIdClockPair = new KeyValuePair<string, ThreadVectorClock>(currentThread, currentThreadInstance.VectorClock);
            SynchronizeVectorClock(newThread, threadIdClockPair);
            currentThreadInstance.IncrementClock();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleThreadJoin(string joinedThread, string currentThread)
        {
            _logger.ConditionalDebug("Thread joined: " + joinedThread + " from Thread " + currentThread);
            ThreadVectorInstance joinedThreadVectorInstance = GetThreadVectorInstance(joinedThread);
            KeyValuePair<string, ThreadVectorClock> threadIdClockPair = new KeyValuePair<string, ThreadVectorClock>(joinedThread, joinedThreadVectorInstance.VectorClock); 
            SynchronizeVectorClock(currentThread, threadIdClockPair);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void HandleStartTask(string task, string currentTask)
        {
            _logger.ConditionalDebug("New Task started: " + task + " from Thread " + Thread.CurrentThread.ManagedThreadId);
            //ThreadVectorInstance currentTaskVectorInstance = GetThreadVectorInstance(currentTask);
            //KeyValuePair<String, ThreadVectorInstance> threadIdClockPair = new KeyValuePair<string, ThreadVectorInstance>("", currentTaskVectorInstance);
            //SynchronizeVectorClock(task, threadIdClockPair);
            //currentTaskVectorInstance.IncrementClock();
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

        private bool CheckLockHistory(int lockRessource, out KeyValuePair<string, ThreadVectorClock> lockThreadIdClockPair)
        {
            return _lockHistory.IsRessourceInLockHistory(lockRessource, out lockThreadIdClockPair);
        }

        private void SynchronizeVectorClock(string ownThread, KeyValuePair<string, ThreadVectorClock> lockThreadIdClockPair)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(ownThread);
            ThreadVectorClock vectorClock = threadVectorInstance.VectorClock;
            vectorClock.Keys.ToList().ForEach(thread =>
            {
                if (thread.Equals(ownThread))
                {
                    vectorClock[thread] += 1;
                }
                else if (thread.Equals(lockThreadIdClockPair.Key))
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
                            LogEventInfo info = new LogEventInfo {Level = LogLevel.Error};
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
