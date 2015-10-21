namespace DPCLibrary.Algorithm
{
    class ThreadEvent
    {

        public enum EventType
        {
            Read = 1, Write = 2
        }

        public EventType ThreadEventType { get; set; }

        public long Ressource { get; }

        public long LockRessource { get; }

        public ThreadEvent(EventType type, long ressource, long lockRessource)
        {
            ThreadEventType = type;
            Ressource = ressource;
            LockRessource = lockRessource;
        }

        public bool CompareRessourceAndLock(ThreadEvent other)
        {
            return other.LockRessource.Equals(LockRessource) && other.Ressource.Equals(Ressource);
        }
    }
}
