using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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

                int ressource = 1;
                int ownLockRessource = 2;
                int otherLockRessource = 3;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleReadAccess(1, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, otherLockRessource);

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

                int ressource = 1;
                int ownLockRessource = 2;
                int otherLockRessource = 3;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, otherLockRessource);

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

                int ressource = 1;
                int ownLockRessource = 2;
                int otherLockRessource = 3;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleReadAccess(1, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleReadAccess(2, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, otherLockRessource);

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

                int ressource = 1;
                int ownLockRessource = 2;
                int otherLockRessource = 3;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleReadAccess(2, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, otherLockRessource);

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

                int ressource = 1;
                int ownLockRessource = 2;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ressource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleReadAccess(2, ressource);

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

                int ownRessource = 1;
                int otherRessource = 2;
                int ownLockRessource = 2;
                int otherLockRessource = 3;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, otherRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, otherLockRessource);

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

                int ownRessource = 1;
                int ownLockRessource = 2;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, ownLockRessource);

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

                int ownRessource = 1;
                int ownLockRessource = 2;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);    // clock: 1=>1
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource); // clock: 1=>2

                ThreadVectorManager.GetInstance().HandleLock(2, ownLockRessource); // clock: 1=>1, 2=>2
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, ownLockRessource); // clock: 1=>1, 2=>3

                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);

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

                int ownRessource = 1;
                int otherRessource = 2;
                int ownLockRessource = 3;
                int otherLockRessource = 4;
                int otherotherLockRessource = 5;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleReadAccess(2, otherRessource);
                
                ThreadVectorManager.GetInstance().HandleLock(3, otherotherLockRessource);
                ThreadVectorManager.GetInstance().HandleReadAccess(3, otherRessource);

                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(3, otherotherLockRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, otherLockRessource);
                
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

                int ownRessource = 1;
                int otherRessource = 2;
                int ownLockRessource = 3;
                int otherLockRessource = 4;
                int otherotherLockRessource = 5;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, otherRessource);

                ThreadVectorManager.GetInstance().HandleLock(3, otherotherLockRessource);
                ThreadVectorManager.GetInstance().HandleReadAccess(3, otherRessource);

                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(3, otherotherLockRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, otherLockRessource);

                string expected = "RaceCondition detected... Ressource: " + otherRessource + ", in Thread: " + 3 + "\r\n";
                Assert.AreEqual(expected, sw.ToString());
            }
        }

        [TestMethod]
        public void TestNoRaceConditionAfterSyncThreeThreads()
        {
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                int ownRessource = 1;
                int otherRessource = 2;
                int ownLockRessource = 3;
                int otherLockRessource = 4;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, otherRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, otherLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(3, otherLockRessource);
                ThreadVectorManager.GetInstance().HandleReadAccess(3, otherRessource);

                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(3, otherLockRessource);

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

                int ownRessource = 1;
                int ownLockRessource = 2;

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(3, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(3, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(3, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(1, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(1, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(1, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(2, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(2, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(2, ownLockRessource);

                ThreadVectorManager.GetInstance().HandleLock(3, ownLockRessource);
                ThreadVectorManager.GetInstance().HandleWriteAccess(3, ownRessource);
                ThreadVectorManager.GetInstance().HandleUnLock(3, ownLockRessource);

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
