using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using Wada.AOP.Logging;
using Wada.EditNcProgramApplication;
using Wada.Extensions;
using Wada.ReadMainNcProgramApplication;
using Wada.UseCase.DataClass;

namespace Wada.NcProgramConcatenationForHoleDrilling.Models;

internal record class ConcatenationPageModel
{
    [Logging]
    internal void Clear()
    {
        NCProgramFile.Value = string.Empty;
        FetchedOperationType.Value = DirectedOperation.Undetected;
        MachineTool.Value = Models.MachineTool.Undefined;
        Material.Value = Models.Material.Undefined;
        Reamer.Value = ReamerType.Undefined;
        Thickness.Value = string.Empty;
    }

    [Logging]
    internal EditNcProgramPram ToEditNcProgramPram()
        => new((DirectedOperationTypeAttempt)FetchedOperationType.Value,
               SubProgramNumber.Value,
               DirectedOperationToolDiameter.Value,
               MainProgramCodes.Where(x => x.MachineToolClassification == MachineTool.Value).Select(x => x.NcProgramCodes).First(),
               (MaterialTypeAttempt)Material.Value,
               (ReamerTypeAttempt)Reamer.Value,
               decimal.Parse(Thickness.Value),
               MainNcProgramParameters
               ?? throw new InvalidOperationException(
                   "リストの準備が出来ていないか 失敗しています"));

    [Logging]
    public void SetMainNCProgramParameters(MainNcProgramParametersAttempt mainNcProgramParametersAttempt)
        => MainNcProgramParameters = mainNcProgramParametersAttempt;

    public ReactivePropertySlim<string> NCProgramFile { get; } = new();

    public ReactivePropertySlim<DirectedOperation> FetchedOperationType { get; } = new(DirectedOperation.Undetected);

    public ReactivePropertySlim<MachineTool> MachineTool { get; } = new();

    public ReactivePropertySlim<Material> Material { get; } = new();

    public ReactivePropertySlim<ReamerType> Reamer { get; } = new();

    public ReactivePropertySlim<string> Thickness { get; } = new(string.Empty);

    public ReactivePropertySlim<string> SubProgramNumber { get; } = new(string.Empty);

    public ReactivePropertySlim<decimal> DirectedOperationToolDiameter { get; } = new();

    public IList<MainNcProgramCodeRequest> MainProgramCodes { get; } = new List<MainNcProgramCodeRequest>();

    public MainNcProgramParametersAttempt? MainNcProgramParameters { get; private set; } = null;
}

public enum DirectedOperation
{
    [EnumDisplayName("タップ")]
    Tapping,
    [EnumDisplayName("リーマ")]
    Reaming,
    [EnumDisplayName("ドリル")]
    Drilling,

    [EnumDisplayName("不明")]
    Undetected = int.MaxValue,
}

public enum MachineTool
{
    [EnumDisplayName("不明")]
    Undefined,

    [EnumDisplayName("RB250F")]
    RB250F = 1,

    [EnumDisplayName("RB260")]
    RB260,

    [EnumDisplayName("611V")]
    Triaxial,
}

public enum Material
{
    [EnumDisplayName("不明")]
    Undefined,

    [EnumDisplayName("AL")]
    Aluminum,

    [EnumDisplayName("SS400")]
    Iron,
}

public enum ReamerType
{
    Undefined,
    Crystal,
    Skill
}

public record class MainNcProgramCodeRequest(
        MachineTool MachineToolClassification,
        IEnumerable<NcProgramCodeAttempt> NcProgramCodes)
{
    [Logging]
    internal static MainNcProgramCodeRequest Parse(MainNcProgramCodeDto mainProgram)
    {
        return new MainNcProgramCodeRequest(
            (MachineTool)mainProgram.MachineToolClassification,
            mainProgram.NcProgramCodeAttempts);
    }
}
