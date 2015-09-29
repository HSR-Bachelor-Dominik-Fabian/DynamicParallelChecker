using System;
using System.Threading.Tasks;

namespace TestProgram
{
    class Program
    {
        private static int _a = 2;
        private static readonly object _b = 3;
        private static readonly int[] _k = {11, 12, 13, 14, 15};
        private static readonly long[] _i = {1233123132, 123, 123, 123132123};
        private static readonly NewObject[] _f = {new NewObject(12), new NewObject(14), new NewObject(15)};

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

            f += 12;
            h += 22;
            _f[0].C = 12231; //Uncomment to analyse Error
        //TODO: Dominik: Fehler analysieren

            //Console.WriteLine("Vor f");
            _k[1] = f;
            //Console.WriteLine("Vor h");
            //_i[2] = h;

            //Console.WriteLine(_a.ToString());
            //Console.WriteLine(_k[2].ToString());
            //Console.ReadLine();
        }

        public static void Test2()
        {
            Console.WriteLine(_b.ToString());
            Console.ReadLine();
        }
    }
}
