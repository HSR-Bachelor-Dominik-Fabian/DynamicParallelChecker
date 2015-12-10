using DPCClient.Model;
using DPCClient.ViewModel;

namespace DPCClient.View.Factories
{
    class DpcDetailViewFactory : IWindowFactory
    {
        public void CreateNewWindow(object model = null)
        {
            var nLogMessage = (NLogMessage)model;
            var viewModel = new DpcDetailViewModel {NLogMessage = nLogMessage};
            var window = new DpcDetailView{
                DataContext = viewModel
            };
            
            window.Show();
        }
    }
}
