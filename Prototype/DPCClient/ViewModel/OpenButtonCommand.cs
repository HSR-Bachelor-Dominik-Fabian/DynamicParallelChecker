using System;
using System.Windows.Input;

namespace DPCClient.ViewModel
{
    class OpenButtonCommand : ICommand
    {
        private readonly DpcViewModel _obj;
        public OpenButtonCommand(DpcViewModel obj)
        {
            _obj = obj;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "Executable (*.exe)|*.exe"
            };
            
            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                string fileName = fileDialog.FileName;
                _obj.FilePath = fileName;
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
