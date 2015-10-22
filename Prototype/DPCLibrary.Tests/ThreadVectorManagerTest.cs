using System;
using System.Collections.Generic;
using System.IO;
using DPCLibrary.Algorithm;
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
                Assert.AreEqual<string>(expected, sw.ToString());
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
                Assert.AreEqual<string>(expected, sw.ToString());
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
                Assert.AreEqual<string>(expected, sw.ToString());
            }
        }


        [TestMethod]
        public void TestHandleUnlock()
        {

        }

        [TestMethod]
        public void TestHandleLock()
        {

        }

        [TestMethod]
        public void TestHandleRead()
        {
            
        }

        [TestMethod]
        public void TestHandleWrite()
        {
            
        }

        [TestCleanup]
        public void CleanUp()
        {
            typeof (ThreadVectorManager).TypeInitializer.Invoke(null, null);
        }
    }
}
