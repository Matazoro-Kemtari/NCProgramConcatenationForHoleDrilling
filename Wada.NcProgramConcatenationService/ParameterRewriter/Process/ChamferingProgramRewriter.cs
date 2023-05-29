using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process;

internal class ChamferingProgramRewriter
{
    /// <summary>
    /// 面取りのメインプログラムを書き換える
    /// </summary>
    /// <param name="ncProgramRewriteParameter">メインプログラムを書き換え引数用オブジェクト</param>
    /// <returns></returns>
    [Logging]
    internal static async Task<NcProgramCode> RewriteAsync(INcProgramRewriteParameter ncProgramRewriteParameter)
    {
        // NCプログラムを走査して書き換え対象を探す
        var rewrittenNcBlocks = await Task.WhenAll(ncProgramRewriteParameter.RewritableCode.NcBlocks
            .Select(async x =>
            {
                if (x == null)
                    return null;

                if (ncProgramRewriteParameter.RewritingParameter.ChamferingDepth == null)
                    throw new InvalidOperationException("面取りが無いのに呼び出された");

                var rewritedNcWords = await Task.WhenAll(x.NcWords
                    .Select(async y =>
                    {
                        if (y.GetType() != typeof(NcWord))
                            return y;

                        NcWord ncWord = (NcWord)y;
                        if (!ncWord.ValueData.Indefinite)
                            return y;

                        return await Task.Run(() => ncWord.Address.Value switch
                        {
                            'S' => RewriteSpin(ncProgramRewriteParameter.Material, ncWord),
                            'Z' => RewriteChamferingDepth(ncProgramRewriteParameter.RewritingParameter.ChamferingDepth.Value, ncWord),
                            'P' => RewriteSubProgramNumber(ncProgramRewriteParameter.SubProgramNumber, ncWord),
                            _ => y
                        });
                    }));

                return new NcBlock(rewritedNcWords, x.HasBlockSkip);
            }));

        return ncProgramRewriteParameter.RewritableCode with
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
    private static INcWord RewriteChamferingDepth(decimal chamferDepth, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        return ncWord with
        {
            ValueData = new CoordinateValue(
                AddDecimalPoint(chamferDepth.ToString()))
        };
    }

    [Logging]
    private static INcWord RewriteSpin(MaterialType material, NcWord ncWord)
    {
        if (!ncWord.ValueData.Indefinite)
            return ncWord;

        string spinValue = material switch
        {
            MaterialType.Aluminum => "1400",
            MaterialType.Iron => "1100",
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
