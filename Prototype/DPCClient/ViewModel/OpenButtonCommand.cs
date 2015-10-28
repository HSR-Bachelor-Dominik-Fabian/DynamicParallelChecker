using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DPCClient.ViewModel
{
    class OpenButtonCommand : ICommand
    {
        private DPCViewModel _obj;
        public OpenButtonCommand(DPCViewModel obj)
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
                Filter = "DLL Assembly (*.dll)|*.dll|Executable (*.exe)|*.exe"
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
