using System.Threading;
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

        public static void RaceConditionDetectedIdentifier()
        {
            
        }
    }
}
