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
        private static int a = 2;
        private static int b = 3;

        static void Main(string[] args)
        {
            while (true)
            {
                Test();
                Task.Factory.StartNew(() =>
                {
                    Test2();
                });
            }
        }

        public static void Test()
        {
            int temp = a;
            Console.WriteLine(a);
        }

        public static void Test2()
        {
            int temp = b;
            Console.WriteLine(b);
        }
    }
}
