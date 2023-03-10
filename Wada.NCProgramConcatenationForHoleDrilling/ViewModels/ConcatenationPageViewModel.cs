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
using Wada.CombineMainNCProgramApplication;
using Wada.EditNCProgramApplication;
using Wada.Extension;
using Wada.NCProgramConcatenationForHoleDrilling.Models;
using Wada.NCProgramConcatenationForHoleDrilling.Views;
using Wada.ReadMainNCProgramApplication;
using Wada.ReadMainNCProgramParametersApplication;
using Wada.ReadSubNCProgramApplication;
using Wada.UseCase.DataClass;

namespace Wada.NCProgramConcatenationForHoleDrilling.ViewModels
{
    public class ConcatenationPageViewModel : BindableBase, INavigationAware, IDestructible, IDropTarget
    {
        private readonly ConcatenationPageModel _concatenation = new();
        private IRegionNavigationService? _regionNavigationService;
        private readonly IDialogService _dialogService;
        private readonly IReadMainNCProgramUseCase _readMainNCProgramUseCase;
        private readonly IReadSubNCProgramUseCase _readSubNCProgramUseCase;
        private readonly IReadMainNCProgramParametersUseCase _readMainNCProgramParametersUseCase;
        private readonly IEditNCProgramUseCase _editNCProgramUseCase;
        private readonly ICombineMainNCProgramUseCase _combineMainNCProgramUseCase;

        private IEnumerable<MainNCProgramCodeDTO>? _mainProgramCodes = null;

        private MainNCProgramParametersAttempt? _mainNCProgramParameters = null;

        public ConcatenationPageViewModel(IDialogService dialogService, IReadMainNCProgramUseCase readMainNCProgramUseCase, IReadSubNCProgramUseCase readSubNCProgramUseCase, IReadMainNCProgramParametersUseCase readMainNCProgramParametersUseCase, IEditNCProgramUseCase editNCProgramUseCase, ICombineMainNCProgramUseCase combineMainNCProgramUseCase)
        {
            _dialogService = dialogService;
            _readMainNCProgramUseCase = readMainNCProgramUseCase;
            _readSubNCProgramUseCase = readSubNCProgramUseCase;
            _readMainNCProgramParametersUseCase = readMainNCProgramParametersUseCase;
            _editNCProgramUseCase = editNCProgramUseCase;
            _combineMainNCProgramUseCase = combineMainNCProgramUseCase;

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
                    (x, y) => x == DirectedOperationTypeAttempt.Reaming && y == ReamerTypeAttempt.Undefined),
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
                catch (ReadMainNCProgramApplicationException ex)
                {
                    var message = MessageNotificationViaLivet.MakeErrorMessage(ex.Message);
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
                catch (ReadMainNCProgramParametersApplicationException ex)
                {
                    var message = MessageNotificationViaLivet.MakeErrorMessage(ex.Message);
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
            EditNCProgramDTO editedCodes;
            try
            {
                editedCodes = await _editNCProgramUseCase.ExecuteAsync(
                    new EditNCProgramPram(
                        _concatenation.FetchedOperationType.Value,
                        _concatenation.SubProgramNumber.Value,
                        _concatenation.DirectedOperationToolDiameter.Value,
                        _mainProgramCodes.Where(x => x.MachineToolClassification == MachineTool.Value)
                                         .Select(x => x.NCProgramCodeAttempts)
                                         .First(),
                        Material.Value,
                        Reamer.Value,
                        decimal.Parse(Thickness.Value),
                        _mainNCProgramParameters));
            }
            catch (EditNCProgramApplicationException ex)
            {
                var message = MessageNotificationViaLivet.MakeErrorMessage(
                    "メインプログラム編集中にエラーが発生しました\n" +
                    $"{ex.Message}");
                await Messenger.RaiseAsync(message);
                return;
            }

            // 結合する
            CombineMainNCProgramParam combineParam = new(
                editedCodes.NCProgramCodes,
                _concatenation.MachineTool.Value,
                _concatenation.Material.Value);
            var combinedCode = await _combineMainNCProgramUseCase.ExecuteAsync(combineParam);

            // 画面遷移
            var navigationParams = new NavigationParameters
            {
                { nameof(combinedCode), combinedCode.NCProgramCode.ToString() }
            };
            _regionNavigationService.RequestNavigate(nameof(PreviewPage), navigationParams);
        }

        /// <summary>
        /// Drag＆Dropして ファイルパスが変わった処理
        /// </summary>
        /// <param name="path"></param>
        [Logging]
        private async void ChangeSubprogramPath(string path)
        {
            if (path == null || path == string.Empty)
                return;

            // サブプログラムを読み込む
            SubNCProgramCodeAttemp subNCProcramCode;
            try
            {
                subNCProcramCode = await _readSubNCProgramUseCase.ExecuteAsync(path);
            }
            catch (ReadSubNCProgramApplicationException ex)
            {
                var message = MessageNotificationViaLivet.MakeInformationMessage(
                    $"サブプログラムの読み込みでエラーが発生しました\n{ex.Message}");
                await Messenger.RaiseAsync(message);
                _concatenation.Clear();

                return;
            }

            // 読み込んだサブプログラムの作業指示を取得する
            _concatenation.FetchedOperationType.Value = subNCProcramCode.DirectedOperationClassification;
            _concatenation.DirectedOperationToolDiameter.Value = subNCProcramCode.DirectedOperationToolDiameter;
            _concatenation.SubProgramNumber.Value = subNCProcramCode.ProgramName;

            IDialogParameters parameters = new DialogParameters(
                $"OperationTypeString={_concatenation.FetchedOperationType.Value.GetEnumDisplayName()}&SubProgramSource={subNCProcramCode}");
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

        [Logging]
        public void Destroy() => Disposables.Dispose();

        /// <summary>Viewを表示した後呼び出されます。</summary>
        /// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。</param>
        public void OnNavigatedTo(NavigationContext navigationContext) => _regionNavigationService = navigationContext.NavigationService;

        /// <summary>表示するViewを判別します</summary>
        /// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。
        /// いろいろな画面に遷移した際に前回の値を記憶させるかどうかを決める 記憶させる場合はTrue、毎回新しく表示させたい場合はFalse</param>
        /// <returns>表示するViewかどうかを表すbool。</returns>
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        /// <summary>別のViewに切り替わる前に呼び出されます。</summary>
        /// <param name="navigationContext">Navigation Requestの情報を表すNavigationContext。</param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        { }

        /// <summary>
        /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        /// </summary>
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public InteractionMessenger Messenger { get; } = new InteractionMessenger();

        [Display(Name = "サブプログラム")]
        [Required(ErrorMessage = "{0}をドラッグアンドドロップしてください")]
        public ReactiveProperty<string> NCProgramFileName { get; }

        public ReactiveProperty<DirectedOperationTypeAttempt> FetchedOperationType { get; }

        [Display(Name = "加工機")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<MachineToolTypeAttempt> MachineTool { get; }

        [Display(Name = "材質")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<MaterialTypeAttempt> Material { get; }

        [Display(Name = "リーマ")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<ReamerTypeAttempt> Reamer { get; }

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
}
