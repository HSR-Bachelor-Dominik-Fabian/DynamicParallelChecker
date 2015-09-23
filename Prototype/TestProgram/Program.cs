using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Threading;

namespace TestProgram
{
    class Program
    {
        private static Object a = 2;
        private static Object b = 3;

        static void Main(string[] args)
        {
            Test();
            Task.Factory.StartNew(() =>
            {
                Test2();
            });
        }

        public static void Test()
        {
            Object temp = a;
            Console.WriteLine(a.ToString());
        }

        public static void Test2()
        {
            Object temp = b;
            Console.WriteLine(b.ToString());
        }
    }
}
