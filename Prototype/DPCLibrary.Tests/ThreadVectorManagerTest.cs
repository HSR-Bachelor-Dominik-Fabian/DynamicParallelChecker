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
        [DeploymentItem(@".\Nlog.config")]
        public void TestRaceConditionReadWrite()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";

            int ressource = 1;
            int ownLockRessource = 2;
            int otherLockRessource = 3;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                $"RaceCondition detected... Ressource: {ressource} -> Thread: {thread2} to Thread: {thread}");
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionWriteWrite()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";

            int ressource = 1;
            int ownLockRessource = 2;
            int otherLockRessource = 3;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);

            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                $"RaceCondition detected... Ressource: {ressource} -> Thread: {thread2} to Thread: {thread}");
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionReadRead()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";

            int ressource = 1;
            int ownLockRessource = 2;
            int otherLockRessource = 3;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread2, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionWriteRead()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";

            int ressource = 1;
            int ownLockRessource = 2;
            int otherLockRessource = 3;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread2, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                $"RaceCondition detected... Ressource: {ressource} -> Thread: {thread2} to Thread: {thread}");
            Assert.AreEqual(1, logs.Count);
        }


        [TestMethod]
        public void TestRaceConditionReadWriteWithoutLock()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";

            int ressource = 1;
            int ownLockRessource = 2;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ressource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleReadAccess(thread2, ressource, lineofCode, "TestMethodName");
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                $"RaceCondition detected... Ressource: {ressource} -> Thread: {thread2} to Thread: {thread}");
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionReadWriteDifferentRessource()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";

            int ownRessource = 1;
            int otherRessource = 2;
            int ownLockRessource = 2;
            int otherLockRessource = 3;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, otherRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionWithSync()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";

            int ownRessource = 1;
            int ownLockRessource = 2;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, ownLockRessource);
                
            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionAfterSync()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";

            int ownRessource = 1;
            int ownLockRessource = 2;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);    // clock: 1=>1
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource); // clock: 1=>2

            ThreadVectorManager.GetInstance().HandleLock(thread2, ownLockRessource); // clock: 1=>1, 2=>2
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, ownLockRessource); // clock: 1=>1, 2=>3

            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                $"RaceCondition detected... Ressource: {ownRessource} -> Thread: {thread} to Thread: {thread2}");
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionThreeThreads()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            string thread3 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 2}";

            int ownRessource = 1;
            int otherRessource = 2;
            int ownLockRessource = 3;
            int otherLockRessource = 4;
            int otherotherLockRessource = 5;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread2, otherRessource, lineofCode, "TestMethodName");
                
            ThreadVectorManager.GetInstance().HandleLock(thread3, otherotherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread3, otherRessource, lineofCode, "TestMethodName");

            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, otherotherLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
                
            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestRaceConditionThreeThreads()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            string thread3 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 2}";

            int ownRessource = 1;
            int otherRessource = 2;
            int ownLockRessource = 3;
            int otherLockRessource = 4;
            int otherotherLockRessource = 5;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, otherRessource, lineofCode, "TestMethodName");

            ThreadVectorManager.GetInstance().HandleLock(thread3, otherotherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread3, otherRessource, lineofCode, "TestMethodName");

            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, otherotherLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);
            
            List<string> logs = GetMemoryLog();
            CollectionAssert.Contains(logs,
                $"RaceCondition detected... Ressource: {otherRessource} -> Thread: {thread3} to Thread: {thread2}");
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionAfterSyncThreeThreads()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            string thread3 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 2}";

            int ownRessource = 1;
            int otherRessource = 2;
            int ownLockRessource = 3;
            int otherLockRessource = 4;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");

            ThreadVectorManager.GetInstance().HandleLock(thread2, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, otherRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, otherLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread3, otherLockRessource);
            ThreadVectorManager.GetInstance().HandleReadAccess(thread3, otherRessource, lineofCode, "TestMethodName");

            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, otherLockRessource);

            List<string> logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionSyncAfterSync()
        {
            string thread = $"Thread_{Thread.CurrentThread.ManagedThreadId}";
            string thread2 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 1}";
            string thread3 = $"Thread_{Thread.CurrentThread.ManagedThreadId + 2}";

            int ownRessource = 1;
            int ownLockRessource = 2;
            int lineofCode = 12;

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread3, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread3, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread2, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread2, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread2, ownLockRessource);

            ThreadVectorManager.GetInstance().HandleLock(thread3, ownLockRessource);
            ThreadVectorManager.GetInstance().HandleWriteAccess(thread3, ownRessource, lineofCode, "TestMethodName");
            ThreadVectorManager.GetInstance().HandleUnLock(thread3, ownLockRessource);

            List<string> logs = GetMemoryLog();
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
