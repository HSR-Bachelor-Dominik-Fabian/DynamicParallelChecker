using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPCClient.ViewModel;

namespace DPCClient.Process
{
    class CheckingProcessManager
    {
        private CopyProcessor _copyProcessor;
        private InstrumentationProcessor _instrumentationProcessor;
        private NLogSocketProcessor _nLogSocketProcessor;

        public CheckingProcessManager()
        {
            _copyProcessor = new CopyProcessor();
            _instrumentationProcessor = new InstrumentationProcessor();
            _nLogSocketProcessor = new NLogSocketProcessor();
        }

        public void Start(DPCViewModel viewModel)
        {
            // Copy all the files
            string copyPath = Directory.GetCurrentDirectory() + @"\" + _copyProcessor.Start(viewModel.FilePathModel);
            
            _instrumentationProcessor.Start(copyPath);

            _copyProcessor.CleanUp();
        }
    }
}
