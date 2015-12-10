using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable All

namespace TestProgram
{
    internal class Program
    {
        private static object _a = 2;
        private static object _b = 3;
        private static readonly int[] _k = { 11, 12, 13, 14, 15 };
        private static readonly long[] _i = { 123313131234232L, 123313131234232L, 123313131234232L, 123313131234232L };
        private static readonly NewObject[] _f = { new NewObject(12), new NewObject(14), new NewObject(15) };
        private static readonly object _c = new object();
        private static readonly bool _bool = true;
        private static readonly char _char = 'a';
        private static readonly sbyte _sbyte = 2;
        private static readonly short _int16 = 23;
        private static readonly int _int32 = 2342;
        private static readonly long _int64 = 23429824398247198;
        private static readonly IntPtr _intPtr = new IntPtr(0);
        private static readonly byte _byte = 2;
        private static readonly ushort _uint16 = 23;
        private static readonly uint _uint32 = 2342;
        private static readonly ulong _uint64 = 23984329479832;
        private static readonly UIntPtr _uIntPtr = new UIntPtr(4);
        private static readonly float _single = 3F;
        private static readonly double _double = 3.2;
        private static TestStruct _testStruct = new TestStruct();
        private static readonly string _string = "Test";
        private static object _arace = 1;
        private static object _brace = 1;


        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            Test();
            Test3();
            Test4();
            Test5();
            Test6();
            Test7();
            Test9();
            Test10();
            Test11();
            Test12();
            Test13();
            Console.ReadKey();
        }

        public static void Test()
        {
            Console.WriteLine("Test()");
            var newObject = new NewObject(123);
            _a = newObject.C;
            var f = _k[1];
            var h = _i[2];
            lock (_b)
            {
                _b = 4;
            }

            f += 12;
            h += 22;
            _f[0].C = 12231; //Uncomment to analyse Errors

            _k[1] = f;
            _i[2] = h;
        }

        public static void Test3()
        {
            Console.WriteLine("Test3()");
            var intArray = new int[5];
            var longArray = new long[5];
            var floatArray = new float[5];
            var doubleArray = new double[5];
            var newObjectArray = new NewObject[5];

            intArray[0] = 5;
            longArray[0] = 4294967296L;
            floatArray[0] = 3.5F;
            doubleArray[0] = 3D;
            newObjectArray[0] = new NewObject(6);

            var tempInt = intArray[0];
            var tempLong = longArray[0];
            var tempFloat = floatArray[0];
            var tempDouble = doubleArray[0];
            var tempObject = newObjectArray[0];
        }

        public static void Test4()
        {
            Console.WriteLine("Test4()");
            short[][] shortArray = { new short[] { 1, 3 }, new short[] { 1, 3 } };
            int[][] intArray = { new[] { 2, 123213 }, new[] { 1232, 423 }, new[] { 421452, 1233 }, new[] { 2123, 3432 } };
            var objectArray = new[] { new[] { new[] { new NewObject(122), new NewObject(3322) }, new[] { new NewObject(122), new NewObject(3322) }, new[] { new NewObject(122), new NewObject(3322) } }, new[] { new[] { new NewObject(122), new NewObject(3322) }, new[] { new NewObject(122), new NewObject(3322) }, new[] { new NewObject(122), new NewObject(3322) } } };
            var structArray = new[] { new[] { new TestStruct(), new TestStruct() }, new[] { new TestStruct(), new TestStruct() } };
            var a = shortArray[0][1];
            a += 2;
            shortArray[1][0] = a;

            var b = intArray[2][1];
            b += 213;
            intArray[1][1] = b;

            var obj = objectArray[1][0][1];

            var struct1 = structArray[0][1];
            struct1.GetA();
            struct1.GetB();
        }

        public static void Test5()
        {
            Console.WriteLine("Test5()");
            var listArray = new List<int> { 1, 23, 54, 1231, 213 };
            var objectArray = new List<NewObject> { new NewObject(332), new NewObject(13331), new NewObject(3323241) };

            var a = listArray[3];
            var obj = objectArray[2];
            a += 231;
            obj.C = 13333333;
            listArray[0] = a;
            objectArray[2] = obj;
        }

        public static void Test6()
        {
            Console.WriteLine("Test6()");
            var testStructArray = new TestStruct[2];
            var testStruct = new TestStruct();
            testStructArray[0] = testStruct;
            var a = testStructArray[0].GetA();
            var b = testStructArray[0].GetB();

        }

