namespace DPCLibrary.Algorithm.Models
{
    class ThreadEvent
    {

        public enum EventType
        {
            Read = 1, Write = 2
        }

        public EventType ThreadEventType { get; set; }

        public int Ressource { get; }

        public string MethodName { get; set; }

        public int Row { get; set; }

        public ThreadEvent(EventType type, int ressource, int row, string methodName)
        {
            ThreadEventType = type;
            Ressource = ressource;
            Row = row;
            MethodName = methodName;
        }

        public bool CompareRessource(ThreadEvent other)
        {
            return other.Ressource.Equals(Ressource);
        }
    }
}
