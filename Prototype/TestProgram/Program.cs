using System;
using System.Threading.Tasks;

namespace TestProgram
{
    internal class Program
    {
        //private static object _a = 2;
        //private static object _b = 3;
        //private static readonly int[] _k = { 11, 12, 13, 14, 15 };
        //private static readonly long[] _i = { 123313131234232L, 123313131234232L, 123313131234232L, 123313131234232L };
        //private static readonly NewObject[] _f = { new NewObject(12), new NewObject(14), new NewObject(15) };
        private static readonly object _c = new object();
        private static readonly bool _bool = true;
        private static readonly char _char = 'a';
        private static readonly sbyte _sbyte = 2;
        private static readonly Int16 _int16 = 23;
        private static readonly Int32 _int32 = 2342;
        private static readonly Int64 _int64 = 23429824398247198;
        private static readonly IntPtr _intPtr = new IntPtr(0);
        private static readonly Byte _byte = 2;
        private static readonly UInt16 _uint16 = 23;
        private static readonly UInt32 _uint32 = 2342;
        private static readonly UInt64 _uint64 = 23984329479832;
        private static readonly UIntPtr _uIntPtr = new UIntPtr(4);
        private static readonly Single _single = 3F;
        private static readonly Double _double = 3.2;
        private static TestStruct _testStruct = new TestStruct();
        private static readonly string _string = "Test";


        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            Console.WriteLine("Main()");
            //Test();
            //Task.Factory.StartNew(Test2).Wait();
            //Test3();
            //Test4();
            //Test5();
            //Test6();
            //Test7();
            //Test8();
            Test9();
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
        //    _f[0].C = 12231; //Uncomment to analyse Errors

        //    Console.WriteLine("Vor f");
        //    _k[1] = f;
        //    Console.WriteLine("Vor h");
        //    _i[2] = h;

        //    Console.WriteLine(_a.ToString());
        //    Console.WriteLine(_k[2].ToString());
        //    Console.ReadLine();
        //}

        //public static void Test2()
        //{
        //    Console.WriteLine(_b.ToString());
        //    Console.ReadLine();
        //}

        //public static void Test3()
        //{
        //    int[] intArray = new int[5];
        //    long[] longArray = new long[5];
        //    float[] floatArray = new float[5];
        //    double[] doubleArray = new double[5];
        //    NewObject[] newObjectArray = new NewObject[5];

        //    intArray[0] = 5;
        //    longArray[0] = 4294967296L;
        //    floatArray[0] = 3.5F;
        //    doubleArray[0] = 3D;
        //    newObjectArray[0] = new NewObject(6);
        //    Console.ReadLine();

        //    int tempInt = intArray[0];
        //    long tempLong = longArray[0];
        //    float tempFloat = floatArray[0];
        //    double tempDouble = doubleArray[0];
        //    NewObject tempObject = newObjectArray[0];
        //}

        //public static void Test4()
        //{
        //    Console.WriteLine("Test4()");
        //    short[][] shortArray = { new short[] { 1, 3 }, new short[] { 1, 3 } };
        //    int[][] intArray = { new[] { 2, 123213 }, new[] { 1232, 423 }, new[] { 421452, 1233 }, new[] { 2123, 3432 } };
        //    var objectArray = new[] { new[] { new[] { new NewObject(122), new NewObject(3322) }, new[] { new NewObject(122), new NewObject(3322) }, new[] { new NewObject(122), new NewObject(3322) } }, new[] { new[] { new NewObject(122), new NewObject(3322) }, new[] { new NewObject(122), new NewObject(3322) }, new[] { new NewObject(122), new NewObject(3322) } } };
        //    TestStruct[][] structArray = new[] { new[] { new TestStruct(), new TestStruct() }, new[] { new TestStruct(), new TestStruct() } };
        //    short a = shortArray[0][1];
        //    a += 2;
        //    shortArray[1][0] = a;

        //    int b = intArray[2][1];
        //    b += 213;
        //    intArray[1][1] = b;

        //    NewObject obj = objectArray[1][0][1];

        //    TestStruct struct1 = structArray[0][1];
        //    struct1.GetA();
        //    struct1.GetB();
        //}

        //public static void Test5()
        //{
        //    Console.WriteLine("Test5()");
        //    List<int> listArray = new List<int> { 1, 23, 54, 1231, 213 };
        //    List<NewObject> objectArray = new List<NewObject> { new NewObject(332), new NewObject(13331), new NewObject(3323241) };

        //    int a = listArray[3];
        //    NewObject obj = objectArray[2];
        //    a += 231;
        //    obj.C = 13333333;
        //    listArray[0] = a;
        //    objectArray[2] = obj;
        //}

        //public static void Test6()
        //{
        //    Console.WriteLine("Test6()");
        //    TestStruct[] testStructArray = new TestStruct[2];
        //    TestStruct testStruct = new TestStruct();
        //    testStructArray[0] = testStruct;
        //    //int a = testStructArray[0].GetA();
        //    //int b = testStructArray[0].GetB();

        //}

        //public static void Test7()
        //{
        //    Console.WriteLine("Before lock() :" + _c);
        //    TestStruct test = new TestStruct(new object(), new object());
        //    Console.WriteLine("Test: " + test.GetA());
        //    lock (_c)
        //    {
        //        Console.WriteLine("in lock() :" + _c);
        //    }
        //    Console.WriteLine("after lock() :" + _c);
        //}

        //public static void Test8()
        //{
        //    TestStruct test = new TestStruct(new object(), new object());
        //    test.GetA();
        //    test.GetB();
        //}

        public static void Test9()
        {
            Console.WriteLine("Test9");
            Console.WriteLine("IsPrimitive? true");
            bool temp1 = _bool;
            char temp2 = _char;
            sbyte temp3 = _sbyte;
            Int16 temp4 = _int16;
            Int32 temp5 = _int32;
            Int64 temp6 = _int64;
            IntPtr temp7 = _intPtr;
            Byte temp8 = _byte;
            UInt16 temp9 = _uint16;
            UInt32 temp10 = _uint32;
            UInt64 temp11 = _uint64;
            UIntPtr temp12 = _uIntPtr;
            Single temp13 = _single;
            Double temp14 = _double;
            string temp17 = _string;

            Console.WriteLine("IsClass? true");
            object temp15 = _c;

            Console.WriteLine("IsValueType? true");
            _testStruct.GetA();
            _testStruct.GetB();
            //TestStruct temp16 = _testStruct;
            _testStruct = new TestStruct();
            //object a = temp16.GetA();
            //object b = temp16.GetB();
        }

        public static void Test10()
        {
            int[] temp = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14};
            Parallel.ForEach(temp, i =>
            {
                i += 2;
            });
        }
    }

    struct TestStruct
    {
        private object a;
        private object b;

        public object GetA()
        {
            a = 2;
            return a;
        }

        public object GetB()
        {
            b = 3;
            return b;
        }
    }
}
