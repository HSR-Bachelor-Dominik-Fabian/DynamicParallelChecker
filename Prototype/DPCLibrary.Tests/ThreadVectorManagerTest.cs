using System;
using System.IO;
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

                long ressource = 1L;
                long ownLockRessource = 2L;
                long otherLockRessource = 3L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);
                ThreadVectorManager.HandleReadAccess(1, ressource);
                ThreadVectorManager.HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.HandleLock(2, otherLockRessource);
                ThreadVectorManager.HandleWriteAccess(2, ressource);
                ThreadVectorManager.HandleUnLock(2, otherLockRessource);

                string expected = "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + 2 + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestRaceConditionWriteWrite()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                long ressource = 1L;
                long ownLockRessource = 2L;
                long otherLockRessource = 3L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);
                ThreadVectorManager.HandleWriteAccess(1, ressource);
                ThreadVectorManager.HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.HandleLock(2, otherLockRessource);
                ThreadVectorManager.HandleWriteAccess(2, ressource);
                ThreadVectorManager.HandleUnLock(2, otherLockRessource);

                string expected = "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + 2 + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionReadRead()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                long ressource = 1L;
                long ownLockRessource = 2L;
                long otherLockRessource = 3L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);
                ThreadVectorManager.HandleReadAccess(1, ressource);
                ThreadVectorManager.HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.HandleLock(2, otherLockRessource);
                ThreadVectorManager.HandleReadAccess(2, ressource);
                ThreadVectorManager.HandleUnLock(2, otherLockRessource);

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

                long ressource = 1L;
                long ownLockRessource = 2L;
                long otherLockRessource = 3L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);
                ThreadVectorManager.HandleWriteAccess(1, ressource);
                ThreadVectorManager.HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.HandleLock(2, otherLockRessource);
                ThreadVectorManager.HandleReadAccess(2, ressource);
                ThreadVectorManager.HandleUnLock(2, otherLockRessource);

                string expected = "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + 2 + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }


        [TestMethod]
        public void TestRaceConditionReadWriteWithoutLock()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                long ressource = 1L;
                long ownLockRessource = 2L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);
                ThreadVectorManager.HandleWriteAccess(1, ressource);
                ThreadVectorManager.HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.HandleReadAccess(2, ressource);

                string expected = "RaceCondition detected... Ressource: " + ressource + ", in Thread: " + 2 + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionReadWriteDifferentRessource()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                long ownRessource = 1L;
                long otherRessource = 2L;
                long ownLockRessource = 2L;
                long otherLockRessource = 3L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);
                ThreadVectorManager.HandleWriteAccess(1, ownRessource);
                ThreadVectorManager.HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.HandleLock(2, otherLockRessource);
                ThreadVectorManager.HandleWriteAccess(2, otherRessource);
                ThreadVectorManager.HandleUnLock(2, otherLockRessource);

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

                long ownRessource = 1L;
                long ownLockRessource = 2L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);
                ThreadVectorManager.HandleWriteAccess(1, ownRessource);
                ThreadVectorManager.HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.HandleLock(2, ownLockRessource);
                ThreadVectorManager.HandleWriteAccess(2, ownRessource);
                ThreadVectorManager.HandleUnLock(2, ownLockRessource);

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

                long ownRessource = 1L;
                long ownLockRessource = 2L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);    // clock: 1=>1
                ThreadVectorManager.HandleWriteAccess(1, ownRessource);
                ThreadVectorManager.HandleUnLock(1, ownLockRessource); // clock: 1=>2

                ThreadVectorManager.HandleLock(2, ownLockRessource); // clock: 1=>1, 2=>2
                ThreadVectorManager.HandleWriteAccess(2, ownRessource);
                ThreadVectorManager.HandleUnLock(2, ownLockRessource); // clock: 1=>1, 2=>3

                ThreadVectorManager.HandleWriteAccess(1, ownRessource);

                string expected = "RaceCondition detected... Ressource: " + ownRessource + ", in Thread: " + 1 + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionThreeThreads()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                long ownRessource = 1L;
                long otherRessource = 2L;
                long ownLockRessource = 3L;
                long otherLockRessource = 4L;
                long otherotherLockRessource = 5L;

                ThreadVectorManager.HandleLock(1, ownLockRessource);
                ThreadVectorManager.HandleWriteAccess(1, ownRessource);

                ThreadVectorManager.HandleLock(2, otherLockRessource);
                ThreadVectorManager.HandleReadAccess(2, otherRessource);
                
                ThreadVectorManager.HandleLock(3, otherotherLockRessource);
                ThreadVectorManager.HandleReadAccess(3, otherRessource);

                ThreadVectorManager.HandleUnLock(1, ownLockRessource);
                ThreadVectorManager.HandleUnLock(3, otherotherLockRessource);
                ThreadVectorManager.HandleUnLock(2, otherLockRessource);
                
                string expected = "";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            typeof (ThreadVectorManager).TypeInitializer.Invoke(null, null);
        }
    }
}
