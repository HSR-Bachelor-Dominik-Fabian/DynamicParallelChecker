using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPCLibrary.Algorithm.Manager
{
    class ThreadVectorManager
    {

        private static readonly Dictionary<int, ThreadVectorInstance> _threadVectorPool
            = new Dictionary<int, ThreadVectorInstance>();

        public static void HandleReadAccess(int threadId, long ressource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
        }

        public static void HandleWriteAccess(int threadId, long ressource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
        }

        public static void HandleLock(int threadId, long lockRessource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
        }

        public static void HandleUnLock(int threadId, long lockRessource)
        {
            ThreadVectorInstance threadVectorInstance = GetThreadVectorInstance(threadId);
        }

        private static ThreadVectorInstance GetThreadVectorInstance(int threadId)
        {
            ThreadVectorInstance threadVectorInstance;
            if (!_threadVectorPool.TryGetValue(threadId, out threadVectorInstance))
            {
                threadVectorInstance = new ThreadVectorInstance(threadId);
                _threadVectorPool.Add(threadId, threadVectorInstance);
            }
            return threadVectorInstance;
        }

    }
}