        public static void Test7()
        {
            Console.WriteLine("Test7()");
            Console.WriteLine("Before lock() :" + _c);
            lock (_c)
            {
                Console.WriteLine("in lock() :" + _c);
            }
            Console.WriteLine("after lock() :" + _c);
        }

        public static void Test9()
        {
            Console.WriteLine("Test9");
            var temp1 = _bool;
            var temp2 = _char;
            var temp3 = _sbyte;
            var temp4 = _int16;
            var temp5 = _int32;
            var temp6 = _int64;
            var temp7 = _intPtr;
            var temp8 = _byte;
            var temp9 = _uint16;
            var temp10 = _uint32;
            var temp11 = _uint64;
            var temp12 = _uIntPtr;
            var temp13 = _single;
            var temp14 = _double;
            var temp17 = _string;

            var temp15 = _c;

            _testStruct.GetA();
            _testStruct.GetB();
            var temp16 = _testStruct;
            _testStruct = new TestStruct();
            var a = temp16.GetA();
            var b = temp16.GetB();
        }

        public static void Test10()
        {
            Console.WriteLine("Test10() => Race Condition");
            var lockA = new object();
            var lockB = new object();
            _brace = 2;
            var taskPool = new List<Task>();
            taskPool.Add(Task.Factory.StartNew(() =>
            {
                lock (lockB)
                {
                    _brace = 3;
                }
                _brace = 4;
                lock (lockA)
                {
                    Console.WriteLine(_arace);
                }
            }));
            taskPool.Add(Task.Factory.StartNew(() =>
            {
                lock (lockA)
                {
                    _arace = 2;
                }
                _brace = 5;
                lock (lockB)
                {
                    Console.WriteLine(_brace);
                }
            }));

            lock (lockA)
            {
                _arace = 3;
            }
            lock (lockB)
            {
                _brace = 6;
            }
            Console.WriteLine(_arace);

            Task.WaitAll(taskPool.ToArray());
        }

