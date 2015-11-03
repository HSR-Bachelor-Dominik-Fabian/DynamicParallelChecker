using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private bool _isReadyForChecking;

        public StartParallelCheckerButtonCommand(DpcViewModel obj)
        {
            _obj = obj;
            _isReadyForChecking = false;
        }

        public bool CanExecute(object parameter)
        {
            return _isReadyForChecking;
        }

        public bool IsReadyForChecking
        {
            get { return _isReadyForChecking; }
            set
            {
                _isReadyForChecking = value;
                CanExecuteChanged?.Invoke(this, new PropertyChangedEventArgs("StartCheckerButton"));
            }
        }

        public void Execute(object parameter)
        {
            IsReadyForChecking = false;
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
