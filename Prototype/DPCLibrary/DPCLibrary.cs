using System.Threading;
using DPCLibrary.Algorithm.Manager;

namespace DPCLibrary
{
    public static class DpcLibrary
    {
        public static void ReadAccess(int obj, int rowNumber)
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorManager.GetInstance().HandleReadAccess(thread, obj, rowNumber);
        }

        public static void WriteAccess(int obj, int rowNumber)
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, obj, rowNumber);
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
    }
}
