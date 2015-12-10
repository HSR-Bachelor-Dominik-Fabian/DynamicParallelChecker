using System.Collections.Generic;
using System.Threading;
using DPCLibrary.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Targets;

namespace DPCLibrary.Tests
{
    [TestClass]
    [DeploymentItem(@".\Nlog.config")]
    public class DpcLibraryTest
    {
        
        [TestMethod]
        public void TestNoRaceConditionRead()
        {
            int a = 1;
            object obj = new object();
            int lineofCode = 12;
            Thread thread2 = new Thread(() =>
            {
                lock (obj)
                {
                    DpcLibrary.LockObject(a);
                    DpcLibrary.ReadAccess(a, lineofCode,"TestMethod");
                    DpcLibrary.UnLockObject(a);
                }
            });
            thread2.Start();

            lock (obj)
            {
                DpcLibrary.LockObject(a);
                DpcLibrary.ReadAccess(a, lineofCode, "TestMethod");
                DpcLibrary.UnLockObject(a);
            }
            thread2.Join();

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionReadWrite()
        {
            int a = 1;
            object obj = new object();
            int lineofCode = 12;
            Thread thread2 = new Thread(() =>
            {
                lock (obj)
                {
                    DpcLibrary.LockObject(a);
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                    DpcLibrary.UnLockObject(a);
                }
            });
            thread2.Start();
            
            lock (obj)
            {
                DpcLibrary.LockObject(a);
                DpcLibrary.ReadAccess(a, lineofCode, "TestMethod");
                DpcLibrary.UnLockObject(a);
            }
            
            thread2.Join();

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionReadWrite()
        {
            int a = 1;
            int b = 2;
            object obj = new object();
            object objB = new object();
            int lineofCode = 12;
            Thread thread2 = new Thread(() =>
            {
                lock (objB)
                {
                    DpcLibrary.LockObject(b);
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                    DpcLibrary.UnLockObject(b);
                }
            });
            thread2.Start();
            lock (obj)
            {
                DpcLibrary.LockObject(a);
                DpcLibrary.ReadAccess(a, lineofCode, "TestMethod");
                DpcLibrary.UnLockObject(a);
            }
            thread2.Join();

            string expected = $"RaceCondition detected... Ressource: {a} -> Thread: ";
            List<string> logs = GetMemoryLog();
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(expected, logs[0].Substring(0, 50));
            
        }

        [TestMethod]
        public void TestNoRaceConditionThreeThreads()
        {
            int a = 1;
            int b = 2;
            object obj = new object();
            object objB = new object();
            int lineofCode = 12;
            Thread thread2 = new Thread(() =>
            {
                lock (objB)
                {
                    DpcLibrary.LockObject(b);
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                    DpcLibrary.UnLockObject(b);
                }
            });
            thread2.Start();
            Thread thread3 = new Thread(() =>
            {
                lock (objB)
                {
                    DpcLibrary.LockObject(b);
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                    DpcLibrary.UnLockObject(b);
                }
            });
            thread3.Start();
            lock (obj)
            {
                DpcLibrary.LockObject(a);
                DpcLibrary.ReadAccess(a, lineofCode, "TestMethod");
                DpcLibrary.UnLockObject(a);
            }
            thread2.Join();
            thread3.Join();

            string expected = $"RaceCondition detected... Ressource: {a} -> Thread: ";
            List<string> logs = GetMemoryLog();
            Assert.AreNotEqual(0,logs.Count);
            Assert.AreEqual(expected, logs[0].Substring(0, 50));
        }

        [TestMethod]
        public void TestNoRaceConditionStartThreads()
        {
            int a = 1;
            int b = 2;
            int lineofCode = 12;
            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            Thread thread2 = new Thread(() =>
            {
                DpcLibrary.LockObject(b);
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                DpcLibrary.UnLockObject(b);
            });
            DpcLibrary.StartThread(thread2);
            thread2.Join();

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }


        [TestCleanup]
        public void CleanUp()
        {
            ThreadVectorFacade.Reset();
            GetMemoryLog().Clear();
        }
        private List<string> GetMemoryLog()
        {
            return (List<string>)((MemoryTarget)LogManager.Configuration.FindTargetByName("memory")).Logs;
        }
    }
}
