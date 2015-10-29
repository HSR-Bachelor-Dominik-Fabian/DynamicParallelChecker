using System;
using System.Windows.Input;
using DPCClient.Model;
using DPCClient.Process;
using System.Threading;

namespace DPCClient.ViewModel
{
    class StartParallelCheckerButtonCommand : ICommand
    {
        private DPCViewModel _obj;
        private CheckingProcessManager _checkingProcessManager;

        public StartParallelCheckerButtonCommand(DPCViewModel obj)
        {
            _obj = obj;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _checkingProcessManager = new CheckingProcessManager();
            Thread checkingThread = new Thread(() =>
            {
                _checkingProcessManager.Start(_obj);
            });
            checkingThread.Start();
            NLogSocketProcessor processor = new NLogSocketProcessor();
            processor.Run(_obj);
            _obj.AddLogEntry(new LogEntryModel(_obj.FilePath));
        }

        public event EventHandler CanExecuteChanged;
    }
}
