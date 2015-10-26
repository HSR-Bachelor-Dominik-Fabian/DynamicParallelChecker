using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DPCLibrary.Algorithm.Manager;

namespace DPCLibrary
{
    public static class DpcLibrary
    {
        public static void ReadAccess(int obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            ThreadVectorManager.GetInstance().HandleReadAccess(currentThreadId, obj);
        }

        public static void WriteAccess(int obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            ThreadVectorManager.GetInstance().HandleWriteAccess(currentThreadId, obj);
        }

        public static void LockObject(int obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            ThreadVectorManager.GetInstance().HandleLock(currentThreadId, obj);
        }

        public static void UnLockObject(int obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            ThreadVectorManager.GetInstance().HandleUnLock(currentThreadId, obj);
        }

        public static T[] GenericList<T>(List<T> genericList)
        {
            return (T[])genericList.GetType().GetRuntimeFields().First(x => x.Name == "_items").GetValue(genericList);
        }
    }
}
