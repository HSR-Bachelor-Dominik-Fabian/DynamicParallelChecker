using System.Collections.Generic;
using System.Threading;
using DPCLibrary.Algorithm.Manager;
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
            var a = 1;
            var obj = new object();
            var lineofCode = 12;
            var thread2 = new Thread(() =>
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

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionReadWrite()
        {
            var a = 1;
            var obj = new object();
            var lineofCode = 12;
            var thread2 = new Thread(() =>
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

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionReadWrite()
        {
            var a = 1;
            var b = 2;
            var obj = new object();
            var objB = new object();
            var lineofCode = 12;
            var thread2 = new Thread(() =>
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
            var logs = GetMemoryLog();
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(expected, logs[0].Substring(0, 50));
            
        }

        [TestMethod]
        public void TestNoRaceConditionThreeThreads()
        {
            var a = 1;
            var b = 2;
            var obj = new object();
            var objB = new object();
            var lineofCode = 12;
            var thread2 = new Thread(() =>
            {
                lock (objB)
                {
                    DpcLibrary.LockObject(b);
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                    DpcLibrary.UnLockObject(b);
                }
            });
            thread2.Start();
            var thread3 = new Thread(() =>
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
            var logs = GetMemoryLog();
            Assert.AreNotEqual(0,logs.Count);
            Assert.AreEqual(expected, logs[0].Substring(0, 50));
        }

        [TestMethod]
        public void TestNoRaceConditionStartThreads()
        {
            var a = 1;
            var b = 2;
            var lineofCode = 12;
            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            var thread2 = new Thread(() =>
            {
                DpcLibrary.LockObject(b);
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                DpcLibrary.UnLockObject(b);
            });
            DpcLibrary.StartThread(thread2);
            thread2.Join();

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }


        [TestCleanup]
        public void CleanUp()
        {
            ThreadVectorManager.Reset();
            GetMemoryLog().Clear();
        }
        private List<string> GetMemoryLog()
        {
            return (List<string>)((MemoryTarget)LogManager.Configuration.FindTargetByName("memory")).Logs;
        }
    }
}
