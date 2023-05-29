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
        NcProgramFile.Value = string.Empty;
        FetchedOperationType.Value = DirectedOperation.Undetected;
        MachineTool.Value = Models.MachineTool.Undefined;
        Material.Value = Models.Material.Undefined;
        Reamer.Value = ReamerType.Undefined;
        HoleType.Value = DrillingMethod.Undefined;
        BlindPilotHoleDepth.Value = string.Empty;
        BlindHoleDepth.Value = string.Empty;
        Thickness.Value = string.Empty;
    }

    [Logging]
    internal EditNcProgramParam ToEditNcProgramParam()
        => new((DirectedOperationTypeAttempt)FetchedOperationType.Value,
               SubProgramNumber.Value,
               DirectedOperationToolDiameter.Value,
               MainProgramCodes.Where(x => x.MachineToolClassification == MachineTool.Value).Select(x => x.NcProgramCodes).First(),
               (MaterialTypeAttempt)Material.Value,
               (ReamerTypeAttempt)Reamer.Value,
               decimal.Parse(Thickness.Value),
               (DrillingMethodAttempt)HoleType.Value,
               BlindPilotHoleDepth.Value,
               BlindHoleDepth.Value,
               MainNcProgramParameters
               ?? throw new InvalidOperationException(
                   "リストの準備が出来ていないか 失敗しています"));

    [Logging]
    internal void SetMainNcProgramParameters(MainNcProgramParametersAttempt mainNcProgramParametersAttempt)
        => MainNcProgramParameters = mainNcProgramParametersAttempt;

    internal ReactivePropertySlim<string> NcProgramFile { get; } = new();

    internal ReactivePropertySlim<DirectedOperation> FetchedOperationType { get; } = new(DirectedOperation.Undetected);

    internal ReactivePropertySlim<MachineTool> MachineTool { get; } = new();

    internal ReactivePropertySlim<Material> Material { get; } = new();

    internal ReactivePropertySlim<ReamerType> Reamer { get; } = new();

    internal ReactivePropertySlim<DrillingMethod> HoleType { get; } = new();

    internal ReactivePropertySlim<string> BlindPilotHoleDepth { get; } = new();

    internal ReactivePropertySlim<string> BlindHoleDepth { get; } = new();

    internal ReactivePropertySlim<string> Thickness { get; } = new(string.Empty);

    internal ReactivePropertySlim<string> SubProgramNumber { get; } = new(string.Empty);

    internal ReactivePropertySlim<decimal> DirectedOperationToolDiameter { get; } = new();

    internal IList<MainNcProgramCodeRequest> MainProgramCodes { get; } = new List<MainNcProgramCodeRequest>();

    internal MainNcProgramParametersAttempt? MainNcProgramParameters { get; private set; } = null;
}

public enum DirectedOperation
{
    [EnumDisplayName("タップ")]
    Tapping,
    [EnumDisplayName("リーマー")]
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

public enum DrillingMethod
{
    Undefined,
    // 通し穴
    ThroughHole,
    // 止まり穴
    BlindHole,
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
