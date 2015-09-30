using System;
using System.Threading.Tasks;

namespace TestProgram
{
    class Program
    {
        private static int _a = 2;
        private static object _b = 3;
        private static readonly int[] _k = { 11, 12, 13, 14, 15 };
        private static readonly long[] _i = { 123313131234232L, 123313131234232L, 123313131234232L, 123313131234232L};
        private static readonly NewObject[] _f = { new NewObject(12), new NewObject(14), new NewObject(15) };

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            Console.WriteLine("Main()");
            //Test();
            Task.Factory.StartNew(Test2).Wait();
            Test3();
        }

        //public static void Test()
        //{
        //    Console.WriteLine("Test()");
        //    NewObject newObject = new NewObject(123);
        //    Console.WriteLine("nach new NewObject()");
        //    _a = newObject.C;
        //    int f = _k[1];
        //    long h = _i[2];
        //    lock (_b)
        //    {
        //        _b = 4;
        //    }

        //    f += 12;
        //    h += 22;
        //    _f[0].C = 12231; //Uncomment to analyse Error
        //    //TODO: Dominik: Fehler analysieren

        //    Console.WriteLine("Vor f");
        //    _k[1] = f;
        //    Console.WriteLine("Vor h");
        //    _i[2] = h;

        //    Console.WriteLine(_a.ToString());
        //    Console.WriteLine(_k[2].ToString());
        //    Console.ReadLine();
        //}

        public static void Test2()
        {
            Console.WriteLine(_b.ToString());
            Console.ReadLine();
        }

        public static void Test3()
        {
            int[] intArray = new int[5];
            long[] longArray = new long[5];
            float[] floatArray = new float[5];
            double[] doubleArray = new double[5];
            NewObject[] newObjectArray = new NewObject[5];

            intArray[0] = 5;
            longArray[0] = 4294967296L;
            floatArray[0] = 3.5F;
            doubleArray[0] = 3D;
            newObjectArray[0] = new NewObject(6);
            Console.ReadLine();

            int tempInt = intArray[0];
            long tempLong = longArray[0];
            float tempFloat = floatArray[0];
            double tempDouble = doubleArray[0];
            NewObject tempObject = newObjectArray[0];
        }
    }
}
