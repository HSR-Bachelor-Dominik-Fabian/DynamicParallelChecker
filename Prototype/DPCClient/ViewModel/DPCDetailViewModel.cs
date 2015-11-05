using System.ComponentModel;
using System.Runtime.CompilerServices;
using DPCClient.Model;

namespace DPCClient.ViewModel
{
    public class DpcDetailViewModel:INotifyPropertyChanged
    {
        private NLogMessage _nLogMessage;
        public NLogMessage NLogMessage
        {
            get
            {
                return _nLogMessage;
            }
            set
            {
                _nLogMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NLogMessage"));
            }
        }

        public DpcDetailViewModel()
        {
            NLogMessage = new NLogMessage();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
