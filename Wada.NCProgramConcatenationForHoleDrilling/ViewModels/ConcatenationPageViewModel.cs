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
using Wada.CombineMainNcProgramApplication;
using Wada.EditNcProgramApplication;
using Wada.Extensions;
using Wada.NcProgramConcatenationForHoleDrilling.Models;
using Wada.NcProgramConcatenationForHoleDrilling.Views;
using Wada.ReadMainNcProgramApplication;
using Wada.ReadMainNcProgramParametersApplication;
using Wada.ReadSubNcProgramApplication;
using Wada.UseCase.DataClass;

namespace Wada.NcProgramConcatenationForHoleDrilling.ViewModels
{
    public class ConcatenationPageViewModel : BindableBase, INavigationAware, IDestructible, IDropTarget
    {
        private readonly ConcatenationPageModel _concatenation = new();
        private IRegionNavigationService? _regionNavigationService;
        private readonly IDialogService _dialogService;
        private readonly IReadMainNcProgramUseCase _readMainNCProgramUseCase;
        private readonly IReadSubNcProgramUseCase _readSubNCProgramUseCase;
        private readonly IReadMainNcProgramParametersUseCase _readMainNCProgramParametersUseCase;
        private readonly IEditNcProgramUseCase _editNCProgramUseCase;
        private readonly ICombineMainNcProgramUseCase _combineMainNCProgramUseCase;

        public ConcatenationPageViewModel(IDialogService dialogService, IReadMainNcProgramUseCase readMainNCProgramUseCase, IReadSubNcProgramUseCase readSubNCProgramUseCase, IReadMainNcProgramParametersUseCase readMainNCProgramParametersUseCase, IEditNcProgramUseCase editNCProgramUseCase, ICombineMainNcProgramUseCase combineMainNCProgramUseCase)
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
                    (x, y) => x == DirectedOperation.Reaming && y == ReamerType.Undefined),
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
                    var mainPrograms = await _readMainNCProgramUseCase.ExecuteAsync();
                    mainPrograms.ToList().ForEach(
                        x => _concatenation.MainProgramCodes.Add(MainNcProgramCodeRequest.Parse(x)));
                }
                catch (Exception ex) when (ex is InvalidOperationException or ReadMainNcProgramUseCaseException)
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
                    _concatenation.SetMainNCProgramParameters(_dto != null
                    ? _dto.Convert()
                    : throw new NcProgramConcatenationForHoleDrillingException(
                        "リストを読み込もうとしましたが、失敗しました\n" +
                        "リストの内容を確認してください"));
                }
                catch (ReadMainNcProgramParametersUseCaseException ex)
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
            if (!_concatenation.MainProgramCodes.Any()
                || _concatenation.MainNcProgramParameters == null)
            {
                var message = MessageNotificationViaLivet.MakeInformationMessage(
                    "設定ファイルの準備ができていません\n" +
                    "数分待って実行してください\n" +
                    "数分待っても状況が変わらない場合は 上長に報告してください");
                await Messenger.RaiseAsync(message);
                return;
            }

            // メインプログラムを編集する
            EditNcProgramDto editedCodes;
            try
            {
                editedCodes = await _editNCProgramUseCase.ExecuteAsync(_concatenation.ToEditNcProgramPram());
            }
            catch (EditNcProgramUseCaseException ex)
            {
                var message = MessageNotificationViaLivet.MakeErrorMessage(
                    "メインプログラム編集中にエラーが発生しました\n" +
                    $"{ex.Message}");
                await Messenger.RaiseAsync(message);
                return;
            }

            // 結合する
            CombineMainNcProgramParam combineParam = new(
                editedCodes.NcProgramCodes,
                (MachineToolTypeAttempt)_concatenation.MachineTool.Value,
                (MaterialTypeAttempt)_concatenation.Material.Value);
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
            OperationDirecterAttemp operationDirecter;
            try
            {
                operationDirecter = await _readSubNCProgramUseCase.ExecuteAsync(path);
            }
            catch (ReadSubNcProgramUseCaseException ex)
            {
                var message = MessageNotificationViaLivet.MakeInformationMessage(
                    $"サブプログラムの読み込みでエラーが発生しました\n{ex.Message}");
                await Messenger.RaiseAsync(message);
                _concatenation.Clear();

                return;
            }

            // 読み込んだサブプログラムの作業指示を取得する
            _concatenation.FetchedOperationType.Value = (DirectedOperation)operationDirecter.DirectedOperationClassification;
            _concatenation.DirectedOperationToolDiameter.Value = operationDirecter.DirectedOperationToolDiameter;
            _concatenation.SubProgramNumber.Value = operationDirecter.SubNcProgramCode.ProgramName;

            IDialogParameters parameters = new DialogParameters(
                $"OperationTypeString={_concatenation.FetchedOperationType.Value.GetEnumDisplayName()}&SubProgramSource={operationDirecter.SubNcProgramCode}");
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

        public ReactiveProperty<DirectedOperation> FetchedOperationType { get; }

        [Display(Name = "加工機")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<MachineTool> MachineTool { get; }

        [Display(Name = "材質")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
        public ReactiveProperty<Material> Material { get; }

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
}
