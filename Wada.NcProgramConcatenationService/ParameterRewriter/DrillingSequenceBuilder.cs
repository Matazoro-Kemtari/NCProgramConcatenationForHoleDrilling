using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

public class DrillingSequenceBuilder : IMainProgramSequenceBuilder
{
    private const decimal chamferingThresholdDrillDiameter = 15.6m;
    private readonly Dictionary<SequenceOrderType, Func<INcProgramRewriteParameter, Task<NcProgramCode>>> _ncProgramRewriters = new()
    {
        { SequenceOrderType.CenterDrilling, CenterDrillingProgramRewriter.RewriteAsync },
        { SequenceOrderType.Drilling, DrillingProgramRewriter.RewriteAsync },
        { SequenceOrderType.Chamfering, ChamferingProgramRewriter.RewriteAsync },
    };
    private readonly DrillingParameterExistencePolicy _parameterPolicy = new();

    [Logging]
    public virtual async Task<IEnumerable<NcProgramCode>> RewriteByToolAsync(ToolParameter toolParameter)
    {
        if (toolParameter.Material == MaterialType.Undefined)
            throw new ArgumentException("素材が未定義です");

        // ドリルのパラメータを受け取る
        var drillingParameters = toolParameter.DrillingParameters;

        if (!_parameterPolicy.ComplyWithAll(drillingParameters, toolParameter.DirectedOperationToolDiameter))
            throw new DomainException(
                $"ドリル径 {toolParameter.DirectedOperationToolDiameter}のリストがありません");

        // ドリルの工程
        SequenceOrder[] sequenceOrders = toolParameter.DirectedOperationToolDiameter >= chamferingThresholdDrillDiameter
            ? new[]
            {
                new SequenceOrder(SequenceOrderType.CenterDrilling),
                new SequenceOrder(SequenceOrderType.Drilling),
            }
            : new[]
            {
                new SequenceOrder(SequenceOrderType.CenterDrilling),
                new SequenceOrder(SequenceOrderType.Drilling),
                new SequenceOrder(SequenceOrderType.Chamfering),
            };

        // メインプログラムを工程ごとに取り出す
        var rewrittenNcPrograms = await Task.WhenAll(sequenceOrders.Select(
            async sequenceOrder =>
            {
                var rewritedProgram = await _ncProgramRewriters[sequenceOrder.SequenceOrderType](
                    MakeCenterDrillingRewriteParameter(sequenceOrder, toolParameter));

                return sequenceOrder.SequenceOrderType == SequenceOrderType.Chamfering
                    ? ReplaceLastM1ToM30(rewritedProgram)
                    : rewritedProgram;
            }));

        return rewrittenNcPrograms.ToList();
    }

    private static INcProgramRewriteParameter MakeCenterDrillingRewriteParameter(SequenceOrder sequenceOrder, ToolParameter toolParameter) => sequenceOrder.SequenceOrderType switch
    {
        SequenceOrderType.CenterDrilling => toolParameter.ToCenterDrillingRewriteParameter(RewriterSelector.Drilling),

        SequenceOrderType.Drilling => toolParameter.ToDrillingRewriteParameter(),

        SequenceOrderType.Chamfering => toolParameter.ToChamferingRewriteParameter(RewriterSelector.Drilling),

        _ => throw new NotImplementedException(),
    };


    /// <summary>
    /// ドリリングの作業指示の時だけ面取りの最後をM1からM30に書き換える
    /// </summary>
    /// <param name="ncProgramCode"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    [Logging]
    public static NcProgramCode ReplaceLastM1ToM30(NcProgramCode ncProgramCode)
    {
        if (ncProgramCode.MainProgramClassification != NcProgramRole.Chamfering)
            throw new ArgumentException("引数に面取り以外のプログラムコードが指定されました");

        bool hasFinded1stWord = false;
        var rewrittenNcBlocks = ncProgramCode.NcBlocks
            .Reverse()
            .Select(x =>
            {
                if (x == null)
                    return null;

                var rewitedNcWords = x.NcWords
                    .Select(y =>
                    {
                        INcWord resuld;
                        if (hasFinded1stWord == false
                        && y.GetType() == typeof(NcWord))
                        {
                            hasFinded1stWord = true;

                            NcWord ncWord = (NcWord)y;
                            if (ncWord.Address.Value == 'M'
                            && ncWord.ValueData.Number == 1)
                                resuld = ncWord with
                                {
                                    ValueData = new NumericalValue("30")
                                };
                            else
                                resuld = y;
                        }
                        else
                            resuld = y;

                        return resuld;
                    })
                    // ここで遅延実行を許すとUnitTestで失敗する
                    .ToList();

                return x with { NcWords = rewitedNcWords };
            })
            .Reverse()
            // ここで遅延実行を許すとプレビューで変更が反映されない
            .ToList();

        return ncProgramCode with
        {
            NcBlocks = rewrittenNcBlocks
        };
    }
}
