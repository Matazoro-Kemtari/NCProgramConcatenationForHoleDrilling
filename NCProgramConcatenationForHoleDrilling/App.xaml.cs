using NCProgramConcatenationForHoleDrilling.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
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
            _ = containerRegistry.Register<IReamingPrameterRepository, ReamingPrameterRepository>();
            _ = containerRegistry.Register<ITappingPrameterRepository, TappingPrameterRepository>();
            _ = containerRegistry.Register<IReadMainNCProgramParametersUseCase, ReadMainNCProgramParametersUseCase>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            // Moduleを読み込む
            moduleCatalog.AddModule<NCProgramConcatenationForHoleDrillingModule>(InitializationMode.WhenAvailable);
        }
    }
}
