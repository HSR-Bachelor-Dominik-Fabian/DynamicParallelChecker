using System;
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
            Thread thread = Thread.CurrentThread;
            ThreadVectorManager.GetInstance().HandleReadAccess(thread, obj);
        }

        public static void WriteAccess(int obj)
        {
            Thread thread = Thread.CurrentThread;
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, obj);
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

        public static T[] GenericList<T>(List<T> genericList)
        {
            return (T[])genericList.GetType().GetRuntimeFields().First(x => x.Name == "_items").GetValue(genericList);
        }
    }
}
