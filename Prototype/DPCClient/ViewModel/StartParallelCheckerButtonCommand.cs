using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DPCClient.Model;
using DPCClient.Process;
using System.Threading;
using System.Windows.Threading;

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
            _obj.LogEntries = new ObservableCollection<NLogMessage>();
            _checkingProcessManager = new CheckingProcessManager();
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Thread checkingThread = new Thread(() =>
            {
                _checkingProcessManager.Start(_obj, dispatcher);
            });
            checkingThread.Start();
        }

        public event EventHandler CanExecuteChanged;
    }
}
