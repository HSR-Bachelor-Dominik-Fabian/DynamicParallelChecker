using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DPCLibrary
{
    public static class DPCLibrary
    {
        public static void readAccess(Object obj)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine(currentThreadId + ": Reading object " + obj.ToString());
        }
    }
}
