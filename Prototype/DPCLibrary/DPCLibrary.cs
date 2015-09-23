using System;
using System.Threading;

namespace DPCLibrary
{
    internal static class DpcLibrary
    {
        public static void ReadAccess(object obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine(currentThreadId + ": Reading object " + obj);
            Console.ReadLine();
        }
    }
}
