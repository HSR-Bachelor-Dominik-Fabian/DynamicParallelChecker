using System;
using System.IO;
using System.Threading;
using DPCLibrary.Algorithm.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DPCLibrary.Tests
{
    [TestClass]
    public class DpcLibraryTest
    {
        
        [TestMethod]
        public void TestNoRaceConditionRead()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                int a = 1;
                Thread thread2 = new Thread(() =>
                {
                    DpcLibrary.LockObject(a);
                    DpcLibrary.ReadAccess(a);
                    DpcLibrary.UnLockObject(a);
                });
                thread2.Start();
                DpcLibrary.LockObject(a);
                DpcLibrary.ReadAccess(a);
                DpcLibrary.UnLockObject(a);
                thread2.Join();

                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionReadWrite()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                int a = 1;
                Thread thread2 = new Thread(() =>
                {
                    DpcLibrary.LockObject(a);
                    DpcLibrary.WriteAccess(a);
                    DpcLibrary.UnLockObject(a);
                });
                thread2.Start();
                DpcLibrary.LockObject(a);
                DpcLibrary.ReadAccess(a);
                DpcLibrary.UnLockObject(a);
                thread2.Join();

                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestRaceConditionReadWrite()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                int a = 1;
                int b = 2;
                Thread thread2 = new Thread(() =>
                {
                    DpcLibrary.LockObject(b);
                    DpcLibrary.WriteAccess(a);
                    DpcLibrary.UnLockObject(b);
                });
                thread2.Start();
                DpcLibrary.LockObject(a);
                DpcLibrary.ReadAccess(a);
                DpcLibrary.UnLockObject(a);
                thread2.Join();

                string expected = "RaceCondition detected... Ressource: " + a + ", in Thread:";
                Assert.AreEqual(expected, sw.ToString().Substring(0,50));
            }
        }

        [TestMethod]
        public void TestNoRaceConditionThreeThreads()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                int a = 1;
                int b = 2;
                Thread thread2 = new Thread(() =>
                {
                    DpcLibrary.LockObject(b);
                    DpcLibrary.WriteAccess(a);
                    DpcLibrary.UnLockObject(b);
                });
                thread2.Start();
                Thread thread3 = new Thread(() =>
                {
                    DpcLibrary.LockObject(b);
                    DpcLibrary.WriteAccess(a);
                    DpcLibrary.UnLockObject(b);
                });
                thread3.Start();
                DpcLibrary.LockObject(a);
                DpcLibrary.ReadAccess(a);
                DpcLibrary.UnLockObject(a);
                thread2.Join();
                thread3.Join();

                string expected = "RaceCondition detected... Ressource: " + a + ", in Thread:";
                Assert.AreEqual(expected, sw.ToString().Substring(0, 50));
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            ThreadVectorManager.Reset();
        }
    }
}
