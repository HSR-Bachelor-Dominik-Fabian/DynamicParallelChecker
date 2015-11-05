using System.IO;
using System.Windows.Threading;
using CodeInstrumentation;
using DPCClient.ViewModel;

namespace DPCClient.Process
{
    class CheckingProcessManager
    {
        private readonly CopyProcessor _copyProcessor;
        private readonly InstrumentationProcessor _instrumentationProcessor;
        private readonly NLogSocketProcessor _nLogSocketProcessor;

        public CheckingProcessManager()
        {
            _copyProcessor = new CopyProcessor();
            _instrumentationProcessor = new InstrumentationProcessor();
            _nLogSocketProcessor = new NLogSocketProcessor();
        }

        public void Start(DpcViewModel viewModel, Dispatcher dispatcher, StartParallelCheckerButtonCommand startCommand)
        {
            _nLogSocketProcessor.Run(viewModel, dispatcher);

            // Copy all the files
            string copyPath = Directory.GetCurrentDirectory() + @"\" + _copyProcessor.Start(viewModel.FilePathModel);

            string method = CodeInstrumentator.DecompileCode(copyPath, "Program", "Test9", 24);

            _instrumentationProcessor.Start(copyPath);

            _copyProcessor.CleanUp();

            _nLogSocketProcessor.Stop();

            dispatcher.Invoke(() =>
            {
                startCommand.IsReadyForChecking = true;
            });

        }
    }
}
