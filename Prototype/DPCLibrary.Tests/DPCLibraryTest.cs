using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        public void TestNoRaceConditionReadFromTask()
        {
            var a = 1;
            var obj = new object();
            var lineofCode = 12;
            var task = Task.Factory.StartNew(() =>
            {
                var thread2 = new Thread(() =>
                {
                    lock (obj)
                    {
                        DpcLibrary.LockObject(a);
                        DpcLibrary.ReadAccess(a, lineofCode, "TestMethod");
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
            });

            DpcLibrary.TaskWait(task);

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
        public void TestNoRaceConditionStartThread()
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

        [TestMethod]
        public void TestNoRaceConditionStartThreadWithParamter()
        {
            var a = 1;
            var b = 2;
            var lineofCode = 12;
            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            var thread2 = new Thread(o =>
            {
                DpcLibrary.LockObject(b);
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                DpcLibrary.UnLockObject(b);
            });
            DpcLibrary.StartThread(thread2, new object());
            thread2.Join();

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionJoinThread()
        {
            var a = 1;
            var lineofCode = 12;
            var thread2 = new Thread(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartThread(thread2);
            DpcLibrary.JoinThread(thread2);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionJoinThreadFromTask()
        {
            var a = 1;
            var lineofCode = 12;
            var task = Task.Factory.StartNew(() =>
            {
                var thread2 = new Thread(() =>
                {
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                });
                DpcLibrary.StartThread(thread2);
                DpcLibrary.JoinThread(thread2);

                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.TaskWait(task);
            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionJoinThreadMilliseconds()
        {
            var a = 1;
            var lineofCode = 12;
            var thread2 = new Thread(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartThread(thread2);
            DpcLibrary.JoinThreadMilliseconds(thread2, 10000);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionJoinThreadTimeout()
        {
            var a = 1;
            var lineofCode = 12;
            var thread2 = new Thread(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartThread(thread2);
            DpcLibrary.JoinThreadTimeout(thread2, new TimeSpan(0,0, 0, 3));

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionStartTask()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = new Task(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartTask(task1);
            DpcLibrary.TaskWait(task1);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionStartTaskFromTask()
        {
            var a = 1;
            var lineofCode = 12;
            var task = Task.Factory.StartNew(() =>
            {
                var task1 = new Task(() =>
                {
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                });
                DpcLibrary.StartTask(task1);
                DpcLibrary.TaskWait(task1);
            });
            DpcLibrary.TaskWait(task);
            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionStartTaskWithScheduler()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = new Task(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartTask(task1, TaskScheduler.Current);
            DpcLibrary.TaskWait(task1);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionWaitTask()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = new Task(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartTask(task1);
            DpcLibrary.TaskWait(task1);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionWaitTaskTimeSpan()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = new Task(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartTask(task1);
            DpcLibrary.TaskWaitTimespan(task1, new TimeSpan(0, 0, 0, 3));

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionWaitTaskTimeout()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = new Task(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartTask(task1);
            DpcLibrary.TaskWaitTimeout(task1, 10000);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionWaitTaskCancelToken()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = new Task(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartTask(task1);
            DpcLibrary.TaskWaitCancelToken(task1, CancellationToken.None);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionWaitTaskTimeoutCancelToken()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = new Task(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            DpcLibrary.StartTask(task1);
            DpcLibrary.TaskWaitTimeOutCancelToken(task1, 10000, CancellationToken.None);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTask()
        {
            var a = 1;
            var lineofCode = 12;
            var action = new Action(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            var task = DpcLibrary.RunTask(action);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTaskFromTask()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = Task.Factory.StartNew(() =>
            {
                var action = new Action(() =>
                {
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                });
                var task = DpcLibrary.RunTask(action);
                DpcLibrary.TaskWait(task);
            });
            DpcLibrary.TaskWait(task1);
            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTaskCancelToken()
        {
            var a = 1;
            var lineofCode = 12;
            var action = new Action(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            var task = DpcLibrary.RunTaskCancel(action, CancellationToken.None);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTaskFunc()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<Task>(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return new Task(() => {});
            });
            var task = DpcLibrary.RunTaskFunc(func);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTaskFuncCancel()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<Task>(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return new Task(() => { });
            });
            var task = DpcLibrary.RunTaskFuncCancel(func, CancellationToken.None);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTaskTResult()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<int>(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return 3;
            });
            var task = DpcLibrary.RunTaskTResult(func);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTaskTResultCancel()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<int>(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return 3;
            });
            var task = DpcLibrary.RunTaskTResultCancel(func, CancellationToken.None);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTaskTaskTResult()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<Task<int>>(() =>
            {
                return new Task<int>(() =>
                {
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                    return 3;
                });
            });
            Task task = DpcLibrary.RunTaskTaskTResult(func);
            DpcLibrary.StartTask(task);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionRunTaskTaskTResultCancel()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<Task<int>>(() =>
            {
                return new Task<int>(() =>
                {
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                    return 3;
                });
            });
            Task task = DpcLibrary.RunTaskTaskTResultCancel(func, CancellationToken.None);
            DpcLibrary.StartTask(task);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNew()
        {
            var a = 1;
            var lineofCode = 12;
            var action = new Action(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            var task = DpcLibrary.StartNew(action, TaskCreationOptions.None);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewFromTask()
        {
            var a = 1;
            var lineofCode = 12;
            var task1 = Task.Factory.StartNew(() =>
            {
                var action = new Action(() =>
                {
                    DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                });
                var task = DpcLibrary.StartNew(action, TaskCreationOptions.None);
                DpcLibrary.TaskWait(task);
            });
            DpcLibrary.TaskWait(task1);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewCancel()
        {
            var a = 1;
            var lineofCode = 12;
            var action = new Action(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            var task = DpcLibrary.StartNewCancel(action, CancellationToken.None, 
                TaskCreationOptions.None, null);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewCancelWithScheduler()
        {
            var a = 1;
            var lineofCode = 12;
            var action = new Action(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            var task = DpcLibrary.StartNewCancel(action, CancellationToken.None,
                TaskCreationOptions.None, TaskScheduler.Current);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewObject()
        {
            var a = 1;
            var lineofCode = 12;
            var action = new Action<object>(o =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            var task = DpcLibrary.StartNewObject(action, new object(), 
                TaskCreationOptions.None);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewObjectCancel()
        {
            var a = 1;
            var lineofCode = 12;
            var action = new Action<object>(o =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            var task = DpcLibrary.StartNewObjectCancel(action, new object(), CancellationToken.None,
                TaskCreationOptions.None, null);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewObjectCancelWithScheduler()
        {
            var a = 1;
            var lineofCode = 12;
            var action = new Action<object>(o =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
            });
            var task = DpcLibrary.StartNewObjectCancel(action, new object(), CancellationToken.None,
                TaskCreationOptions.None, TaskScheduler.Current);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewTResult()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<int>(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return 3;
            });
            var task = DpcLibrary.StartNewTResult(func, TaskCreationOptions.None);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewTResultCancel()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<int>(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return 3;
            });
            var task = DpcLibrary.StartNewTResultCancel(func, CancellationToken.None, 
                TaskCreationOptions.None, null);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewTResultCancelWithScheduler()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<int>(() =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return 3;
            });
            var task = DpcLibrary.StartNewTResultCancel(func, CancellationToken.None,
                TaskCreationOptions.None, TaskScheduler.Current);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewObjectTResult()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<object, int>(o =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return 3;
            });
            var task = DpcLibrary.StartNewObjectTResult(func, new object(), TaskCreationOptions.None);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewObjectTResultCancel()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<object, int>(o =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return 3;
            });
            var task = DpcLibrary.StartNewObjectTResultCancel(func, new object(), 
                CancellationToken.None, TaskCreationOptions.None, null);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
            Assert.AreEqual(0, logs.Count);
        }

        [TestMethod]
        public void TestNoRaceConditionTaskStartNewObjectTResultCancelWithScheduler()
        {
            var a = 1;
            var lineofCode = 12;
            var func = new Func<object, int>(o =>
            {
                DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");
                return 3;
            });
            var task = DpcLibrary.StartNewObjectTResultCancel(func, new object(),
                CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);
            DpcLibrary.TaskWait(task);

            DpcLibrary.WriteAccess(a, lineofCode, "TestMethod");

            var logs = GetMemoryLog();
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
