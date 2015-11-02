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
        private readonly DpcViewModel _obj;
        private CheckingProcessManager _checkingProcessManager;
        private bool _isChecking;

        public StartParallelCheckerButtonCommand(DpcViewModel obj)
        {
            _obj = obj;
            _isChecking = false;
        }

        public bool CanExecute(object parameter)
        {
            return !_isChecking;
        }

        public bool IsChecking
        {
            get { return _isChecking; }
            set
            {
                _isChecking = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {
            IsChecking = true;
            _obj.LogEntries = new ObservableCollection<NLogMessage>();
            _checkingProcessManager = new CheckingProcessManager();
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Thread checkingThread = new Thread(() =>
            {
                _checkingProcessManager.Start(_obj, dispatcher, this);
            });
            checkingThread.Start();
        }

        public event EventHandler CanExecuteChanged;
    }
}
