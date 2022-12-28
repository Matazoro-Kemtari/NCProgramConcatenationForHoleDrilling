﻿using NCProgramConcatenationForHoleDrilling.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Wada.NCProgramConcatenationForHoleDrilling;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramFile;
using Wada.ReadSubNCProgramApplication;

namespace NCProgramConcatenationForHoleDrilling
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // NCプログラム読み込み
            _ = containerRegistry.Register<INCProgramRepository, NCProgramRepository>();
            _ = containerRegistry.Register<IStreamReaderOpener, StreamReaderOpener>();
            _ = containerRegistry.Register<IReadSubNCProgramUseCase, ReadSubNCProgramUseCase>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            // Moduleを読み込む
            moduleCatalog.AddModule<NCProgramConcatenationForHoleDrillingModule>(InitializationMode.WhenAvailable);
        }
    }
}