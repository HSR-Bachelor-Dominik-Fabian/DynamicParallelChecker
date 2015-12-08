using System;
using System.IO;
using CodeInstrumentation;

namespace DPCClient.Process
{
    class InstrumentationProcessor
    {
        public void Start(string path)
        {
            string workingDir = Path.GetDirectoryName(path) ?? string.Empty;
            CodeInstrumentator.InjectCodeInstrumentation(path);
            System.Diagnostics.Process process = new System.Diagnostics.Process { StartInfo = { FileName = path, WorkingDirectory = workingDir } };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new InvalidProgramException($"Programm Exited with: {process.ExitCode}");
            }
        }
    }
}
