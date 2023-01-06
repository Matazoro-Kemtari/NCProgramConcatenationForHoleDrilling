using GongSolutions.Wpf.DragDrop;
using Livet.Messaging;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows;
using Wada.AOP.Logging;
using Wada.Extension;
using Wada.NCProgramConcatenationForHoleDrilling.Models;
using Wada.NCProgramConcatenationForHoleDrilling.Views;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.ReadMainNCProgramApplication;
using Wada.ReadSubNCProgramApplication;

namespace Wada.NCProgramConcatenationForHoleDrilling.ViewModels
{
    public class ConcatenationPageViewModel : BindableBase, IDestructible, IDropTarget
    {
        private readonly ConcatenationPageModel _concatenation = new();
        private readonly IRegionNavigationService _regionNavigationService;
        private readonly IDialogService _dialogService;
        private readonly IReadMainNCProgramUseCase _readMainNCProgramUseCase;
        private readonly IReadSubNCProgramUseCase _readSubNCProgramUseCase;

        private readonly List<string> _mainProgramNames = new()
        {
            "CD.txt",
            "DR.txt",
            "MENTORI.txt",
            "REAMER.txt",
            "TAP.txt",
        };
        private readonly Dictionary<string, NCProgramCode> _mainProgramCodes = new();

        public ConcatenationPageViewModel(IRegionNavigationService regionNavigationService, IDialogService dialogService, IReadMainNCProgramUseCase readMainNCProgramUseCase, IReadSubNCProgramUseCase readSubNCProgramUseCase)
        {
            _regionNavigationService = regionNavigationService;
            _dialogService = dialogService;
            _readMainNCProgramUseCase = readMainNCProgramUseCase;
            _readSubNCProgramUseCase = readSubNCProgramUseCase;

            NCProgramFileName = _concatenation
                .NCProgramFile
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .SetValidateAttribute(() => NCProgramFileName)
                .AddTo(Disposables);

            // ドラッグアンドドロップされて 値が書き換わったイベント
            _concatenation.NCProgramFile
                .Skip(1)
                .Where(x => x != null)
                .Subscribe(x => ChangeSubprogramPath(x!));

            ErrorMsgNCProgramFileName = NCProgramFileName
                .ObserveErrorChanged
                .Select(x => x?.Cast<string>().FirstOrDefault())
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposables);

            FetchedOperationType = _concatenation
                .FetchedOperationType
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .AddTo(Disposables);

            MachineTool = _concatenation
                .MachineTool
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .SetValidateAttribute(() => MachineTool)
                .AddTo(Disposables);

            ErrorMsgMachineTool = MachineTool
                .ObserveErrorChanged
                .Select(x => x?.Cast<string>().FirstOrDefault())
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposables);

            Material = _concatenation
                .Material
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .SetValidateAttribute(() => Material)
                .AddTo(Disposables);

