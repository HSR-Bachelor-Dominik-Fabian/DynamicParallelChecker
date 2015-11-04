using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DPCClient.Model;

namespace DPCClient.ViewModel
{
    class DpcViewModel : INotifyPropertyChanged , INotifyCollectionChanged
    {
        private readonly ListClickCommand _listClickCommand;
        private readonly OpenButtonCommand _openButtonCommand;
        private readonly FilePathModel _filePathModel;
        private readonly ObservableCollection<NLogMessage> _logEntryModels;
        private readonly StartParallelCheckerButtonCommand _startParallelCheckerButtonCommand;

        public DpcViewModel()
        {
            _listClickCommand = new ListClickCommand();
            _openButtonCommand = new OpenButtonCommand(this);
            _filePathModel = new FilePathModel("");
            _logEntryModels = new ObservableCollection<NLogMessage>();
            _startParallelCheckerButtonCommand = new StartParallelCheckerButtonCommand(this);
        }

        public ICommand OpenBtnClick => _openButtonCommand;
        public ICommand ListClickComm => _listClickCommand;
        public string FilePath
        {
            get { return _filePathModel.FilePath; }
            set
            {
                _filePathModel.FilePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilePath"));
                if (!string.IsNullOrWhiteSpace(value) && File.Exists(value))
                {
                    _startParallelCheckerButtonCommand.IsReadyForChecking = true;
                }
                else
                {
                    _startParallelCheckerButtonCommand.IsReadyForChecking = false;
                }
            }
        }

        public FilePathModel FilePathModel => _filePathModel;

        public ObservableCollection<NLogMessage> LogEntries
        {
            get { return _logEntryModels; }
            set
            {
                _logEntryModels.Clear();
                foreach (NLogMessage message in value)
                {
                    _logEntryModels.Add(message);
                }
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddLogEntry(NLogMessage entry)
        {
            _logEntryModels.Add(entry);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }

        public ICommand StartParallelCheckerBtnClick => _startParallelCheckerButtonCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
