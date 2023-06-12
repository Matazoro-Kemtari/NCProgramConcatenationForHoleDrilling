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
    private const decimal chamferingThresholdDrillDiameter = 15.6m;
    private readonly ParameterType _parameterType;
    private readonly ReamerType _reamerType;
    private readonly Dictionary<SequenceOrderType, Func<INcProgramRewriteParameter, Task<NcProgramCode>>> _ncProgramRewriters = new()
    {
        { SequenceOrderType.CenterDrilling, CenterDrillingProgramRewriter.RewriteAsync },
        { SequenceOrderType.PilotDrilling, DrillingProgramRewriter.RewriteAsync },
        { SequenceOrderType.SecondaryPilotDrilling, DrillingProgramRewriter.RewriteAsync },
        { SequenceOrderType.Chamfering, ChamferingProgramRewriter.RewriteAsync },
        { SequenceOrderType.Reaming, ReamingProgramRewriter.RewriteAsync },
    };

    private protected ReamingSequenceBuilderBase(ParameterType parameterType, ReamerType reamerType)
    {
        _parameterType = parameterType;
        _reamerType = reamerType;
    }

    [Logging]
    public virtual async Task<IEnumerable<NcProgramCode>> RewriteByToolAsync(ToolParameter toolParameter)
    {
        if (toolParameter.Material == MaterialType.Undefined)
            throw new ArgumentException("素材が未定義です");

        // _parameterTypeリーマのパラメータを受け取る
        IEnumerable<ReamingProgramParameter> reamingParameters = _parameterType switch
        {
            ParameterType.CrystalReamerParameter => toolParameter.CrystalReamerParameters,
            ParameterType.SkillReamerParameter => toolParameter.SkillReamerParameters,
            _ => throw new NotImplementedException(),
        };

        ReamingProgramParameter reamingParameter;
        try
        {
            reamingParameter = reamingParameters.First(x => x.DirectedOperationToolDiameter == toolParameter.DirectedOperationToolDiameter);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainException(
                $"リーマー径 {toolParameter.DirectedOperationToolDiameter}のリストがありません", ex);
        }

        // リーマーの工程
        SequenceOrder[] sequenceOrders = IsChamferingExecute(reamingParameter)
            ? new[]
            {
                new SequenceOrder(SequenceOrderType.CenterDrilling),
                new SequenceOrder(SequenceOrderType.PilotDrilling),
                new SequenceOrder(SequenceOrderType.SecondaryPilotDrilling),
                new SequenceOrder(SequenceOrderType.Chamfering),
                new SequenceOrder(SequenceOrderType.Reaming),
            }
            : new[]
            {
                new SequenceOrder(SequenceOrderType.CenterDrilling),
                new SequenceOrder(SequenceOrderType.PilotDrilling),
                new SequenceOrder(SequenceOrderType.SecondaryPilotDrilling),
                new SequenceOrder(SequenceOrderType.Reaming),
            };

        // メインプログラムを工程ごとに取り出す
        var rewrittenNcPrograms = await Task.WhenAll(sequenceOrders.Select(
            async sequenceOrder => await _ncProgramRewriters[sequenceOrder.SequenceOrderType](
                MakeCenterDrillingRewriteParameter(sequenceOrder, toolParameter))));

        return rewrittenNcPrograms.ToList();
    }

    private static bool IsChamferingExecute(ReamingProgramParameter reamingParameter)
        => !(reamingParameter.ChamferingDepth == null
        || reamingParameter.SecondaryPilotHoleDiameter >= chamferingThresholdDrillDiameter);

    private INcProgramRewriteParameter MakeCenterDrillingRewriteParameter(SequenceOrder sequenceOrder, ToolParameter toolParameter)
    {
        var rewriterSelector = _reamerType switch
        {
            ReamerType.CrystalReamerParameter => RewriterSelector.CrystalReaming,
            ReamerType.SkillReamerParameter => RewriterSelector.SkillReaming,
            _ => throw new NotImplementedException(),
        };

        return sequenceOrder.SequenceOrderType switch
        {
            SequenceOrderType.CenterDrilling => toolParameter.ToCenterDrillingRewriteParameter(rewriterSelector),

            SequenceOrderType.PilotDrilling => toolParameter.ToPilotDrillingRewriteParameter(rewriterSelector),

            SequenceOrderType.SecondaryPilotDrilling => toolParameter.ToSecondaryPilotDrillingRewriteParameter(rewriterSelector),

            SequenceOrderType.Chamfering => toolParameter.ToChamferingRewriteParameter(rewriterSelector),

            SequenceOrderType.Reaming => toolParameter.ToReamingRewriteParameter(rewriterSelector),

            _ => throw new NotImplementedException(),
        };
    }
}
