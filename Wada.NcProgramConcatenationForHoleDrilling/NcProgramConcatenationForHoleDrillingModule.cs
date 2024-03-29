﻿using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wada.NcProgramConcatenationForHoleDrilling.Views;

namespace Wada.NcProgramConcatenationForHoleDrilling
{
    public class NcProgramConcatenationForHoleDrillingModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager?.RequestNavigate("ContentRegion", nameof(ConcatenationPage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // ViewをDIコンテナに登録する
            containerRegistry.RegisterDialog<NotationContentConfirmationDialog>();
            containerRegistry.RegisterForNavigation<ConcatenationPage>();
            containerRegistry.RegisterForNavigation<PreviewPage>();
        }
    }
}