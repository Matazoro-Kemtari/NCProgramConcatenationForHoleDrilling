using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

public class TappingSequenceBuilder : IMainProgramSequenceBuilder
{
    private readonly Dictionary<SequenceOrderType, Func<INcProgramRewriteArg, NcProgramCode>> _ncProgramRewriters = new()
    {
        { SequenceOrderType.CenterDrilling, CenterDrillingProgramRewriter.Rewrite },
        { SequenceOrderType.PilotDrilling, DrillingProgramRewriter.Rewrite },
        { SequenceOrderType.Chamfering, ChamferingProgramRewriter.Rewrite },
        { SequenceOrderType.Tapping, TappingProgramRewriter.Rewrite }
    };


    [Logging]
    public virtual IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolArg rewriteByToolRecord)
    {
        if (rewriteByToolRecord.Material == MaterialType.Undefined)
            throw new ArgumentException("素材が未定義です");

        // タップのパラメータを受け取る
        var tappingParameters = rewriteByToolRecord.TapParameters;

        // ドリルのパラメータを受け取る
        var drillingParameters = rewriteByToolRecord.DrillingParameters;

        TappingProgramParameter tappingParameter;
        try
        {
            tappingParameter = tappingParameters
                .First(x => x.DirectedOperationToolDiameter == rewriteByToolRecord.DirectedOperationToolDiameter);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainException(
                $"タップ径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません", ex);
        }

        var drillingParameter = drillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= tappingParameter.PreparedHoleDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {tappingParameter.PreparedHoleDiameter}");

        var drillingDepth = rewriteByToolRecord.DrillingMethod switch
        {
            DrillingMethod.ThroughHole => rewriteByToolRecord.Thickness + drillingParameter.DrillTipLength,
            DrillingMethod.BlindHole => rewriteByToolRecord.BlindPilotHoleDepth,
            _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
        };

        // タップの工程
        SequenceOrder[] sequenceOrders = new[]
        {
            new SequenceOrder(SequenceOrderType.CenterDrilling),
            new SequenceOrder(SequenceOrderType.PilotDrilling),
            new SequenceOrder(SequenceOrderType.Chamfering),
            new SequenceOrder(SequenceOrderType.Tapping),
        };

        // メインプログラムを工程ごとに取り出す
        var rewrittenNcPrograms = sequenceOrders.Select(
            sequenceOrder => _ncProgramRewriters[sequenceOrder.SequenceOrderType](
                MakeCenterDrillingRewriteArg(sequenceOrder, rewriteByToolRecord)));

        return rewrittenNcPrograms.ToList();
    }

    private static INcProgramRewriteArg MakeCenterDrillingRewriteArg(SequenceOrder sequenceOrder, RewriteByToolArg rewriteByToolRecord)
    {
        // タップのパラメータを受け取る
        var tappingParameters = rewriteByToolRecord.TapParameters;

        // ドリルのパラメータを受け取る
        var drillingParameters = rewriteByToolRecord.DrillingParameters;

        TappingProgramParameter tappingParameter;
        try
        {
            tappingParameter = tappingParameters
                .First(x => x.DirectedOperationToolDiameter == rewriteByToolRecord.DirectedOperationToolDiameter);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainException(
                $"タップ径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません", ex);
        }

        var drillingParameter = drillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= tappingParameter.PreparedHoleDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {tappingParameter.PreparedHoleDiameter}");

        var drillingDepth = rewriteByToolRecord.DrillingMethod switch
        {
            DrillingMethod.ThroughHole => rewriteByToolRecord.Thickness + drillingParameter.DrillTipLength,
            DrillingMethod.BlindHole => rewriteByToolRecord.BlindPilotHoleDepth,
            _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
        };

        return sequenceOrder.SequenceOrderType switch
        {
            SequenceOrderType.CenterDrilling => new CenterDrillingRewriteArg(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                tappingParameter,
                rewriteByToolRecord.SubProgramNumber),

            SequenceOrderType.PilotDrilling => new DrillingRewriteArg(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                drillingDepth,
                drillingParameter,
                rewriteByToolRecord.SubProgramNumber,
                tappingParameter.PreparedHoleDiameter),

            SequenceOrderType.Chamfering => new ChamferingRewriteArg(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                tappingParameter,
                rewriteByToolRecord.SubProgramNumber),

            SequenceOrderType.Tapping => new TappingRewriteArg(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                rewriteByToolRecord.DrillingMethod switch
                {
                    DrillingMethod.ThroughHole => rewriteByToolRecord.Thickness + 5m,
                    DrillingMethod.BlindHole => rewriteByToolRecord.BlindHoleDepth,
                    _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
                },
                tappingParameter,
                rewriteByToolRecord.SubProgramNumber),

            _ => throw new NotImplementedException(),
        };
    }
}
