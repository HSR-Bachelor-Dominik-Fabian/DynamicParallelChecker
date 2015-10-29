using CodeInstrumentationTest;

namespace DPCClient.Process
{
    class InstrumentationProcessor
    {

        public bool Start(string path)
        {
            CodeInstrumentator.InjectCodeInstrumentation(path);
            System.Diagnostics.Process process = new System.Diagnostics.Process { StartInfo = { FileName = path } };
            process.Start();
            process.WaitForExit();
            return true;
        }
    }
}
