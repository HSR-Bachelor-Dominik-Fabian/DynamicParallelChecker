using DPCClient.Model;
using DPCClient.ViewModel;

namespace DPCClient.View.Factories
{
    class DpcDetailViewFactory : IWindowFactory
    {
        public void CreateNewWindow(object model = null)
        {
            NLogMessage nLogMessage = (NLogMessage)model;
            DpcDetailViewModel viewModel = new DpcDetailViewModel {NLogMessage = nLogMessage};
            DpcDetailView window = new DpcDetailView{
                DataContext = viewModel
            };
            
            window.Show();
        }
    }
}
