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
    /// <param name="ncProgramRewriteArg">メインプログラムを書き換え引数用オブジェクト</param>
    /// <returns></returns>
    [Logging]
    internal static NcProgramCode Rewrite(INcProgramRewriteArg ncProgramRewriteArg)
    {
        var drillingRewriteArg = (DrillingRewriteArg)ncProgramRewriteArg;

        // NCプログラムを走査して書き換え対象を探す
        var rewrittenNcBlocks = drillingRewriteArg.RewritableCode.NcBlocks
            .Select(x =>
            {
                if (x == null)
                    return null;

                var rewritedNcWords = x.NcWords
                    .Select(y =>
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
                                        drillingRewriteArg.DrillDiameter));
                            else
                                result = y;
                        }
                        else if (y.GetType() == typeof(NcWord))
                        {
                            NcWord ncWord = (NcWord)y;
                            if (ncWord.ValueData.Indefinite)
                                result = ncWord.Address.Value switch
                                {
                                    'S' => RewriteSpin(drillingRewriteArg.Material, (DrillingProgramParameter)drillingRewriteArg.RewritingParameter, ncWord),
                                    'Z' => RewriteDrillingDepth(drillingRewriteArg.DrillingDepth, ncWord),
                                    'Q' => RewriteCutDepth((DrillingProgramParameter)drillingRewriteArg.RewritingParameter, ncWord),
                                    'F' => RewriteFeed(drillingRewriteArg.Material, (DrillingProgramParameter)drillingRewriteArg.RewritingParameter, ncWord),
                                    'P' => RewriteSubProgramNumber(drillingRewriteArg.SubProgramNumber, ncWord),
                                    _ => y
                                };
                            else
                                result = y;
                        }
                        else
                            result = y;

                        return result;
                    });

                return new NcBlock(rewritedNcWords, x.HasBlockSkip);
            });

        return drillingRewriteArg.RewritableCode with
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

internal record class DrillingRewriteArg(
    NcProgramCode RewritableCode,
    MaterialType Material,
    decimal DrillingDepth,
    IMainProgramParameter RewritingParameter,
    string SubProgramNumber,
    decimal DrillDiameter) : INcProgramRewriteArg;
