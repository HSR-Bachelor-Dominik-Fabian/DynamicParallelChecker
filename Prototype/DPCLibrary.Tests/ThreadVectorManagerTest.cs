using System;
using System.Collections.Generic;
using System.Threading;
using DPCLibrary.Algorithm.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Targets;

namespace DPCLibrary.Tests
{
    [TestClass]
    public class ThreadVectorManagerTest
    {
        [TestMethod]
        public void TestRaceConditionReadWrite()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });

            int ressource = 1;
            int ownLockRessource = 2;
            int otherLockRessource = 3;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + thread2.ManagedThreadId);
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionWriteWrite()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });

            int ressource = 1;
            int ownLockRessource = 2;
            int otherLockRessource = 3;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);

            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + thread2.ManagedThreadId);
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionReadRead()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });

            int ressource = 1;
            int ownLockRessource = 2;
            int otherLockRessource = 3;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread2, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionWriteRead()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });

            int ressource = 1;
            int ownLockRessource = 2;
            int otherLockRessource = 3;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread2, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + thread2.ManagedThreadId);
            Assert.AreEqual(1, logs.Count);
        }


        [TestMethod]
        public void TestRaceConditionReadWriteWithoutLock()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });

            int ressource = 1;
            int ownLockRessource = 2;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ressource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleReadAccess(thread2, ressource);
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + thread2.ManagedThreadId);
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionReadWriteDifferentRessource()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });

            int ownRessource = 1;
            int otherRessource = 2;
            int ownLockRessource = 2;
            int otherLockRessource = 3;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, otherRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionWithSync()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });

            int ownRessource = 1;
            int ownLockRessource = 2;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, ownLockRessource);
                
            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionAfterSync()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });

            int ownRessource = 1;
            int ownLockRessource = 2;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);    // clock: 1=>1
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource); // clock: 1=>2

            ThreadVectorManager.GetInstance().HandleLock(thread2, ownLockRessource); // clock: 1=>1, 2=>2
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, ownLockRessource); // clock: 1=>1, 2=>3

            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                "RaceCondition detected... Ressource: " + ownRessource + ", in Thread: " + thread.ManagedThreadId);
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionThreeThreads()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            Thread thread3 = new Thread(() => { });

            int ownRessource = 1;
            int otherRessource = 2;
            int ownLockRessource = 3;
            int otherLockRessource = 4;
            int otherotherLockRessource = 5;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread2, otherRessource);
                
            ThreadVectorManager.GetInstance().HandleLock(thread3, otherotherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread3, otherRessource);

            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, otherotherLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
                
            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionThreeThreads()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            Thread thread3 = new Thread(() => { });

            int ownRessource = 1;
            int otherRessource = 2;
            int ownLockRessource = 3;
            int otherLockRessource = 4;
            int otherotherLockRessource = 5;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, otherRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread3, otherotherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread3, otherRessource);

            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, otherotherLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
            
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                "RaceCondition detected... Ressource: " + otherRessource + ", in Thread: " + thread3.ManagedThreadId);
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionAfterSyncThreeThreads()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            Thread thread3 = new Thread(() => { });

            int ownRessource = 1;
            int otherRessource = 2;
            int ownLockRessource = 3;
            int otherLockRessource = 4;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, otherRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread3, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread3, otherRessource);

            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, otherLockRessource);

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionSyncAfterSync()
        {
            Thread thread = Thread.CurrentThread;
            Thread thread2 = new Thread(() => { });
            Thread thread3 = new Thread(() => { });

            int ownRessource = 1;
            int ownLockRessource = 2;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread3, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread3, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread3, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread3, ownRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, ownLockRessource);

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [DeploymentItem("./Nlog.config")]
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
