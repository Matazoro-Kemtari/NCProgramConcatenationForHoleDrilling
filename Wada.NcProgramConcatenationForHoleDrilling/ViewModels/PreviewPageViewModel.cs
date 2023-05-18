using Livet.Messaging;
using Livet.Messaging.IO;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationForHoleDrilling.Models;
using Wada.NcProgramConcatenationForHoleDrilling.Views;
using Wada.NcProgramConcatenationService;

namespace Wada.NcProgramConcatenationForHoleDrilling.ViewModels
{
    public class PreviewPageViewModel : BindableBase, INavigationAware, IDestructible
    {
        private readonly PreviewPageModel _previewPageModel = new();
        private IRegionNavigationService? _regionNavigationService;
        private readonly IStreamWriterOpener _streamWriterOpener;
        private readonly INcProgramReadWriter _ncProgramReadWriter;

        public PreviewPageViewModel(IStreamWriterOpener streamWriterOpener, INcProgramReadWriter ncProgramReadWriter)
        {
            _streamWriterOpener = streamWriterOpener;
            _ncProgramReadWriter = ncProgramReadWriter;

            CombinedProgramSource = _previewPageModel
                .CombinedProgramSource
                .AddTo(Disposables);

            ExecCommand = new AsyncReactiveCommand()
                .WithSubscribe(() => SaveNcProgramCodeAsync())
                .AddTo(Disposables);

            PreviousViewCommand = new DelegateCommand(
                () => _regionNavigationService?.Journal.GoBack(),
                () => _regionNavigationService?.Journal?.CanGoBack ?? false);
        }

        [Logging]
        private async Task SaveNcProgramCodeAsync()
        {
            var message = MessageNotificationViaLivet.MakeSaveFileDialog();
            await Messenger.RaiseAsync(message);

            // 画面遷移
            _regionNavigationService.RequestNavigate(nameof(ConcatenationPage));
        }

        /// <summary>
        /// Livet.SavingFileSelectionDialogを呼び出した後のコールバック関数
        /// 非同期だがコールバック関数のためvoid
        /// </summary>
        /// <param name="message"></param>
        public async void SaveDialogClosed(SavingFileSelectionMessage message)
        {
            if (message.Response == null)
                // キャンセル
                return;

            var savingFilePath = message.Response[0];
            using var writer = _streamWriterOpener.Open(savingFilePath);
            await _ncProgramReadWriter.WriteAllAsync(writer, _previewPageModel.CombinedProgramSource.Value);
        }

        public void Destroy() => Disposables.Dispose();

        /// <summary>表示するViewを判別します</summary>
        /// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。
        /// いろいろな画面に遷移した際に前回の値を記憶させるかどうかを決める 記憶させる場合はTrue、毎回新しく表示させたい場合はFalse</param>
        /// <returns>表示するViewかどうかを表すbool。</returns>
        [Logging]
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        /// <summary>別のViewに切り替わる前に呼び出されます。</summary>
        /// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。</param>
        [Logging]
        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        /// <summary>Viewを表示した後呼び出されます。</summary>
        /// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。</param>
        [Logging]
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _regionNavigationService = navigationContext.NavigationService;
            PreviousViewCommand.RaiseCanExecuteChanged();

            string combinedCode = navigationContext.Parameters.GetValue<string>(nameof(combinedCode));
            _previewPageModel.CombinedProgramSource.Value = combinedCode;
        }

        /// <summary>
        /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        /// </summary>
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public InteractionMessenger Messenger { get; } = new InteractionMessenger();

        public ReactivePropertySlim<string> CombinedProgramSource { get; }

        public AsyncReactiveCommand ExecCommand { get; }

        public DelegateCommand PreviousViewCommand { get; }
    }
}
