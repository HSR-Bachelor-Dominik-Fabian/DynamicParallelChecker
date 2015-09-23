using System;
using System.Threading;

namespace DPCLibrary
{
    public static class DpcLibrary
    {
        public static void ReadAccess(object obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine(currentThreadId + ": Reading object " + obj);
            Console.ReadLine();
        }
    }
}
