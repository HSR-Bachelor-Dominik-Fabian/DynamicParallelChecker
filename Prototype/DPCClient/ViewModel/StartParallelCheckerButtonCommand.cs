using System;
using System.Windows.Input;
using DPCClient.Model;
using DPCClient.Process;

namespace DPCClient.ViewModel
{
    class StartParallelCheckerButtonCommand : ICommand
    {
        private DPCViewModel _obj;

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
            NLogSocketProcessor processor = new NLogSocketProcessor();
            processor.Run(_obj);
            _obj.AddLogEntry(new LogEntryModel(_obj.FilePath));
        }

        public event EventHandler CanExecuteChanged;
    }
}
