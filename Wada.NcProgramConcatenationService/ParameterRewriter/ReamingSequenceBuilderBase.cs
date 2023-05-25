using System.Data;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

internal enum ReamerType
{
    CrystalReamerParameter,
    SkillReamerParameter,
}

public abstract class ReamingSequenceBuilderBase : IMainProgramSequenceBuilder
{
    private readonly ParameterType _parameterType;
    private readonly ReamerType _reamerType;

    private protected ReamingSequenceBuilderBase(ParameterType parameterType, ReamerType reamerType)
    {
        _parameterType = parameterType;
        _reamerType = reamerType;
    }

    [Logging]
    public virtual IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord)
    {
        if (rewriteByToolRecord.Material == MaterialType.Undefined)
            throw new ArgumentException("素材が未定義です");

        // _parameterTypeリーマのパラメータを受け取る
        IEnumerable<ReamingProgramParameter> reamingParameters;
        if (_parameterType == ParameterType.CrystalReamerParameter)
            reamingParameters = rewriteByToolRecord.CrystalReamerParameters;
        else
            reamingParameters = rewriteByToolRecord.SkillReamerParameters;

        ReamingProgramParameter reamingParameter;
        try
        {
            reamingParameter = reamingParameters.First(x => x.DirectedOperationToolDiameter == rewriteByToolRecord.DirectedOperationToolDiameter);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainException(
                $"リーマー径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません", ex);
        }

        // ドリルのパラメータを受け取る

        // 下穴 1回目
        var fastDrillingParameter = rewriteByToolRecord.DrillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= reamingParameter.PreparedHoleDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {reamingParameter.PreparedHoleDiameter}");
        var fastDrillingDepth = rewriteByToolRecord.DrillingMethod switch
        {
            DrillingMethod.ThroughHole => rewriteByToolRecord.Thickness + fastDrillingParameter.DrillTipLength,
            DrillingMethod.BlindHole => rewriteByToolRecord.BlindPilotHoleDepth,
            _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
        };

        // 下穴 2回目
        var secondDrillingParameter = rewriteByToolRecord.DrillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= reamingParameter.SecondPreparedHoleDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {reamingParameter.SecondPreparedHoleDiameter}");
        var secondDrillingDepth = rewriteByToolRecord.Thickness + secondDrillingParameter.DrillTipLength;

        // リーマーの工程
        SequenceOrder[] sequenceOrders = new[]
        {
            new SequenceOrder(SequenceOrderType.CenterDrilling),
            new SequenceOrder(SequenceOrderType.PilotDrilling),
            new SequenceOrder(SequenceOrderType.Drilling),
            new SequenceOrder(SequenceOrderType.Chamfering),
            new SequenceOrder(SequenceOrderType.Reaming),
        };

        // メインプログラムを工程ごとに取り出す
        var rewrittenNcPrograms = sequenceOrders.Select(sequenceOrder => sequenceOrder.SequenceOrderType switch
        {
            SequenceOrderType.CenterDrilling => CenterDrillingProgramRewriter.Rewrite(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                reamingParameter,
                rewriteByToolRecord.SubProgramNumber),

            SequenceOrderType.PilotDrilling => DrillingProgramRewriter.Rewrite(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                fastDrillingDepth,
                fastDrillingParameter,
                rewriteByToolRecord.SubProgramNumber,
                reamingParameter.PreparedHoleDiameter),

            SequenceOrderType.Drilling => DrillingProgramRewriter.Rewrite(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                secondDrillingDepth,
                secondDrillingParameter,
                rewriteByToolRecord.SubProgramNumber,
                reamingParameter.SecondPreparedHoleDiameter),

            SequenceOrderType.Chamfering => reamingParameter.ChamferingDepth != null
            ? ChamferingProgramRewriter.Rewrite(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                reamingParameter,
                rewriteByToolRecord.SubProgramNumber)
            : null,

            SequenceOrderType.Reaming => ReamingProgramRewriter.Rewrite(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                _reamerType,
                rewriteByToolRecord.DrillingMethod switch
                {
                    DrillingMethod.ThroughHole => rewriteByToolRecord.Thickness + 5m,
                    DrillingMethod.BlindHole => rewriteByToolRecord.BlindHoleDepth,
                    _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
                },
                reamingParameter,
                rewriteByToolRecord.SubProgramNumber),

            _ => throw new NotImplementedException(),
        });

        return (IEnumerable<NcProgramCode>)rewrittenNcPrograms.Where(x => x != null);
    }
}
