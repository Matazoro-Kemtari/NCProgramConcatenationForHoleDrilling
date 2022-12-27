using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wada.NCProgramConcatenationForHoleDrilling.Views;

namespace Wada.NCProgramConcatenationForHoleDrilling
{
    public class NCProgramConcatenationForHoleDrillingModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager?.RegisterViewWithRegion("ContentRegion", typeof(ConcatenationPage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<NotationContentConfirmationDialog>();
        }
    }
}