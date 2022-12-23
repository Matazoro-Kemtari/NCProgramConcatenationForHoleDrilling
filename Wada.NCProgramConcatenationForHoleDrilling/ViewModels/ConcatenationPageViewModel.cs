using GongSolutions.Wpf.DragDrop;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationForHoleDrilling.Models;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.ReadSubNCProgramApplication;

namespace Wada.NCProgramConcatenationForHoleDrilling.ViewModels
{
    public class ConcatenationPageViewModel : BindableBase, IDestructible, IDropTarget
    {
        private readonly Concatenation _concatenation = new();
        private readonly IReadSubNCProgramUseCase _readSubNCProgramUseCase;

        public ConcatenationPageViewModel(IReadSubNCProgramUseCase readSubNCProgramUseCase)
        {
            _readSubNCProgramUseCase = readSubNCProgramUseCase;

            NCProgramFileName = _concatenation
                .NCProgramFile
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposables);
            // TODO: ここでファイルを展開する処理を入れる
            _concatenation.NCProgramFile.Subscribe([Logging] async (x) =>
            {
                if (x == null)
                    return;

                NCProgramCode ncProcramCode;
                try
                {
                    ncProcramCode = await _readSubNCProgramUseCase.ExecuteAsync(x);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationForHoleDrillingException(ex.Message, ex);
                }
                MessageBox.Show(x);
            });
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = dragFileList.Any(x =>
            {
                return Path.GetExtension(x) == string.Empty;
            }) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = dragFileList.Any(x =>
            {
                return Path.GetExtension(x) == string.Empty;
            }) ? DragDropEffects.Copy : DragDropEffects.None;

            _concatenation.NCProgramFile.Value =
                dragFileList.FirstOrDefault(x => Path.GetExtension(x) == string.Empty);
        }

        public void Destroy() => Disposables.Dispose();

        /// <summary>
        /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        /// </summary>
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public ReactivePropertySlim<string> NCProgramFileName { get; }
    }
}
