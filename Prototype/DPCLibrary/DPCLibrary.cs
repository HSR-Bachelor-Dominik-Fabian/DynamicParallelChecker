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
            task.Wait();
            TaskWaitAll(task);
        }

        public static bool TaskWaitTimespan(Task task, TimeSpan timespan)
        {
            bool result = task.Wait(timespan);
            TaskWaitAll(task);
            return result;
        }

        public static bool TaskWaitTimeout(Task task, int millisecondsTimeout)
        {
            bool result = task.Wait(millisecondsTimeout);
            TaskWaitAll(task);
            return result;

        }

        public static void TaskWaitCancelToken(Task task, CancellationToken cancellationToken)
        {
            task.Wait(cancellationToken);
            TaskWaitAll(task);
        }

        public static bool TaskWaitTimeOutCancelToken(Task task, int millisecondsTimeout, 
            CancellationToken cancellationToken)
        {
            bool result = task.Wait(millisecondsTimeout, cancellationToken);
            TaskWaitAll(task);
            return result;
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

        public static Task RunTask(Action action)
        {
            Task result = new Task(action);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task RunTaskCancel(Action action, CancellationToken cancellationToken)
        {
            Task result = new Task(action, cancellationToken);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task RunTaskFunc(Func<Task> func)
        {
            Task result = new Task<Task>(func);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task RunTaskFuncCancel(Func<Task> func, CancellationToken cancellationToken)
        {
            Task<Task> result = new Task<Task>(func, cancellationToken, TaskCreationOptions.DenyChildAttach);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task<TResult> RunTaskTResult<TResult>(Func<TResult> function)
        {
            Task<TResult> result = new Task<TResult>(function);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task<TResult> RunTaskTResultCancel<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            Task<TResult> result = new Task<TResult>(function, cancellationToken);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task<TResult> RunTaskTaskTResult<TResult>(Func<Task<TResult>> function)
        {
            Task<Task<TResult>> result = new Task<Task<TResult>>(function);
            RunTaskAll(result);
            result.Start();
            return result.Result;
        }

        public static Task<TResult> RunTaskTaskTResultCancel<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            Task<Task<TResult>> result = new Task<Task<TResult>>(function, cancellationToken);
            RunTaskAll(result);
            result.Start();
            return result.Result;
        }

        private static void RunTaskAll(Task task)
        {
            string currentThreadId;
            if (Task.CurrentId.HasValue)
            {
                _logger.ConditionalTrace(
                    $"Run Task: {task.Id} in Task: {Task.CurrentId} on WorkerThread {Thread.CurrentThread.ManagedThreadId}");
                currentThreadId = $"Task_{Task.CurrentId}";
            }
            else
            {
                _logger.ConditionalTrace($"Run Task: {task.Id} on Thread {Thread.CurrentThread.ManagedThreadId}");
                currentThreadId = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            }

            ThreadVectorManager.GetInstance().HandleTaskRun($"Task_{task.Id}", currentThreadId);
        }
        
        public static void RaceConditionDetectedIdentifier()
        {
            
        }
    }
}
