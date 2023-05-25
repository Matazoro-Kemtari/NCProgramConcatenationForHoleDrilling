using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

internal record class SequenceOrder(SequenceOrderType SequenceOrderType)
{
    public NcProgramRole ToNcProgramRole() => SequenceOrderType switch
    {
        SequenceOrderType.CenterDrilling => NcProgramRole.CenterDrilling,
        SequenceOrderType.PilotDrilling or SequenceOrderType.Drilling => NcProgramRole.Drilling,
        SequenceOrderType.Chamfering => NcProgramRole.Chamfering,
        SequenceOrderType.Reaming => NcProgramRole.Reaming,
        SequenceOrderType.Tapping => NcProgramRole.Tapping,
        _ => throw new NotImplementedException(),
    };
}

internal enum SequenceOrderType
{
    CenterDrilling,
    PilotDrilling,
    Drilling,
    Chamfering,
    Reaming,
    Tapping,
}
