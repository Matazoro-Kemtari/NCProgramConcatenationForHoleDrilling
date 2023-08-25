using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process;

internal class TappingProgramRewriter
{
    /// <summary>
    /// タップのメインプログラムを書き換える
    /// </summary>
    /// <param name="ncProgramRewriteParameter">メインプログラムを書き換え引数用オブジェクト</param>
    /// <returns></returns>
    [Logging]
    internal static async Task<NcProgramCode> RewriteAsync(INcProgramRewriteParameter ncProgramRewriteParameter)
    {
        var tappingRewriteParameter = (TappingRewriteParameter)ncProgramRewriteParameter;

        // NCプログラムを走査して書き換え対象を探す
        var rewrittenNcBlocks = await Task.WhenAll(tappingRewriteParameter.RewritableCode.NcBlocks
            .Select(async x =>
            {
                if (x == null)
                    return null;

                var rewritedNcWords = await Task.WhenAll(x.NcWords
                    .Select(async y => await Task.Run(() =>
                    {
                        if (y.GetType() == typeof(NcComment))
                        {
                            NcComment nCComment = (NcComment)y;
                            if (nCComment.Comment == "TAP")
                                return new NcComment(
                                    string.Concat(
                                        nCComment.Comment,
                                        " M",
                                        tappingRewriteParameter.RewritingParameter.DirectedOperationToolDiameter));
                            else
                                return y;
                        }
                        else if (y.GetType() == typeof(NcWord))
                        {
                            var tappingProgramParameter = (TappingProgramParameter)tappingRewriteParameter.RewritingParameter;
                            NcWord ncWord = (NcWord)y;
                            if (ncWord.ValueData.Indefinite)
                                return ncWord.Address.Value switch
                                {
                                    'S' => RewriteSpin(tappingRewriteParameter.Material, tappingProgramParameter, ncWord),
                                    'Z' => RewriteTappingDepth(tappingRewriteParameter.TappingDepth, ncWord),
                                    'F' => RewriteFeed(tappingRewriteParameter.Material, tappingProgramParameter, ncWord),
                                    'P' => RewriteSubProgramNumber(tappingRewriteParameter.SubProgramNumber, ncWord),
                                    _ => y
                                };
                            else
                                return y;
                        }
                        else
                            return y;
                    })));

                return new NcBlock(rewritedNcWords, x.HasBlockSkip);
            }));

        return tappingRewriteParameter.RewritableCode with
        {
            NcBlocks = rewrittenNcBlocks
        };
    }

    [Logging]
    private static INcWord RewriteSubProgramNumber(string subProgramNumber, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        return ncWord with { ValueData = new NumericalValue(subProgramNumber) };
    }

    [Logging]
    private static INcWord RewriteFeed(MaterialType material, TappingProgramParameter tappingProgramParameter, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        var feedValue = material switch
        {
            MaterialType.Aluminum => tappingProgramParameter.FeedForAluminum.ToString(),
            MaterialType.Iron => tappingProgramParameter.FeedForIron.ToString(),
            _ => throw new AggregateException(nameof(material)),
        };

        return ncWord with { ValueData = new NumericalValue(feedValue) };
    }

    [Logging]
    private static INcWord RewriteTappingDepth(decimal tappingDepth, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        return ncWord with
        {
            ValueData = new CoordinateValue(
                AddDecimalPoint(Convert.ToString(-tappingDepth)))
        };
    }

    [Logging]
    private static INcWord RewriteSpin(MaterialType material, TappingProgramParameter tappingParameter, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        var spinValue = material switch
        {
            MaterialType.Aluminum => tappingParameter.SpinForAluminum.ToString(),
            MaterialType.Iron => tappingParameter.SpinForIron.ToString(),
            _ => throw new AggregateException(nameof(material)),
        };

        return ncWord with { ValueData = new NumericalValue(spinValue) };
    }

    /// <summary>
    /// 座標数値はドットがないと1/1000されるためドットを付加
    /// パラメータリストはドットが省略されている
    /// </summary>
    /// <param name="value">座標値</param>
    /// <returns></returns>
    [Logging]

    static string AddDecimalPoint(string value)
    {
        if (!value.Contains('.'))
            value += ".";
        return value;
    }
}
