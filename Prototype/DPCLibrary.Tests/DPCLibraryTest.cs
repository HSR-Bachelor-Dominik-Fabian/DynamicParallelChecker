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

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionReadWrite()
        {
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

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionReadWrite()
        {
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
            List<string> logs = GetMemoryLog();
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(expected, logs[0].Substring(0, 50));
            
        }

        [TestMethod]
        public void TestNoRaceConditionThreeThreads()
        {
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
            List<string> logs = GetMemoryLog();
            Assert.AreNotEqual(0,logs.Count);
            Assert.AreEqual(expected, logs[0].Substring(0, 50));
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
