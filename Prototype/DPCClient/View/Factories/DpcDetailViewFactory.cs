using System;
using DPCClient.Model;
using DPCClient.Properties;
using DPCClient.ViewModel;

namespace DPCClient.View.Factories
{
    class DpcDetailViewFactory : IWindowFactory
    {
        public void CreateNewWindow(object model = null)
        {
            NLogMessage nLogMessage = (NLogMessage)model;
            DpcDetailViewModel viewModel = new DpcDetailViewModel();
            viewModel.NLogMessage = nLogMessage;
            DpcDetailView window = new DpcDetailView{
                DataContext = viewModel
            };
            
            window.Show();
        }
    }
}
