using System;
using System.Threading.Tasks;

namespace TestProgram
{
    class Program
    {
        private static object a = 2;
        private static readonly object b = 3;

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            Test();
            Task.Factory.StartNew(Test2);
        }

        public static void Test()
        {
            a = 123;
            Console.WriteLine(a.ToString());
        }

        public static void Test2()
        {
            //TODO:Fabian: Luag das aa :)
            Console.WriteLine(b.ToString());
        }
    }
}
