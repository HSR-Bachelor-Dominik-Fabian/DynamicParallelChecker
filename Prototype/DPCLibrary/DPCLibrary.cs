using System;
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
            string threadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"ReadAccess in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                threadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                threadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }
            
            ThreadVectorManager.GetInstance().HandleReadAccess(threadId, obj, rowNumber, methodName);
        }

        public static void WriteAccess(int obj, int rowNumber, string methodName)
        {
            string threadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"WriteAccess in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                threadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                threadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }
            ThreadVectorManager.GetInstance().HandleWriteAccess(threadId, obj, rowNumber, methodName);
        }

        public static void LockObject(int obj)
        {
            string threadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"LockObject in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                threadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                threadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }
            ThreadVectorManager.GetInstance().HandleLock(threadId, obj);
        }

        public static void UnLockObject(int obj)
        {
            string threadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"UnLockObject in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                threadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                threadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }
            ThreadVectorManager.GetInstance().HandleUnLock(threadId, obj);
        }

        public static void StartThread(Thread thread, object parameter = null)
        {
            _logger.ConditionalTrace(
                $"New threadId started: {thread.ManagedThreadId} from threadId {Thread.CurrentThread.ManagedThreadId}");
            ThreadVectorManager.GetInstance().HandleThreadStart($"Thread_{thread.ManagedThreadId}", $"Thread_{Thread.CurrentThread.ManagedThreadId}");
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
            string threadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"JoinThread in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                threadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                _logger.ConditionalTrace(
                    $"Thread joined: {thread.ManagedThreadId} on Thread {Thread.CurrentThread.ManagedThreadId}");
                threadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }
            
            ThreadVectorManager.GetInstance().HandleThreadJoin($"Thread_{thread.ManagedThreadId}", threadId);
        }

        public static void StartTask(Task task, TaskScheduler scheduler = null)
        {
            string currentThreadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"New Task started: {task.Id} in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                currentThreadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                _logger.ConditionalTrace($"New Task started: {task.Id} on Thread {Thread.CurrentThread.ManagedThreadId}");
                currentThreadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }

            ThreadVectorManager.GetInstance().HandleStartTask($"Task_{task.Id}", currentThreadId);
            if (scheduler != null)
            {
                task.Start(scheduler);
            }
            else
            {
                task.Start();
            }
        }

        public static void TaskWait(Task task)
        {
            TaskWaitAll(task);
            task.Wait();
        }

        public static bool TaskWaitTimespan(Task task, TimeSpan timespan)
        {
            TaskWaitAll(task);
            return task.Wait(timespan);
        }

        public static bool TaskWaitTimeout(Task task, int millisecondsTimeout)
        {
            TaskWaitAll(task);
            return task.Wait(millisecondsTimeout);
        }

        public static void TaskWaitCancelToken(Task task, CancellationToken cancellationToken)
        {
            TaskWaitAll(task);
            task.Wait(cancellationToken);
        }

        public static bool TaskWaitTimeOutCancelToken(Task task, int millisecondsTimeout, 
            CancellationToken cancellationToken)
        {
            TaskWaitAll(task);
            return task.Wait(millisecondsTimeout, cancellationToken);
        }

        private static void TaskWaitAll(Task task)
        {
            string currentThreadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"Wait for Task: {task.Id} in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                currentThreadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                _logger.ConditionalTrace($"Wait for Task: {task.Id} on Thread {Thread.CurrentThread.ManagedThreadId}");
                currentThreadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }
            ThreadVectorManager.GetInstance().HandleTaskWait($"Task_{task.Id}", currentThreadId);
        }

        public static Task RunTask(Action action, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task result;
            if (cancellationToken.Equals(default(CancellationToken)))
            {
                result = Task.Run(action, cancellationToken);
            }
            else
            {
                result = Task.Run(action);
            }
            RunTaskAll(result);
            return result;
        }
        /*
        public static Task RunTaskCancelToken(Action action, )
        {
            Task result = Task.Run(action);
            RunTaskAll(result);
            return result;
        }
        */
        private static void RunTaskAll(Task task)
        {
            string currentThreadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"Wait for Task: {task.Id} in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                currentThreadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                _logger.ConditionalTrace($"Wait for Task: {task.Id} on Thread {Thread.CurrentThread.ManagedThreadId}");
                currentThreadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }

            ThreadVectorManager.GetInstance().HandleTaskRun($"Task_{task.Id}", currentThreadId);
        }
        
        public static void RaceConditionDetectedIdentifier()
        {
            
        }
    }
}
