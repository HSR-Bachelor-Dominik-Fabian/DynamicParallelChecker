using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPCLibrary.Algorithm
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
