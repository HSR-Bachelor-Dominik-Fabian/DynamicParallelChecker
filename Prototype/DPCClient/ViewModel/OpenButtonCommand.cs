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
            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "Executable (*.exe)|*.exe"
            };
            
            var result = fileDialog.ShowDialog();

            if (result == true)
            {
                _obj.FilePath = fileDialog.FileName;
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
