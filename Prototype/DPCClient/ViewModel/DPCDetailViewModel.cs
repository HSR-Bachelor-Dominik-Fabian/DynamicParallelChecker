using System.ComponentModel;
using System.Runtime.CompilerServices;
using DPCClient.Model;
using DPCClient.Process;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

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
                CreateClassContent();
            }
        }

        private TextDocument _mainClassCode;
        public TextDocument MainClassCode
        {
            get
            {
                return _mainClassCode;
            }
            set
            {
                _mainClassCode = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("MainClassCode"));
            }
        }

        private TextDocument _confilctingClassCode;
        public TextDocument ConflictingClassCode
        {
            get
            {
                return _confilctingClassCode;
            }
            set
            {
                _confilctingClassCode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConflictingClassCode"));
            }
        }

        public DpcDetailViewModel()
        {
            NLogMessage = new NLogMessage();
            MainClassCode = new TextDocument();
        }

        private void CreateClassContent()
        {
            MainClassCode = NLogMessage.MethodContent != null ? new TextDocument {Text = NLogMessage.MethodContent} : null;
            ConflictingClassCode = NLogMessage.ConflictContent != null ? new TextDocument { Text = NLogMessage.ConflictContent } : null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
