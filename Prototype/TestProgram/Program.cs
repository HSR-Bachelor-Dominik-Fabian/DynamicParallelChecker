using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TestProgram
{
    class Program
    {
        private static int _a = 2;
        private static readonly object _b = 3;
        private static int[] _k = {11, 12, 13, 14, 15};
        private static long[] _i = {1233123132, 123, 123, 123132123};
        private static NewObject[] _f = {new NewObject(12), new NewObject(14), new NewObject(15)};

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            Test();
            Task.Factory.StartNew(Test2).Wait();
        }

        public static void Test()
        {
            NewObject newObject = new NewObject(123);
            _a = newObject.C;
            int f = _k[1];
            long h = _i[2];
            NewObject hallo = _f[0];
            Console.WriteLine(_a.ToString());
            Console.WriteLine(_k[2].ToString());
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
