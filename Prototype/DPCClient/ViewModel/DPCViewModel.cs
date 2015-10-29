using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DPCClient.Model;

namespace DPCClient.ViewModel
{
    class DPCViewModel : INotifyPropertyChanged , INotifyCollectionChanged
    {
        private readonly OpenButtonCommand _openButtonCommand;
        private readonly FilePathModel _filePathModel;
        private ObservableCollection<NLogMessage> _logEntryModels;
        private readonly StartParallelCheckerButtonCommand _startParallelCheckerButtonCommand;


        public DPCViewModel()
        {
            _openButtonCommand = new OpenButtonCommand(this);
            _filePathModel = new FilePathModel("");
            _logEntryModels = new ObservableCollection<NLogMessage>();
            _startParallelCheckerButtonCommand = new StartParallelCheckerButtonCommand(this);
        }

        public ICommand OpenBtnClick => _openButtonCommand;

        public string FilePath
        {
            get { return _filePathModel.FilePath; }
            set
            {
                _filePathModel.FilePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilePath"));
            }
        }

        public FilePathModel FilePathModel => _filePathModel;

        public ObservableCollection<NLogMessage> LogEntry
        {
            get { return _logEntryModels; }
            set
            {
                _logEntryModels = value;
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
