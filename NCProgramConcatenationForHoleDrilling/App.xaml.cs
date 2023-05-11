﻿using NcProgramConcatenationForHoleDrilling.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Wada.CombineMainNcProgramApplication;
using Wada.EditNcProgramApplication;
using Wada.MainProgramPrameterSpreadSheet;
using Wada.NcProgramConcatenationForHoleDrilling;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.MainProgramCombiner;
using Wada.NcProgramConcatenationService.ParameterRewriter;
using Wada.NcProgramFile;
using Wada.ReadMainNcProgramApplication;
using Wada.ReadMainNcProgramParametersApplication;
using Wada.ReadSubNcProgramApplication;
using Wada.StoreNcProgramCodeApplication;

namespace NcProgramConcatenationForHoleDrilling
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
            _ = containerRegistry.Register<INcProgramRepository, NCProgramRepository>();
            _ = containerRegistry.Register<IStreamReaderOpener, StreamReaderOpener>();
            _ = containerRegistry.Register<IReadMainNcProgramUseCase, ReadMainNcProgramUseCase>();
            _ = containerRegistry.Register<IReadSubNcProgramUseCase, ReadSubNcProgramUseCase>();

            // パラメーターリスト読み込み
            _ = containerRegistry.Register<IStreamOpener, StreamOpener>();
            _ = containerRegistry.Register<ReamingPrameterReader>();
            _ = containerRegistry.Register<TappingPrameterReader>();
            _ = containerRegistry.Register<DrillingParameterReader>();
            _ = containerRegistry.Register<IDrillSizeDataReader, DrillSizeDataReader>();
            _ = containerRegistry.Register<IReadMainNcProgramParametersUseCase, ReadMainNcProgramParametersUseCase>();

            // メインプログラムの編集
            _ = containerRegistry.Register<CrystalReamingParameterRewriter>();
            _ = containerRegistry.Register<SkillReamingParameterRewriter>();
            _ = containerRegistry.Register<TappingParameterRewriter>();
            _ = containerRegistry.Register<DrillingParameterRewriter>();
            _ = containerRegistry.Register<IEditNcProgramUseCase, EditNcProgramUseCase>();

            // メインプログラムの結合
            _ = containerRegistry.Register<IMainProgramCombiner, MainProgramCombiner>();
            _ = containerRegistry.Register<ICombineMainNcProgramUseCase, CombineMainNcProgramUseCase>();

            // メインプログラムの保存
            _ = containerRegistry.Register<IStreamWriterOpener, StreamWriterOpener>();
            _ = containerRegistry.Register<IStoreNcProgramCodeUseCase, StoreNcProgramCodeUseCase>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            // Moduleを読み込む
            moduleCatalog.AddModule<NcProgramConcatenationForHoleDrillingModule>(InitializationMode.WhenAvailable);
        }
    }
}
