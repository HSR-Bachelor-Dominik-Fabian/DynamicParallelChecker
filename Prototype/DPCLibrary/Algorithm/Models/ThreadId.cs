namespace DPCLibrary.Algorithm.Models
{
    class ThreadId
    {
        public string Identifier { get; }

        public ThreadId(string identifier)
        {
            Identifier = identifier;
        }

        public static implicit operator ThreadId(string identifier)
        {
            return new ThreadId(identifier);
        }
    }
}
