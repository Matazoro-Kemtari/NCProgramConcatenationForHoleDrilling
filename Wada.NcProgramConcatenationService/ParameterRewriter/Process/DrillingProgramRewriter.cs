using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process;

internal class DrillingProgramRewriter
{
    /// <summary>
    /// 下穴ドリルのメインプログラムを書き換える
    /// </summary>
    /// <param name="ncProgramRewriteParameter">メインプログラムを書き換え引数用オブジェクト</param>
    /// <returns></returns>
    [Logging]
    internal static async Task<NcProgramCode> RewriteAsync(INcProgramRewriteParameter ncProgramRewriteParameter)
    {
        var drillingRewriteParameter = (DrillingRewriteParameter)ncProgramRewriteParameter;

        // NCプログラムを走査して書き換え対象を探す
        var rewrittenNcBlocks = await Task.WhenAll(drillingRewriteParameter.RewritableCode.NcBlocks
            .Select(async x =>
            {
                if (x == null)
                    return null;

                var rewritedNcWords = await Task.WhenAll(x.NcWords
                    .Select(async y =>
                    {
                        INcWord result;
                        if (y.GetType() == typeof(NcComment))
                        {
                            NcComment nCComment = (NcComment)y;
                            if (nCComment.Comment == "DR")
                                result = new NcComment(
                                    string.Concat(
                                        nCComment.Comment,
                                        ' ',
                                        drillingRewriteParameter.DrillDiameter));
                            else
                                result = y;
                        }
                        else if (y.GetType() == typeof(NcWord))
                        {
                            NcWord ncWord = (NcWord)y;
                            if (ncWord.ValueData.Indefinite)
                                result = await Task.Run(() => ncWord.Address.Value switch
                                {
                                    'S' => RewriteSpin(drillingRewriteParameter.Material, (DrillingProgramParameter)drillingRewriteParameter.RewritingParameter, ncWord),
                                    'Z' => RewriteDrillingDepth(drillingRewriteParameter.DrillingDepth, ncWord),
                                    'Q' => RewriteCutDepth((DrillingProgramParameter)drillingRewriteParameter.RewritingParameter, ncWord),
                                    'F' => RewriteFeed(drillingRewriteParameter.Material, (DrillingProgramParameter)drillingRewriteParameter.RewritingParameter, ncWord),
                                    'P' => RewriteSubProgramNumber(drillingRewriteParameter.SubProgramNumber, ncWord),
                                    _ => y
                                });
                            else
                                result = y;
                        }
                        else
                            result = y;

                        return result;
                    }));

                return new NcBlock(rewritedNcWords, x.HasBlockSkip);
            }));

        return drillingRewriteParameter.RewritableCode with
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
    private static INcWord RewriteFeed(MaterialType material, DrillingProgramParameter drillingParameter, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        string feedValue = material switch
        {
            MaterialType.Aluminum => drillingParameter.FeedForAluminum.ToString(),
            MaterialType.Iron => drillingParameter.FeedForIron.ToString(),
            _ => throw new AggregateException(nameof(material)),
        };

        return ncWord with
        {
            ValueData = new NumericalValue(feedValue)
        };
    }

    [Logging]
    private static INcWord RewriteCutDepth(DrillingProgramParameter drillingParameter, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        return ncWord with
        {
            ValueData = new CoordinateValue(
                AddDecimalPoint(drillingParameter.CutDepth.ToString()))
        };
    }

    [Logging]
    private static INcWord RewriteDrillingDepth(decimal drillingDepth, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        return ncWord with
        {
            // 板厚＋刃先の長さ
            ValueData = new CoordinateValue(
                AddDecimalPoint(
                    Convert.ToString(-drillingDepth)))
        };
    }

    [Logging]
    private static INcWord RewriteSpin(MaterialType material, DrillingProgramParameter drillingParameter, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        string spinValue = material switch
        {
            MaterialType.Aluminum => drillingParameter.SpinForAluminum.ToString(),
            MaterialType.Iron => drillingParameter.SpinForIron.ToString(),
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
