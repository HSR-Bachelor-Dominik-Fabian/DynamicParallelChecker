using System;
using System.Threading;

namespace DPCLibrary
{
    public static class DpcLibrary
    {
        public static void ReadAccess(int obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Thread: " + currentThreadId + ": Reading object " + obj);
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
    }
}
