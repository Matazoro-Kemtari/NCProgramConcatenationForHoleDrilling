using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process
{
    internal class CenterDrillingProgramRewriter
    {
        /// <summary>
        /// センタードリルのメインプログラムを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="rewritingParameter">対象のパラメータ</param>
        /// <returns></returns>
        [Logging]
        internal static NcProgramCode Rewrite(
            NcProgramCode rewritableCode,
            MaterialType material,
            IMainProgramPrameter rewritingParameter,
            string subProgramNumber)
        {
            // NCプログラムを走査して書き換え対象を探す
            var rewrittenNcBlocks = rewritableCode.NcBlocks
                .Select(x =>
                {
                    if (x == null)
                        return null;

                    var rewritedNcWords = x.NcWords
                            .Select(y =>
                            {
                                if (y.GetType() != typeof(NcWord))
                                    return y;

                                NcWord ncWord = (NcWord)y;
                                if (!ncWord.ValueData.Indefinite)
                                    return y;

                                return ncWord.Address.Value switch
                                {
                                    'S' => RewriteSpin(material, ncWord),
                                    'Z' => RewriteCenterDrillDepth(rewritingParameter.CenterDrillDepth, ncWord),
                                    'F' => RewriteFeed(material, ncWord),
                                    'P' => RewriteSubProgramNumber(subProgramNumber, ncWord),
                                    _ => y
                                };
                            });

                    return new NcBlock(rewritedNcWords, x.HasBlockSkip);
                });

            return rewritableCode with
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
        private static INcWord RewriteFeed(MaterialType material, NcWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            string feedValue = material switch
            {
                MaterialType.Aluminum => "150",
                MaterialType.Iron => "100",
                _ => throw new AggregateException(nameof(material)),
            };

            return ncWord with { ValueData = new NumericalValue(feedValue) };
        }

        [Logging]
        private static INcWord RewriteCenterDrillDepth(decimal centerDrillDepth, NcWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            return ncWord with
            {
                ValueData = new CoordinateValue(
                    AddDecimalPoint(centerDrillDepth.ToString()))
            };
        }

        [Logging]
        private static INcWord RewriteSpin(MaterialType material, NcWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            string spinValue = material switch
            {
                MaterialType.Aluminum => "2000",
                MaterialType.Iron => "1500",
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
}
