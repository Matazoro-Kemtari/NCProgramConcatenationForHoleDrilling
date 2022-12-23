using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using Wada.NCProgramConcatenationForHoleDrilling.Models;

namespace Wada.NCProgramConcatenationForHoleDrilling.ViewModels
{
    public class NotationContentConfirmationDialogViewModel : BindableBase, IDialogAware, IDestructible
    {
        private readonly NotationContentConfirmation _notationContentConfirmation = new();
        
        public NotationContentConfirmationDialogViewModel()
        {
            OperationTypeString = _notationContentConfirmation
                .OperationTypeString
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposables);

            SubProgramSource = _notationContentConfirmation
                .SubProgramSource
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposables);

            ExecCommand = new ReactiveCommand()
                .WithSubscribe(() =>
                    // ダイアログクローズイベントをキック
                    RequestClose?.Invoke(new DialogResult(ButtonResult.OK)))
                .AddTo(Disposables);

            CancelCommand = new ReactiveCommand()
                .WithSubscribe(() =>
                    // ダイアログクローズイベントをキック
                    RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)))
                .AddTo(Disposables);
        }

        public string Title => "注記内容確認";
        public event Action<IDialogResult> RequestClose;
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            _notationContentConfirmation.OperationTypeString.Value = parameters.GetValue<string>(nameof(OperationTypeString));
            _notationContentConfirmation.SubProgramSource.Value = parameters.GetValue<string>(nameof(SubProgramSource));
        }

        /// <summary>オブジェクトを破棄します</summary>
        public void Destroy() => Disposables.Dispose();

        /// <summary>
        /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        /// </summary>
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public ReactivePropertySlim<string> OperationTypeString { get; }

        public ReactivePropertySlim<string> SubProgramSource { get; }

        public ReactiveCommand ExecCommand { get; }

        public ReactiveCommand CancelCommand { get; }
    }
}