            ErrorMsgMaterial = Material
                .ObserveErrorChanged
                .Select(x => x?.Cast<string>().FirstOrDefault())
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposables);

            Reamer = _concatenation
                .Reamer
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .SetValidateAttribute(() => Reamer)
                .AddTo(Disposables);

            ErrorMsgReamer = Reamer
                .ObserveErrorChanged
                .Select(x => x?.Cast<string>().FirstOrDefault())
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposables);

            Thickness = _concatenation
                .Thickness
                .ToReactivePropertyAsSynchronized(x => x.Value)
                .SetValidateAttribute(() => Thickness)
                .AddTo(Disposables);

            ErrorMsgThickness = Thickness
                .ObserveErrorChanged
                .Select(x => x?.Cast<string>().FirstOrDefault())
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposables);

            // コマンドボタンのbind
            NextViewCommand = new[]
            {
                NCProgramFileName.ObserveHasErrors,
                MachineTool.ObserveHasErrors,
                Material.ObserveHasErrors,
                FetchedOperationType.CombineLatest(
                    Reamer,
                    (x, y) => x == DirectedOperationType.Reaming && y == ReamerType.Undefined),
                Thickness.ObserveHasErrors,
            }
            .CombineLatestValuesAreAllFalse()
            .ToReactiveCommand()
            .WithSubscribe(() => MoveNextView())
            .AddTo(Disposables);

            ClearCommand = new ReactiveCommand()
                .WithSubscribe(() => _concatenation.Clear())
                .AddTo(Disposables);

            // メインプログラム読込
            _ = Task.Run(() =>
            {
                _mainProgramNames.ForEach(async x =>
                {
                    NCProgramCode ncCode = await _readMainNCProgramUseCase.ExecuteAsync(Path.Combine("メインプログラム", x).ToString());
                    _mainProgramCodes.Add(x, ncCode);
                });
            });
        }

        [Logging]
        private void MoveNextView()
        {
            switch (FetchedOperationType.Value)
            {
                case DirectedOperationType.Drilling:
                    
                    break;
                case DirectedOperationType.Reaming:
                    break;
                case DirectedOperationType.TapProcessing:
                    break;
                default:
                    break;
            }
        }

        [Logging]
        private async void ChangeSubprogramPath(string path)
        {
            if (path == null || path == string.Empty)
                return;

            // サブプログラムを読み込む
            NCProgramCode ncProcramCode = await _readSubNCProgramUseCase.ExecuteAsync(path);

            try
            {
                // 読み込んだサブプログラムの作業指示を取得する
                _concatenation.FetchedOperationType.Value = ncProcramCode.FetchOperationType();
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
                $"OperationTypeString={_concatenation.FetchedOperationType.Value.GetEnumDisplayName()}&SubProgramSource={ncProcramCode}");
            IDialogResult? dialogResult = default;
            _dialogService.ShowDialog(nameof(NotationContentConfirmationDialog),
                parameters,
                result => dialogResult = result);

            if (dialogResult == null || dialogResult.Result != ButtonResult.OK)
            {
                _concatenation.Clear();
                return;
            }
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

        public InteractionMessenger Messenger { get; } = new InteractionMessenger();

        [Display(Name = "サブプログラム")]
        [Required(ErrorMessage = "{0}をドラッグアンドドロップしてください")]
        public ReactiveProperty<string> NCProgramFileName { get; }

        public ReactiveProperty<DirectedOperationType> FetchedOperationType { get; }

        [Display(Name = "加工機")]
        [Range(1, double.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<MachineToolType> MachineTool { get; }

        [Display(Name = "材質")]
        [Range(1, double.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<MaterialType> Material { get; }

        [Display(Name = "リーマ")]
        [Range(1, double.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<ReamerType> Reamer { get; }

        [Display(Name = "板厚")]
        [Required(ErrorMessage = "{0}を入力してください")]
        [RegularExpression(@"[0-9]+(\.[0-9]+)?", ErrorMessage = "半角の整数または小数を入力してください")]
        [Range(1, double.MaxValue, ErrorMessage = "{0}は{1:F}～{2:F}の範囲を入力してください")]
        public ReactiveProperty<string> Thickness { get; }

        public ReactiveCommand NextViewCommand { get; }

        public ReactiveCommand ClearCommand { get; }

        public ReadOnlyReactivePropertySlim<string?> ErrorMsgNCProgramFileName { get; }
        public ReadOnlyReactivePropertySlim<string?> ErrorMsgMachineTool { get; }
        public ReadOnlyReactivePropertySlim<string?> ErrorMsgMaterial { get; }
        public ReadOnlyReactivePropertySlim<string?> ErrorMsgThickness { get; }
        public ReadOnlyReactivePropertySlim<string?> ErrorMsgReamer { get; }
    }

    public enum MachineToolType
    {
        Undefined,
        RB250F,
        RB260,
        Triaxial,
    }
    public enum MaterialType
    {
        Undefined,
        Aluminum,
        Iron,
    }
    public enum ReamerType
    {
        Undefined,
        Crystal,
        Skill
    }
}
