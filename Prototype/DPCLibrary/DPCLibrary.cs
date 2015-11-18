using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DPCLibrary.Algorithm.Manager;
using NLog;

namespace DPCLibrary
{
    public static class DpcLibrary
    {
        private static readonly Logger _logger = LogManager.GetLogger("DPCLibrary");
        public static void ReadAccess(int obj, int rowNumber, string methodName)
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorManager.GetInstance().HandleReadAccess(thread, obj, rowNumber, methodName);
        }

        public static void WriteAccess(int obj, int rowNumber, string methodName)
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, obj, rowNumber, methodName);
        }

        public static void LockObject(int obj)
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorManager.GetInstance().HandleLock(thread, obj);
        }

        public static void UnLockObject(int obj)
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorManager.GetInstance().HandleUnLock(thread, obj);
        }

        public static void StartThread(Thread thread, object parameter = null)
        {
            _logger.ConditionalTrace("New Thread started: " + thread.ManagedThreadId + " from Thread " + Thread.CurrentThread.ManagedThreadId);
            ThreadVectorManager.GetInstance().HandleThreadStart(thread, Thread.CurrentThread);
            if (parameter != null)
            {
                thread.Start(parameter);
            }
            else
            {
                thread.Start();
            }
        }

        public static void JoinThread(Thread thread)
        {
            JoinThreadAll(thread);
            thread.Join();
        }
        
        public static bool JoinThreadMilliseconds(Thread thread, int millisecondsTimeout = 0)
        {
            JoinThreadAll(thread);
            return thread.Join(millisecondsTimeout);
        }

        public static bool JoinThreadTimeout(Thread thread, TimeSpan timeout)
        {
            JoinThreadAll(thread);
            return thread.Join(timeout);
        }

        private static void JoinThreadAll(Thread thread)
        {
            _logger.ConditionalTrace("Thread joined: " + thread.ManagedThreadId + " from Thread " + Thread.CurrentThread.ManagedThreadId);
            ThreadVectorManager.GetInstance().HandleThreadJoin(thread, Thread.CurrentThread);
        }

        public static void StartTask(Task task, TaskScheduler scheduler = null)
        {
            _logger.ConditionalTrace("New Task started: " + task.Id + " from Thread " + Thread.CurrentThread.ManagedThreadId);
            // TODO:Fabian ThreadVectorManager.GetInstance().HandleStartTask(task);
            if (scheduler != null)
            {
                task.Start(scheduler);
            }
            else
            {
                task.Start();
            }
        }

        public static void StartNew()
        {
            
        }

        public static void RaceConditionDetectedIdentifier()
        {
            
        }
    }
}
