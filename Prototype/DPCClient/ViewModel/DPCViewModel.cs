using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using DPCClient.Model;

namespace DPCClient.ViewModel
{
    class DPCViewModel : INotifyPropertyChanged , INotifyCollectionChanged
    {
        private readonly OpenButtonCommand _openButtonCommand;
        private readonly FilePathModel _filePathModel;
        private ObservableCollection<LogEntryModel> _logEntryModels;
        private readonly StartParallelCheckerButtonCommand _startParallelCheckerButtonCommand;


        public DPCViewModel()
        {
            _openButtonCommand = new OpenButtonCommand(this);
            _filePathModel = new FilePathModel("");
            _logEntryModels = new ObservableCollection<LogEntryModel>();
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

        public ObservableCollection<LogEntryModel> LogEntry
        {
            get { return _logEntryModels; }
            set
            {
                _logEntryModels = value;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void AddLogEntry(LogEntryModel entry)
        {
            _logEntryModels.Add(entry);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }

        public ICommand StartParallelCheckerBtnClick => _startParallelCheckerButtonCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
