using System;
using System.Threading.Tasks;

namespace TestProgram
{
    class Program
    {
        private static int _a = 2;
        private static readonly object _b = 3;

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            Test();
            Task.Factory.StartNew(Test2).Wait();
        }

        public static void Test()
        {
            _a = 123;
            Console.WriteLine(_a.ToString());
            Console.ReadLine();
        }

        public static void Test2()
        {
            //TODO:Fabian: Luag das aa :)
            Console.WriteLine(_b.ToString());
            Console.ReadLine();
        }
    }
}