        public static void Test11()
        {
            Console.WriteLine("Test11()");
            object a = 1;
            var lockA = new object();
            object b = 1;
            var lockB = new object();
            var test = new object[] {1, 2, 3};
            var test3 = new object[] {1,2,null};
            var class123 = new TestClass();
            var stringarray = new string[] {"Test", "Test2331", class123.B};
            var test4 = new object[] {new NewObject(123), new Action(()=>Test12())};
            var test2 = string.Concat(new string[] {"asdf", stringarray[0], "dasddad"});

            var objArray = new object[3][] {new object[3] {new NewObject(123), new Action(() => Test12()), 123 }, new object[3] {"Test", "123", 123}, new object[3] {323, 12332,12333}  };


            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    objArray[i][j] = _uint16;
                    for (var k = 0; i < 3; i++)
                    {
                        objArray[j][k] = _int32;
                        for (var l = 0; j < 3; j++)
                        {
                            objArray[k][l] = _double;
                            for (var m = 0; i < 3; i++)
                            {
                                objArray[l][m] = _single;
                                for (var n = 0; j < 3; j++)
                                {
                                    objArray[m][n] = "123";
                                }
                            }
                        }
                    }
                }
            }

            test4[1] = new Action(()=> {Console.WriteLine("Test");});
            var task = Task.Factory.StartNew(() =>
            {
                lock (lockA)
                {
                    a = 2;
                }
                lock (lockB)
                {
                    b = 2;
                }
            });
            lock (lockA)
            {
                a = 3;
            }
            lock (lockB)
            {
                b = 2;
            }
            task.Wait();
        }

        public static void Test12()
        {
            Console.WriteLine("Test12()");
            object a = 1;
            var lockA = new object();
            object b = 1;
            var lockB = new object();
            var thread1 = new Thread(() =>
            {
                a = 2;
                b = 2;
            });
            thread1.Start();
            thread1.Join();
            a = 3;
            b = 2;
        }


        public static void Test13()
        {
            Console.WriteLine("Test13()");
            Console.WriteLine("Task");
            _a = 4;
            var task = new Task(() => { _a = 5; });
            task.Start();
            //task.Start(TaskScheduler.Current);

            task.Wait();
            task.Wait(100);
            task.Wait(new TimeSpan(0, 0, 1));
            task.Wait(CancellationToken.None);
            task.Wait(100, new CancellationToken(true));

            _a = 4;
            var task2 = Task.Run(() => { });
            var task3 = Task.Run(() => { }, CancellationToken.None);

            Task task4 = Task.Run(() => { return 2; });
            Task task5 = Task.Run(() => { return 2; }, CancellationToken.None);

            var task6 = Task.Run(() => { return task; });
            task6.Wait();
            var task7 = Task.Run(() => { return task; }, CancellationToken.None);
            task7.Wait();

            Task task8 = Task.Run(() => { return new Task<object>(() => { Console.Write("Task.Run() - func"); return _a; }); });
            Task task9 = Task.Run(() => { return new Task<int>(() => { return 3; }); }, CancellationToken.None);

            Console.WriteLine("Task.Factory");

            var action = new Action(() => { });

            var task10 = Task.Factory.StartNew(action);

            task10.ContinueWith((x) => { return x; });

            task10.Wait();

            var task11 = Task.Factory.StartNew(action, CancellationToken.None);

            var task12 = Task.Factory.StartNew(action, TaskCreationOptions.None);

            var task13 = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);

            var func = new Func<int>(() => { return 3; });

            var task14 = Task.Factory.StartNew(func);

            var task15 = Task.Factory.StartNew(func, CancellationToken.None);

            var task16 = Task.Factory.StartNew(func, TaskCreationOptions.None);

            var task17 = Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);

            Task.WaitAll(new Task[] { task11, task12, task13, task14, task15, task16, task17 });

            var action2 = new Action<object>((x) => { });

            var task18 = Task.Factory.StartNew(action2, new object());

            var task19 = Task.Factory.StartNew(action2, new object(), CancellationToken.None);

            var task20 = Task.Factory.StartNew(action2, new object(), TaskCreationOptions.None);

            var task21 = Task.Factory.StartNew(action2, new object(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);

            Func<object, int> func2 = o => { return 2; };

            var task22 = Task.Factory.StartNew(func2, new object());

            var task23 = Task.Factory.StartNew(func2, new object(), CancellationToken.None);

            var task24 = Task.Factory.StartNew(func2, new object(), TaskCreationOptions.None);

            var task25 = Task.Factory.StartNew(func2, new object(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);

            Task.WaitAll(new Task[] { task18, task19, task20, task21, task22, task23, task24, task25 });

            Console.WriteLine("Task.Factory WaitAll/WaitAny");

            Task<double>[] taskArray = { Task<double>.Factory.StartNew(() => DoComputation(1.0)),
                                     Task<double>.Factory.StartNew(() => DoComputation(100.0)),
                                     Task<double>.Factory.StartNew(() => DoComputation(1000.0)) };

            Task.WaitAll(taskArray);

            Console.WriteLine("");

            taskArray = new Task<double>[] { Task<double>.Factory.StartNew(() => DoComputation(1.0)),
                                     Task<double>.Factory.StartNew(() => DoComputation(100.0)),
                                     Task<double>.Factory.StartNew(() => DoComputation(1000.0)) };

            Task.WaitAny(taskArray);

            Console.WriteLine("Thread.Start / Join");

            var thread = new Thread(() => { DoComputation(100.0); });

            thread.Start();

            thread.Join();

            thread = new Thread(() => { DoComputation(100.0); });

            thread.Start();

            thread.Join(100);

            thread = new Thread(() => { DoComputation(100.0); });

            thread.Start();

            thread.Join(new TimeSpan(0, 0, 1));

            Console.WriteLine("Thread.Start / Abort");

            thread = new Thread(() => { DoComputation(100.0); });

            thread.Start();

            thread.Abort();

            Console.WriteLine("Thread.Start / Interrupt");

            thread = new Thread(() => { DoComputation(100.0); });

            thread.Start();

            thread.Interrupt();

            Console.WriteLine("Parallel");

            Parallel.Invoke(() => { DoComputation(10000.0); }, () => { DoComputation(10000.0); });

            Parallel.For(0, 10, (x) => { DoComputation(x); });

            var ints = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };

            Parallel.ForEach(ints, (x) => { DoComputation(x); });

        }

        private static double DoComputation(double start)
        {
            double sum = 0;
            for (var value = start; value <= start + 1000; value += .1)
                sum += value;

            return sum;
        }
    }

    class TestClass
    {
        private string _b = "Test";

        public string B
        {
            get
            {
                return _b;
            }
            set { _b = value; }
        }
    }

    struct TestStruct
    {
        private object _a;
        private object _b;

        public object GetA()
        {
            _a = 2;
            return _a;
        }

        public object GetB()
        {
            _b = 3;
            return _b;
        }
    }
}
