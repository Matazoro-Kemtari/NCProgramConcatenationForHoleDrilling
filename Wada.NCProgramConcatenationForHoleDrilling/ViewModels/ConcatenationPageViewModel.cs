using GongSolutions.Wpf.DragDrop;
using Livet.Messaging;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Wada.AOP.Logging;
using Wada.Extension;
using Wada.NCProgramConcatenationForHoleDrilling.Models;
using Wada.NCProgramConcatenationForHoleDrilling.Views;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.ReadSubNCProgramApplication;

namespace Wada.NCProgramConcatenationForHoleDrilling.ViewModels
{
    public class ConcatenationPageViewModel : BindableBase, IDestructible, IDropTarget
    {
        private readonly Concatenation _concatenation = new();
        private readonly IDialogService _dialogService;
        private readonly IReadSubNCProgramUseCase _readSubNCProgramUseCase;

        public ConcatenationPageViewModel(IDialogService dialogService, IReadSubNCProgramUseCase readSubNCProgramUseCase)
        {
            _dialogService = dialogService;
            _readSubNCProgramUseCase = readSubNCProgramUseCase;

            NCProgramFileName = _concatenation
                .NCProgramFile
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposables);

            // ドラッグアンドドロップされて 値が書き換わったイベント
            _concatenation.NCProgramFile.Skip(1).Subscribe(x => ChangeSubprogramPath(x));
        }

        [Logging]
        private async void ChangeSubprogramPath(string path)
        {
            if (path == null || path == string.Empty)
                return;

            // サブプログラムを読み込む
            NCProgramCode ncProcramCode = await _readSubNCProgramUseCase.ExecuteAsync(path);

            // 読み込んだサブプログラムの作業指示を取得する
            OperationType operationType;
            try
            {
                operationType = ncProcramCode.FetchOperationType();
            }
            catch (NCProgramConcatenationServiceException ex)
            {
                _concatenation.Clear();

                var message = MessageNotificationViaLivet.MakeExclamationMessage(
                    ex.Message);
                await Messenger.RaiseAsync(message);
                return;
            }

            IDialogParameters parameters = new DialogParameters(
                $"OperationTypeString={operationType.GetEnumDisplayName()}&SubProgramSource={ncProcramCode}");
            IDialogResult? dialogResult = default;
            _dialogService.ShowDialog(nameof(NotationContentConfirmationDialog),
                parameters,
                result => dialogResult = result);

            if (dialogResult == null || dialogResult.Result != ButtonResult.OK)
                return;
        }

        [Logging]
        public void DragOver(IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = dragFileList.Any(x =>
            {
                return Path.GetExtension(x) == string.Empty;
            }) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        [Logging]
        public void Drop(IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = dragFileList.Any(x =>
            {
                return Path.GetExtension(x) == string.Empty;
            }) ? DragDropEffects.Copy : DragDropEffects.None;

            _concatenation.NCProgramFile.Value =
                dragFileList.FirstOrDefault(x => Path.GetExtension(x) == string.Empty) ?? string.Empty;
        }

        public void Destroy() => Disposables.Dispose();

        /// <summary>
        /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        /// </summary>
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public ReactivePropertySlim<string> NCProgramFileName { get; }

        public InteractionMessenger Messenger { get; } = new InteractionMessenger();
    }
}
