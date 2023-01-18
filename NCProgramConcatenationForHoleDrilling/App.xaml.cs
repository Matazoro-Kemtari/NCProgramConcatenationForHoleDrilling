﻿using NCProgramConcatenationForHoleDrilling.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Wada.EditNCProgramApplication;
using Wada.MainProgramPrameterSpreadSheet;
using Wada.NCProgramConcatenationForHoleDrilling;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramFile;
using Wada.ReadMainNCProgramApplication;
using Wada.ReadMainNCProgramParametersApplication;
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
            _ = containerRegistry.Register<IReadMainNCProgramUseCase, ReadMainNCProgramUseCase>();
            _ = containerRegistry.Register<IReadSubNCProgramUseCase, ReadSubNCProgramUseCase>();

            // パラメーターリスト読み込み
            _ = containerRegistry.Register<IStreamOpener, StreamOpener>();
            _ = containerRegistry.Register<ReamingPrameterRepository>();
            _ = containerRegistry.Register<TappingPrameterRepository>();
            _ = containerRegistry.Register<DrillingParameterRepositoy>();
            _ = containerRegistry.Register<IReadMainNCProgramParametersUseCase, ReadMainNCProgramParametersUseCase>();

            // メインプログラムの編集
            _ = containerRegistry.Register<IEditNCProgramUseCase, EditNCProgramUseCase>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            // Moduleを読み込む
            moduleCatalog.AddModule<NCProgramConcatenationForHoleDrillingModule>(InitializationMode.WhenAvailable);
        }
    }
}
