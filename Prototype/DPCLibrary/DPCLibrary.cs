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
            _logger.ConditionalTrace("New threadId started: " + thread.ManagedThreadId + " from threadId " + Thread.CurrentThread.ManagedThreadId);
            ThreadVectorManager.GetInstance().HandleThreadStart($"Thread_{thread.ManagedThreadId}", $"Thread_{Thread.CurrentThread}");
            if (parameter != null)
            {
                thread.Start(parameter);
            }
            else
            {
                thread.Start();
            }
            
        }

        public static void RaceConditionDetectedIdentifier()
        {
            
        }
    }
}
