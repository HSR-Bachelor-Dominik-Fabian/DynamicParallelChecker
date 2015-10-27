namespace DPCLibrary.Algorithm
{
    class ThreadEvent
    {

        public enum EventType
        {
            Read = 1, Write = 2
        }

        public EventType ThreadEventType { get; set; }

        public int Ressource { get; }

        public ThreadEvent(EventType type, int ressource)
        {
            ThreadEventType = type;
            Ressource = ressource;
        }

        public bool CompareRessource(ThreadEvent other)
        {
            return other.Ressource.Equals(Ressource);
        }
    }
}
