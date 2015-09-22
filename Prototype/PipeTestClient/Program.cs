using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Threading;

namespace PipeTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                int a = 0;
                a += 2;
                Task.Factory.StartNew(() => {
                    Test2();
                });
                Thread.Sleep(1000);
                Console.WriteLine("DONE");

                Test();
            }
            
        }

        public static void Test()
        {
            Console.WriteLine(123);
        }

        public static void Test2()
        {
            Console.WriteLine(323332);
        }
    }
}
