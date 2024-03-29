﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DPCLibrary.Algorithm;
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
            
            ThreadVectorFacade.GetInstance().HandleReadAccess(threadId, obj, rowNumber, methodName);
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
            ThreadVectorFacade.GetInstance().HandleWriteAccess(threadId, obj, rowNumber, methodName);
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
            ThreadVectorFacade.GetInstance().HandleLock(threadId, obj);
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
            ThreadVectorFacade.GetInstance().HandleUnLock(threadId, obj);
        }

        public static void StartThread(Thread thread, object parameter = null)
        {
            _logger.ConditionalTrace(
                $"New threadId started: {thread.ManagedThreadId} from threadId {Thread.CurrentThread.ManagedThreadId}");
            ThreadVectorFacade.GetInstance().HandleThreadStart($"Thread_{thread.ManagedThreadId}", $"Thread_{Thread.CurrentThread.ManagedThreadId}");
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
            
            ThreadVectorFacade.GetInstance().HandleThreadJoin($"Thread_{thread.ManagedThreadId}", threadId);
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

            ThreadVectorFacade.GetInstance().HandleStartTask($"Task_{task.Id}", currentThreadId);
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
            var result = task.Wait(timespan);
            TaskWaitAll(task);
            return result;
        }

        public static bool TaskWaitTimeout(Task task, int millisecondsTimeout)
        {
            var result = task.Wait(millisecondsTimeout);
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
            var result = task.Wait(millisecondsTimeout, cancellationToken);
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
            ThreadVectorFacade.GetInstance().HandleTaskWait($"Task_{task.Id}", currentThreadId);
        }

        public static Task RunTask(Action action)
        {
            var result = new Task(action);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task RunTaskCancel(Action action, CancellationToken cancellationToken)
        {
            var result = new Task(action, cancellationToken);
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
            var result = new Task<Task>(func, cancellationToken, TaskCreationOptions.DenyChildAttach);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task<TResult> RunTaskTResult<TResult>(Func<TResult> function)
        {
            var result = new Task<TResult>(function);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task<TResult> RunTaskTResultCancel<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            var result = new Task<TResult>(function, cancellationToken);
            RunTaskAll(result);
            result.Start();
            return result;
        }

        public static Task<TResult> RunTaskTaskTResult<TResult>(Func<Task<TResult>> function)
        {
            var result = new Task<Task<TResult>>(function);
            RunTaskAll(result);
            result.Start();
            return result.Result;
        }

        public static Task<TResult> RunTaskTaskTResultCancel<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            var result = new Task<Task<TResult>>(function, cancellationToken);
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

            ThreadVectorFacade.GetInstance().HandleTaskRun($"Task_{task.Id}", currentThreadId);
        }

        public static Task StartNew(Action action, TaskCreationOptions taskCreationOptions )
        {
            var result = taskCreationOptions.Equals(TaskCreationOptions.None)
                ? new Task(action) : new Task(action, taskCreationOptions);
            StartNewTaskAll(result);
            result.Start();
            return result;
        }

        public static Task StartNewCancel(Action action, CancellationToken cancellationToken, 
            TaskCreationOptions taskCreationOptions, TaskScheduler taskScheduler)
        {
            var result = taskCreationOptions.Equals(TaskCreationOptions.None) 
                ? new Task(action, cancellationToken) : new Task(action, cancellationToken, taskCreationOptions);
            StartNewTaskAll(result);
            if (taskScheduler != null)
                result.Start(taskScheduler);
            else
                result.Start();
            return result;
        }

        public static Task StartNewObject(Action<object> action, object state, TaskCreationOptions creationOptions)
        {
            var result = creationOptions.Equals(TaskCreationOptions.None)
                ? new Task(action, TaskCreationOptions.None) : new Task(action, creationOptions);
            StartNewTaskAll(result);
            result.Start();
            return result;
        }

        public static Task StartNewObjectCancel(Action<object> action, object state, CancellationToken cancellationToken, 
            TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            var result = creationOptions.Equals(TaskCreationOptions.None)
                ? new Task(action, state, cancellationToken) : new Task(action, state, cancellationToken, creationOptions);
            StartNewTaskAll(result);
            if (scheduler != null)
                result.Start(scheduler);
            else
                result.Start();
            return result;
        }

        public static Task<TResult> StartNewTResult<TResult>(Func<TResult> function, TaskCreationOptions creationOptions)
        {
            var result = creationOptions.Equals(TaskCreationOptions.None)
                ? new Task<TResult>(function, TaskCreationOptions.None) : new Task<TResult>(function, creationOptions);
            StartNewTaskAll(result);
            result.Start();
            return result;
        }

        public static Task<TResult> StartNewTResultCancel<TResult>(Func<TResult> function, CancellationToken cancellationToken, 
            TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            var result = creationOptions.Equals(TaskCreationOptions.None)
                ? new Task<TResult>(function, cancellationToken) : new Task<TResult>(function, cancellationToken, creationOptions);
            StartNewTaskAll(result);
            if (scheduler != null)
                result.Start(scheduler);
            else
                result.Start();
            return result;
        }

        public static Task<TResult> StartNewObjectTResult<TResult>(Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            var result = creationOptions.Equals(TaskCreationOptions.None)
                ? new Task<TResult>(function, state, TaskCreationOptions.None) : new Task<TResult>(function, state, creationOptions);
            StartNewTaskAll(result);
            result.Start();
            return result;
        }

        public static Task<TResult> StartNewObjectTResultCancel<TResult>(Func<object, TResult> function, object state, CancellationToken cancellationToken,
            TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            var result = creationOptions.Equals(TaskCreationOptions.None)
                ? new Task<TResult>(function, state, cancellationToken) : new Task<TResult>(function, state, cancellationToken, creationOptions);
            StartNewTaskAll(result);
            if (scheduler != null)
                result.Start(scheduler);
            else
                result.Start();
            return result;
        }

        private static void StartNewTaskAll(Task task)
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

            ThreadVectorFacade.GetInstance().HandleStartNewTask($"Task_{task.Id}", currentThreadId);
        }

        public static void RaceConditionDetectedIdentifier()
        {
            
        }
    }
}
