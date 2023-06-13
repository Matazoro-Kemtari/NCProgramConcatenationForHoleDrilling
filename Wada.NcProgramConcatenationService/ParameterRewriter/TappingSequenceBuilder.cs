using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

public class TappingSequenceBuilder : IMainProgramSequenceBuilder
{
    private const decimal chamferingThresholdDrillDiameter = 15.6m;
    private readonly Dictionary<SequenceOrderType, Func<INcProgramRewriteParameter, Task<NcProgramCode>>> _ncProgramRewriters = new()
    {
        { SequenceOrderType.CenterDrilling, CenterDrillingProgramRewriter.RewriteAsync },
        { SequenceOrderType.PilotDrilling, DrillingProgramRewriter.RewriteAsync },
        { SequenceOrderType.Chamfering, ChamferingProgramRewriter.RewriteAsync },
        { SequenceOrderType.Tapping, TappingProgramRewriter.RewriteAsync }
    };
    private readonly TappingParameterExistencePolicy _tappingParameterPolicy = new();
    private readonly DrillingParameterExistencePolicy _drillingParameterPolicy = new();

    [Logging]
    public virtual async Task<IEnumerable<NcProgramCode>> RewriteByToolAsync(ToolParameter toolParameter)
    {
        if (toolParameter.Material == MaterialType.Undefined)
            throw new ArgumentException("素材が未定義です");

        // タップのパラメータを受け取る
        var tappingParameters = toolParameter.TapParameters;

        if (!_tappingParameterPolicy.ComplyWithAll(tappingParameters, toolParameter.DirectedOperationToolDiameter))
            throw new DomainException(
                $"タップ径 {toolParameter.DirectedOperationToolDiameter}のリストがありません");

        var tappingParameter = tappingParameters
            .First(x => x.DirectedOperationToolDiameter == toolParameter.DirectedOperationToolDiameter);

        // ドリルのパラメータを受け取る
        var drillingParameters = toolParameter.DrillingParameters;

        if (!_drillingParameterPolicy.ComplyWithAll(drillingParameters, tappingParameter.PilotHoleDiameter))
            throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {tappingParameter.PilotHoleDiameter}");

        // タップの工程
        SequenceOrder[] sequenceOrders = tappingParameter.PilotHoleDiameter >= chamferingThresholdDrillDiameter
            ? new[]
            {
                new SequenceOrder(SequenceOrderType.CenterDrilling),
                new SequenceOrder(SequenceOrderType.PilotDrilling),
                new SequenceOrder(SequenceOrderType.Tapping),
            }
            : new[]
            {
                new SequenceOrder(SequenceOrderType.CenterDrilling),
                new SequenceOrder(SequenceOrderType.PilotDrilling),
                new SequenceOrder(SequenceOrderType.Chamfering),
                new SequenceOrder(SequenceOrderType.Tapping),
            };

        // メインプログラムを工程ごとに取り出す
        var rewrittenNcPrograms = await Task.WhenAll(sequenceOrders.Select(
            async sequenceOrder => await _ncProgramRewriters[sequenceOrder.SequenceOrderType](
                MakeCenterDrillingRewriteParameter(sequenceOrder, toolParameter))));

        return rewrittenNcPrograms.ToList();
    }

    private static INcProgramRewriteParameter MakeCenterDrillingRewriteParameter(SequenceOrder sequenceOrder, ToolParameter toolParameter)
    {
        return sequenceOrder.SequenceOrderType switch
        {
            SequenceOrderType.CenterDrilling => toolParameter.ToCenterDrillingRewriteParameter(RewriterSelector.Tapping),

            SequenceOrderType.PilotDrilling => toolParameter.ToPilotDrillingRewriteParameter(RewriterSelector.Tapping),

            SequenceOrderType.Chamfering => toolParameter.ToChamferingRewriteParameter(RewriterSelector.Tapping),

            SequenceOrderType.Tapping => toolParameter.ToTappingRewriteParameter(),

            _ => throw new NotImplementedException(),
        };
    }
}
