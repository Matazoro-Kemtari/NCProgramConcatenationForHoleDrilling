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
    private readonly Dictionary<SequenceOrderType, Func<INcProgramRewriteArg, NcProgramCode>> _ncProgramRewriters = new()
    {
        { SequenceOrderType.CenterDrilling, CenterDrillingProgramRewriter.Rewrite },
        { SequenceOrderType.PilotDrilling, DrillingProgramRewriter.Rewrite },
        { SequenceOrderType.Drilling, DrillingProgramRewriter.Rewrite },
        { SequenceOrderType.Chamfering, ChamferingProgramRewriter.Rewrite },
        { SequenceOrderType.Reaming, ReamingProgramRewriter.Rewrite },
    };

    private protected ReamingSequenceBuilderBase(ParameterType parameterType, ReamerType reamerType)
    {
        _parameterType = parameterType;
        _reamerType = reamerType;
    }

    [Logging]
    public virtual IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolArg rewriteByToolRecord)
    {
        if (rewriteByToolRecord.Material == MaterialType.Undefined)
            throw new ArgumentException("素材が未定義です");

        // _parameterTypeリーマのパラメータを受け取る
        IEnumerable<ReamingProgramParameter> reamingParameters = _parameterType switch
        {
            ParameterType.CrystalReamerParameter => rewriteByToolRecord.CrystalReamerParameters,
            ParameterType.SkillReamerParameter => rewriteByToolRecord.SkillReamerParameters,
            _ => throw new NotImplementedException(),
        };

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

        // リーマーの工程
        SequenceOrder[] sequenceOrders = reamingParameter.ChamferingDepth == null
            ? new[]
            {
                new SequenceOrder(SequenceOrderType.CenterDrilling),
                new SequenceOrder(SequenceOrderType.PilotDrilling),
                new SequenceOrder(SequenceOrderType.Drilling),
                new SequenceOrder(SequenceOrderType.Reaming),
            }
            : new[]
            {
                new SequenceOrder(SequenceOrderType.CenterDrilling),
                new SequenceOrder(SequenceOrderType.PilotDrilling),
                new SequenceOrder(SequenceOrderType.Drilling),
                new SequenceOrder(SequenceOrderType.Chamfering),
                new SequenceOrder(SequenceOrderType.Reaming),
            };

        // メインプログラムを工程ごとに取り出す
        var rewrittenNcPrograms = sequenceOrders.Select(
            sequenceOrder => _ncProgramRewriters[sequenceOrder.SequenceOrderType](
                MakeCenterDrillingRewriteArg(sequenceOrder, rewriteByToolRecord)));

        return rewrittenNcPrograms.ToList();
    }

    private INcProgramRewriteArg MakeCenterDrillingRewriteArg(SequenceOrder sequenceOrder, RewriteByToolArg rewriteByToolRecord)
    {
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

        return sequenceOrder.SequenceOrderType switch
        {
            SequenceOrderType.CenterDrilling => new CenterDrillingRewriteArg(
            rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
            rewriteByToolRecord.Material,
            reamingParameter,
            rewriteByToolRecord.SubProgramNumber),

            SequenceOrderType.PilotDrilling => new DrillingRewriteArg(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                fastDrillingDepth,
                fastDrillingParameter,
                rewriteByToolRecord.SubProgramNumber,
                reamingParameter.PreparedHoleDiameter),

            SequenceOrderType.Drilling => new DrillingRewriteArg(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                secondDrillingDepth,
                secondDrillingParameter,
                rewriteByToolRecord.SubProgramNumber,
                reamingParameter.SecondPreparedHoleDiameter),

            SequenceOrderType.Chamfering => new ChamferingRewriteArg(
                rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == sequenceOrder.ToNcProgramRole()),
                rewriteByToolRecord.Material,
                reamingParameter,
                rewriteByToolRecord.SubProgramNumber),

            SequenceOrderType.Reaming => new ReamingRewriteArg(
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
        };
    }
}
