using GongSolutions.Wpf.DragDrop;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using Wada.NCProgramConcatenationForHoleDrilling.Models;

namespace Wada.NCProgramConcatenationForHoleDrilling.ViewModels
{
    public class ConcatenationPageViewModel : BindableBase, IDestructible, IDropTarget
    {
        private readonly Concatenation _concatenation = new();

        public ConcatenationPageViewModel()
        {
            NCProgramFileName = _concatenation
                .NCProgramFile
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposables);
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
