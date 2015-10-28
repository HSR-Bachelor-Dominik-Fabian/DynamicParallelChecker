using System;
using System.Windows.Input;
using DPCClient.Model;

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

            _obj.AddLogEntry(new LogEntryModel(_obj.FilePath));
        }

        public event EventHandler CanExecuteChanged;
    }
}
