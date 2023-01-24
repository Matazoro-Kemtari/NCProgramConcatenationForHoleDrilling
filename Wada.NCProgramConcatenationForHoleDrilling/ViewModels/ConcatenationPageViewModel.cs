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
using System.Threading.Tasks;
using System.Windows;
using Wada.AOP.Logging;
using Wada.EditNCProgramApplication;
using Wada.Extension;
using Wada.NCProgramConcatenationForHoleDrilling.Models;
using Wada.NCProgramConcatenationForHoleDrilling.Views;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;
using Wada.ReadMainNCProgramApplication;
using Wada.ReadMainNCProgramParametersApplication;
using Wada.ReadSubNCProgramApplication;
using Wada.UseCase.DataClass;

namespace Wada.NCProgramConcatenationForHoleDrilling.ViewModels
{
    public class ConcatenationPageViewModel : BindableBase, IDestructible, IDropTarget
    {
        private readonly ConcatenationPageModel _concatenation = new();
        private readonly IRegionNavigationService _regionNavigationService;
        private readonly IDialogService _dialogService;
        private readonly IReadMainNCProgramUseCase _readMainNCProgramUseCase;
        private readonly IReadSubNCProgramUseCase _readSubNCProgramUseCase;
        private readonly IReadMainNCProgramParametersUseCase _readMainNCProgramParametersUseCase;
        private readonly IEditNCProgramUseCase _editNCProgramUseCase;

        private IEnumerable<MainNCProgramCodeDTO>? _mainProgramCodes = null;

        private MainNCProgramParametersAttempt? _mainNCProgramParameters = null;

        public ConcatenationPageViewModel(IRegionNavigationService regionNavigationService, IDialogService dialogService, IReadMainNCProgramUseCase readMainNCProgramUseCase, IReadSubNCProgramUseCase readSubNCProgramUseCase, IReadMainNCProgramParametersUseCase readMainNCProgramParametersUseCase, IEditNCProgramUseCase editNCProgramUseCase)
        {
            _regionNavigationService = regionNavigationService;
            _dialogService = dialogService;
            _readMainNCProgramUseCase = readMainNCProgramUseCase;
            _readSubNCProgramUseCase = readSubNCProgramUseCase;
            _readMainNCProgramParametersUseCase = readMainNCProgramParametersUseCase;
            _editNCProgramUseCase = editNCProgramUseCase;

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
            .ToAsyncReactiveCommand()
            .WithSubscribe(() => MoveNextViewAsync())
            .AddTo(Disposables);

            ClearCommand = new ReactiveCommand()
                .WithSubscribe(() => _concatenation.Clear())
                .AddTo(Disposables);

            // メインプログラム読込
            _ = Task.Run(async () =>
            {
                try
                {
                    _mainProgramCodes = await _readMainNCProgramUseCase.ExecuteAsync();
                }
                catch (Exception ex) when (ex is NCProgramConcatenationServiceException || ex is InvalidOperationException)
                {
                    var message = MessageNotificationViaLivet.MakeErrorMessage(
                        $"リストの内容が正しくありません\n{ex.Message}");
                    await Messenger.RaiseAsync(message);
                    Environment.Exit(0);
                }
                catch (OpenFileStreamReaderException ex)
                {
                    var message = MessageNotificationViaLivet.MakeErrorMessage(
                        $"メインプログラムの内容を読み込もうとしましたが\n{ex.Message}");
                    await Messenger.RaiseAsync(message);
                    Environment.Exit(0);
                }

            });

            // リスト読み込み
            _ = Task.Run(async () =>
            {
                try
                {
                    var _dto = await _readMainNCProgramParametersUseCase.ExecuteAsync();
                    _mainNCProgramParameters = _dto != null
                    ? _dto.Convert()
                    : throw new NCProgramConcatenationForHoleDrillingException(
                        "リストを読み込もうとしましたが、失敗しました\n" +
                        "リストの内容を確認してください");
                }
                catch (NCProgramConcatenationServiceException ex)
                {
                    var message = MessageNotificationViaLivet.MakeErrorMessage(
                        $"リストの内容が正しくありません\n{ex.Message}");
                    await Messenger.RaiseAsync(message);
                    Environment.Exit(0);
                }
                catch (OpenFileStreamException ex)
                {
                    var message = MessageNotificationViaLivet.MakeErrorMessage(
                        $"リストの内容を読み込もうとしましたが\n{ex.Message}");
                    await Messenger.RaiseAsync(message);
                    Environment.Exit(0);
                }
            });
        }

        [Logging]
        private async Task MoveNextViewAsync()
        {
            if (_mainProgramCodes == null || _mainNCProgramParameters == null)
            {
                var message = MessageNotificationViaLivet.MakeInformationMessage(
                    "設定ファイルの準備ができていません\n" +
                    "数分待って実行してください\n" +
                    "数分待っても状況が変わらない場合は 上長に報告してください");
                await Messenger.RaiseAsync(message);
                return;
            }

            // メインプログラムを編集する
            var editedCodes = await _editNCProgramUseCase.ExecuteAsync(
                new EditNCProgramPram(
                    (DirectedOperationTypeAttempt)_concatenation.FetchedOperationType.Value,
                    _concatenation.SubProgramNumber.Value,
                    _concatenation.TargetToolDiameter.Value,
                    _mainProgramCodes.Where(x => x.MachineToolClassification == (MachineToolTypeAttempt)MachineTool.Value)
                                     .Select(x => x.NCProgramCodeAttempts)
                                     .First(),
                    (MaterialTypeAttempt)Material.Value,
                    (ReamerTypeAttempt)Reamer.Value,
                    decimal.Parse(Thickness.Value),
                    _mainNCProgramParameters));

            // 結合する

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
                _concatenation.TargetToolDiameter.Value = ncProcramCode.FetchTargetToolDiameter();
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

            _concatenation.SubProgramNumber.Value = Path.GetFileNameWithoutExtension(path);

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

        public ReactiveProperty<NCProgramConcatenationService.ValueObjects.DirectedOperationType> FetchedOperationType { get; }

        [Display(Name = "加工機")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<MachineToolType> MachineTool { get; }

        [Display(Name = "材質")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<MaterialType> Material { get; }

        [Display(Name = "リーマ")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<ReamerType> Reamer { get; }

        [Display(Name = "板厚")]
        [Required(ErrorMessage = "{0}を入力してください")]
        [RegularExpression(@"[0-9]+(\.[0-9]+)?", ErrorMessage = "半角の整数または小数を入力してください")]
        [Range(1, double.MaxValue, ErrorMessage = "{0}は{1:F}～{2:F}の範囲を入力してください")]
        public ReactiveProperty<string> Thickness { get; }

        public AsyncReactiveCommand NextViewCommand { get; }

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
