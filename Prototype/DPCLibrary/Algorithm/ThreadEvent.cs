namespace DPCLibrary.Algorithm
{
    class ThreadEvent
    {

        public enum EventType
        {
            Read, Write, Lock, Unlock
        }

        public EventType ThreadEventType { get; }

        public long Ressource { get; }

        public long LockRessource { get; }

        public ThreadEvent(EventType type, long ressource, long lockRessource)
        {
            ThreadEventType = type;
            Ressource = ressource;
            LockRessource = lockRessource;
        }
    }
}
