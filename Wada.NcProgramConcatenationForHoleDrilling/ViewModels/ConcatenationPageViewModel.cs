using GongSolutions.Wpf.DragDrop;
using Livet.Messaging;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
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

namespace Wada.NcProgramConcatenationForHoleDrilling.ViewModels;

public class ConcatenationPageViewModel : BindableBase, INavigationAware, IDestructible, IDropTarget
{
    private readonly ConcatenationPageModel _concatenation = new();
    private IRegionNavigationService? _regionNavigationService;
    private readonly IDialogService _dialogService;
    private readonly IReadMainNcProgramUseCase _readMainNcProgramUseCase;
    private readonly IReadSubNcProgramUseCase _readSubNcProgramUseCase;
    private readonly IReadMainNcProgramParametersUseCase _readMainNcProgramParametersUseCase;
    private readonly IEditNcProgramUseCase _editNcProgramUseCase;
    private readonly ICombineMainNcProgramUseCase _combineMainNcProgramUseCase;

    public ConcatenationPageViewModel(IDialogService dialogService, IReadMainNcProgramUseCase readMainNcProgramUseCase, IReadSubNcProgramUseCase readSubNcProgramUseCase, IReadMainNcProgramParametersUseCase readMainNcProgramParametersUseCase, IEditNcProgramUseCase editNcProgramUseCase, ICombineMainNcProgramUseCase combineMainNcProgramUseCase)
    {
        _dialogService = dialogService;
        _readMainNcProgramUseCase = readMainNcProgramUseCase;
        _readSubNcProgramUseCase = readSubNcProgramUseCase;
        _readMainNcProgramParametersUseCase = readMainNcProgramParametersUseCase;
        _editNcProgramUseCase = editNcProgramUseCase;
        _combineMainNcProgramUseCase = combineMainNcProgramUseCase;

        NcProgramFileName = _concatenation.NcProgramFile.ToReactivePropertyAsSynchronized(x => x.Value)
                                                        .SetValidateAttribute(() => NcProgramFileName)
                                                        .AddTo(Disposables);

        // ドラッグアンドドロップされて 値が書き換わったイベント
        _concatenation.NcProgramFile.Skip(1)
                                    .Where(x => x != null)
                                    .Subscribe(x => ChangeSubprogramPath(x!));

        ErrorMsgNcProgramFileName = NcProgramFileName
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        FetchedOperationType = _concatenation
            .FetchedOperationType.ToReactivePropertyAsSynchronized(x => x.Value)
                                 .AddTo(Disposables);

        MachineTool = _concatenation.MachineTool.ToReactivePropertyAsSynchronized(x => x.Value)
                                                .SetValidateAttribute(() => MachineTool)
                                                .AddTo(Disposables);

        ErrorMsgMachineTool = MachineTool
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        Material = _concatenation.Material.ToReactivePropertyAsSynchronized(x => x.Value)
                                          .SetValidateAttribute(() => Material)
                                          .AddTo(Disposables);

        ErrorMsgMaterial = Material
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        Reamer = _concatenation.Reamer.ToReactivePropertyAsSynchronized(x => x.Value)
                                      .SetValidateAttribute(() => Reamer)
                                      .AddTo(Disposables);

        ErrorMsgReamer = Reamer
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        HoleType = _concatenation.HoleType.ToReactivePropertyAsSynchronized(x => x.Value)
                                          .SetValidateAttribute(() => HoleType)
                                          .AddTo(Disposables);

        ErrorMsgDrillingMethod = HoleType
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        BlindPilotHoleDepth = _concatenation.BlindPilotHoleDepth.ToReactivePropertyAsSynchronized(x => x.Value)
                                                                .SetValidateAttribute(() => BlindPilotHoleDepth)
                                                                .AddTo(Disposables);

        ErrorMsgBlindPilotHoleDepth = BlindPilotHoleDepth
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        BlindHoleDepth = _concatenation.BlindHoleDepth.ToReactivePropertyAsSynchronized(x => x.Value)
                                                      .SetValidateAttribute(() => BlindHoleDepth)
                                                      .AddTo(Disposables);

        ErrorMsgBlindHoleDepth = BlindHoleDepth
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        Thickness = _concatenation.Thickness.ToReactivePropertyAsSynchronized(x => x.Value)
                                            .SetValidateAttribute(() => Thickness)
                                            .AddTo(Disposables);

        ErrorMsgThickness = Thickness
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        HoleDepthRelationship = FetchedOperationType.CombineLatest(
            BlindPilotHoleDepth,
            BlindHoleDepth,
            (fetchedOperationType, blindPilotHoleDepth, BlindHoleDepth) =>
            {
                if (!double.TryParse(blindPilotHoleDepth, out var pilotDepth))
                    pilotDepth = default;
                if (!double.TryParse(BlindHoleDepth, out var holeDepth))
                    holeDepth = default;

                return fetchedOperationType == DirectedOperation.Drilling ? double.MaxValue : pilotDepth - holeDepth;
            })
            .ToReactiveProperty()
            .SetValidateNotifyError(v => ValidatePilotHoleDepthIsDeeper(v))
            .AddTo(Disposables);

        ErrorMsgHoleDepthRelationship = HoleDepthRelationship
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        ThicknessHoleDepthRelationship = FetchedOperationType.CombineLatest(
            HoleType,
            Thickness,
            BlindPilotHoleDepth,
            BlindHoleDepth,
            (fetchedOperationType, holeType, thickness, blindPilotHoleDepth, blindHoleDepth) =>
            {
                if (!double.TryParse(blindPilotHoleDepth, out var _pilotDepth))
                    _pilotDepth = default;
                if (!double.TryParse(blindHoleDepth, out var _holeDepth))
                    _holeDepth = default;
                var _maxHoleDepth = fetchedOperationType == DirectedOperation.Drilling
                ? _holeDepth
                : (new[] { _pilotDepth, _holeDepth }).Max();

                if (!double.TryParse(thickness, out var _thickness))
                    _thickness = default;

                return holeType == DrillingMethod.ThroughHole ? double.MaxValue : _thickness - _maxHoleDepth;
            })
            .ToReactiveProperty()
            .SetValidateNotifyError(v => ValidateBlindHoleAgainstThickness(v))
            .AddTo(Disposables);

        ErrorMsgThicknessHoleDepthRelationship = ThicknessHoleDepthRelationship
            .ObserveErrorChanged.Select(x => x?.Cast<string>().FirstOrDefault())
                                .ToReadOnlyReactivePropertySlim()
                                .AddTo(Disposables);

        // コマンドボタンのbind
        NextViewCommand = new[]
        {
            NcProgramFileName.ObserveHasErrors,
            MachineTool.ObserveHasErrors,
            Material.ObserveHasErrors,
            FetchedOperationType.CombineLatest(
                Reamer,
                (x, y) => x == DirectedOperation.Reaming && y == ReamerType.Undefined),
            HoleType.ObserveHasErrors,
            FetchedOperationType.CombineLatest(
                HoleType,
                BlindPilotHoleDepth.ObserveHasErrors,
                (fetchedOperationType, holeType, blindPilotHoleDepth)
                => !(fetchedOperationType is DirectedOperation.Drilling or DirectedOperation.Undetected)
                   && holeType == DrillingMethod.BlindHole
                   && blindPilotHoleDepth),
            HoleType.CombineLatest(
                BlindHoleDepth.ObserveHasErrors,
                (holeType, blindHoleDepth)
                => holeType == DrillingMethod.BlindHole
                   && blindHoleDepth),
            FetchedOperationType.CombineLatest(
                HoleType,
                HoleDepthRelationship.ObserveHasErrors,
                (fetchedOperationType, holeType, holeDepthRelationship)
                => !(fetchedOperationType is DirectedOperation.Drilling or DirectedOperation.Undetected)
                   && holeType == DrillingMethod.BlindHole
                   && holeDepthRelationship),
            Thickness.ObserveHasErrors,
            HoleType.CombineLatest(
                ThicknessHoleDepthRelationship.ObserveHasErrors,
                (holeType, thicknessHoleDepthRelationship)
                => holeType == DrillingMethod.BlindHole
                   && thicknessHoleDepthRelationship),
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
                var mainPrograms = await _readMainNcProgramUseCase.ExecuteAsync();
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
                var _dto = await _readMainNcProgramParametersUseCase.ExecuteAsync();
                _concatenation.SetMainNcProgramParameters(_dto != null
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
            editedCodes = await _editNcProgramUseCase.ExecuteAsync(_concatenation.ToEditNcProgramParam());
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
        var combinedCode = await _combineMainNcProgramUseCase.ExecuteAsync(combineParam);

        // 画面遷移
        var navigationParams = new NavigationParameters
        {
            { nameof(combinedCode), combinedCode.NcProgramCode.ToString() }
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
            operationDirecter = await _readSubNcProgramUseCase.ExecuteAsync(path);
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

        _concatenation.NcProgramFile.Value =
            dragFileList.FirstOrDefault(x => Path.GetExtension(x) == string.Empty) ?? string.Empty;
    }

    /// <summary>
    /// 下穴深さの方がより深いか検証する
    /// </summary>
    /// <param name="value">下穴深さ</param>
    /// <returns></returns>
    [Logging]
    private string? ValidatePilotHoleDepthIsDeeper(double? value)
    {
        if (value == null)
            return null;

        if (value <= 0)
        {
            var blindHoleName = _concatenation.FetchedOperationType.Value == DirectedOperation.Undetected
                ? "止まり穴"
                : _concatenation.FetchedOperationType.Value.GetEnumDisplayName();
            return $"下穴深さには {blindHoleName}深さを超える値を入力してください";
        }

        return null;
    }

    /// <summary>
    /// 止まり穴深さが板厚を超えていないか検証する
    /// </summary>
    /// <param name="value">止まり穴深さ</param>
    /// <returns></returns>
    [Logging]
    private string? ValidateBlindHoleAgainstThickness(double? value)
    {
        if (value == null)
            return null;

        if (value <= 0)
        {
            var blindHoleName = _concatenation.FetchedOperationType.Value == DirectedOperation.Undetected
           ? "止まり穴"
           : _concatenation.FetchedOperationType.Value.GetEnumDisplayName();
            return $"{blindHoleName}深さは板厚を超えない値を入力してください";
        }

        return null;
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
    public ReactiveProperty<string> NcProgramFileName { get; }

    public ReactiveProperty<DirectedOperation> FetchedOperationType { get; }

    [Display(Name = "加工機")]
    [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
    public ReactiveProperty<MachineTool> MachineTool { get; }

    [Display(Name = "材質")]
    [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
    public ReactiveProperty<Material> Material { get; }

    [Display(Name = "リーマー")]
    [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
    public ReactiveProperty<ReamerType> Reamer { get; }

    [Display(Name = "穴加工")]
    [Range(1, int.MaxValue, ErrorMessage = "{0}を選択してください")]
    public ReactiveProperty<DrillingMethod> HoleType { get; }

    [Display(Name = "下穴深さ")]
    [Required(ErrorMessage = "{0}を入力してください")]
    [RegularExpression(@"[0-9]+(\.[0-9]+)?", ErrorMessage = "半角の整数または小数を入力してください")]
    [Range(1, double.MaxValue, ErrorMessage = "{0}は{1:F}～{2:F}の範囲を入力してください")]
    public ReactiveProperty<string> BlindPilotHoleDepth { get; }

    [Display(Name = "止まり穴深さ")]
    [Required(ErrorMessage = "{0}を入力してください")]
    [RegularExpression(@"[0-9]+(\.[0-9]+)?", ErrorMessage = "半角の整数または小数を入力してください")]
    [Range(1, double.MaxValue, ErrorMessage = "{0}は{1:F}～{2:F}の範囲を入力してください")]
    public ReactiveProperty<string> BlindHoleDepth { get; }

    [Display(Name = "板厚")]
    [Required(ErrorMessage = "{0}を入力してください")]
    [RegularExpression(@"[0-9]+(\.[0-9]+)?", ErrorMessage = "半角の整数または小数を入力してください")]
    [Range(1, double.MaxValue, ErrorMessage = "{0}は{1:F}～{2:F}の範囲を入力してください")]
    public ReactiveProperty<string> Thickness { get; }

    public AsyncReactiveCommand NextViewCommand { get; }

    public ReactiveCommand ClearCommand { get; }

    /// <summary>
    /// 下穴深さと止まり穴深さの関係表示用
    /// </summary>
    public ReactiveProperty<double> HoleDepthRelationship { get; }

    /// <summary>
    /// 板厚と下穴深さの関係表示用
    /// </summary>
    public ReactiveProperty<double> ThicknessHoleDepthRelationship { get; }

    public ReadOnlyReactivePropertySlim<string?> ErrorMsgNcProgramFileName { get; }
    public ReadOnlyReactivePropertySlim<string?> ErrorMsgMachineTool { get; }
    public ReadOnlyReactivePropertySlim<string?> ErrorMsgMaterial { get; }
    public ReadOnlyReactivePropertySlim<string?> ErrorMsgThickness { get; }
    public ReadOnlyReactivePropertySlim<string?> ErrorMsgReamer { get; }
    public ReadOnlyReactivePropertySlim<string?> ErrorMsgDrillingMethod { get; }
    public ReadOnlyReactivePropertySlim<string?> ErrorMsgBlindHoleDepth { get; }
    public ReadOnlyReactivePropertySlim<string?> ErrorMsgBlindPilotHoleDepth { get; }

    public ReadOnlyReactivePropertySlim<string?> ErrorMsgHoleDepthRelationship { get; }
    public ReadOnlyReactivePropertySlim<string?> ErrorMsgThicknessHoleDepthRelationship { get; }
}
