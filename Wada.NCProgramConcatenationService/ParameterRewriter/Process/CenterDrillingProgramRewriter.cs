using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Process
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
        internal static NCProgramCode Rewrite(
            NCProgramCode rewritableCode,
            MaterialType material,
            IMainProgramPrameter rewritingParameter,
            string subProgramNumber)
        {
            // NCプログラムを走査して書き換え対象を探す
            var rewritedNCBlocks = rewritableCode.NCBlocks
                .Select(x =>
                {
                    if (x == null)
                        return null;

                    var rewritedNCWords = x.NCWords
                            .Select(y =>
                            {
                                if (y.GetType() != typeof(NCWord))
                                    return y;

                                NCWord ncWord = (NCWord)y;
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

                    return new NCBlock(rewritedNCWords, x.HasBlockSkip);
                });

            return rewritableCode with
            {
                NCBlocks = rewritedNCBlocks
            };
        }

        [Logging]
        private static INCWord RewriteSubProgramNumber(string subProgramNumber, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            return ncWord with { ValueData = new NumericalValue(subProgramNumber) };
        }

        [Logging]
        private static INCWord RewriteFeed(MaterialType material, NCWord ncWord)
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
        private static INCWord RewriteCenterDrillDepth(decimal centerDrillDepth, NCWord ncWord)
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
        private static INCWord RewriteSpin(MaterialType material, NCWord ncWord)
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
