using System;
using System.IO;
using System.Windows.Input;
using CodeInstrumentation;
using DPCClient.Model;
using DPCClient.View.Factories;

namespace DPCClient.ViewModel
{
    class ListClickCommand : ICommand
    {
        private readonly DpcViewModel _obj;
        private readonly IWindowFactory _windowFactory;
        public ListClickCommand(DpcViewModel obj)
        {
            _obj = obj;
            _windowFactory = new DpcDetailViewFactory();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            NLogMessage entry = (NLogMessage) parameter;
            // ReSharper disable once AssignNullToNotNullAttribute
            string path = Path.Combine(Directory.GetCurrentDirectory(), "work_decompile", Path.GetFileName(_obj.FilePath));
            entry.MethodContent =
                CodeInstrumentator.DecompileCode(path, NLogMessage.GetTypeName(entry.MethodName), entry.MethodName, entry.RowCount);
            entry.ConflictContent =
                CodeInstrumentator.DecompileCode(path, NLogMessage.GetTypeName(entry.ConflictMethodName),
                    entry.ConflictMethodName, entry.ConflictRow);
            _windowFactory.CreateNewWindow(entry);
        }

        public event EventHandler CanExecuteChanged;
    }
}
