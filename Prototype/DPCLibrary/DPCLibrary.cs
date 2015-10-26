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
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Thread: " + currentThreadId + ": Reading object " + obj);
            ThreadVectorManager.HandleReadAccess(currentThreadId, obj);
            Console.ReadLine();
        }

        public static void WriteAccess(int obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Thread: " + currentThreadId + ": Writing object " + obj);
            Console.ReadLine();
        }

        public static void LockObject(int obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Thread: " + currentThreadId + ": Locking object " + obj);
            Console.ReadLine();
        }

        public static void UnLockObject(int obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Thread: " + currentThreadId + ": Unlocking object " + obj);
            Console.ReadLine();
        }

        public static T[] GenericList<T>(List<T> genericList)
        {
            Console.WriteLine("GenericListCall");
            return (T[])genericList.GetType().GetRuntimeFields().First(x => x.Name == "_items").GetValue(genericList);
        }
    }
}
