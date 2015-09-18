using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeCommLibrary
{
    public static class PipeComm
    {

        public static void writePipe(string line)
        {
            NamedPipeClientStream clientStream = new NamedPipeClientStream("pipeDPCNet");
            clientStream.Connect();
            StreamWriter writer = new StreamWriter(clientStream);

            writer.WriteLine(line);
            writer.Flush();
        }
    }
}
