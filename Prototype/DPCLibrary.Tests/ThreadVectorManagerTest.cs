using System;
using System.IO;
using System.Threading;
using DPCLibrary.Algorithm.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DPCLibrary.Tests
{
    [TestClass]
    public class ThreadVectorManagerTest
    {
        [TestMethod]
        public void TestRaceConditionReadWrite()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + thread2.ManagedThreadId + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestRaceConditionWriteWrite()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + thread2.ManagedThreadId + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionReadRead()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestRaceConditionWriteRead()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + thread2.ManagedThreadId + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }


        [TestMethod]
        public void TestRaceConditionReadWriteWithoutLock()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                Thread thread = Thread.CurrentThread;
                Thread thread2 = new Thread(() => { });

                int ressource = 1;
                int ownLockRessource = 2;

                ThreadVectorManager.GetInstance().HandleLock(thread, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(thread, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(thread, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleReadAccess(thread2, ressource);

                string expected = "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + thread2.ManagedThreadId + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionReadWriteDifferentRessource()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionWithSync()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestRaceConditionAfterSync()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "RaceCondition detected... Ressource: " + ownRessource + ", in Thread: " + thread.ManagedThreadId + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionThreeThreads()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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
                
                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestRaceConditionThreeThreads()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "RaceCondition detected... Ressource: " + otherRessource + ", in Thread: " + thread3.ManagedThreadId + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionAfterSyncThreeThreads()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionSyncAfterSync()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

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

                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            ThreadVectorManager.Reset();
        }
    }
}
