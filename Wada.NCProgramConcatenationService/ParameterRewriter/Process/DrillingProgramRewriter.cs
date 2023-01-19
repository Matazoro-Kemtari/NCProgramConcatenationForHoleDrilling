using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Process
{
    internal class DrillingProgramRewriter
    {
        /// <summary>
        /// 下穴ドリルのパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="diameter"></param>
        /// <param name="thickness"></param>
        /// <param name="drillingParameter"></param>
        /// <returns></returns>
        [Logging]
        internal static NCProgramCode Rewrite(
            NCProgramCode rewritableCode,
            MaterialType material,
            decimal diameter,
            decimal thickness,
            DrillingProgramPrameter drillingParameter)
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
                                return ncWord.Address.Value switch
                                {
                                    'S' => RewriteSpin(material, drillingParameter, ncWord),
                                    'Z' => RewriteDrillDepth(thickness, drillingParameter, ncWord),
                                    'Q' => RewriteCutDepth(drillingParameter, ncWord),
                                    'F' => RewriteFeed(material, drillingParameter, ncWord),
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

        private static INCWord RewriteFeed(MaterialType material, DrillingProgramPrameter drillingParameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var feed = (NumericalValue)ncWord.ValueData;
            return ncWord with
            {
                ValueData = RewriteFeedValueData(material, drillingParameter, feed)
            };
        }

        private static INCWord RewriteCutDepth(DrillingProgramPrameter drillingParameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var cutDepth = (CoordinateValue)ncWord.ValueData;

            return ncWord with { ValueData = RewriteCutDepthValueData(drillingParameter, cutDepth) };
        }

        private static INCWord RewriteDrillDepth(decimal thickness, DrillingProgramPrameter drillingParameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var depth = (CoordinateValue)ncWord.ValueData;
            return ncWord with
            {
                ValueData = RewriteDrillDepthValueData(thickness, drillingParameter, depth)
            };
        }

        private static INCWord RewriteSpin(MaterialType material, DrillingProgramPrameter drillingParameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var spin = (NumericalValue)ncWord.ValueData;
            return ncWord with { ValueData = RewriteSpinValueData(material, drillingParameter, spin) };
        }

        [Logging]
        private static IValueData RewriteFeedValueData(MaterialType material, DrillingProgramPrameter drillingParameter, NumericalValue valueData)
        {
            string feedValue = material switch
            {
                MaterialType.Aluminum => drillingParameter.FeedForAluminum.ToString(),
                MaterialType.Iron => drillingParameter.FeedForIron.ToString(),
                _ => throw new AggregateException(nameof(material)),
            };
            return valueData with { Value = feedValue };
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

        [Logging]
        private static IValueData RewriteCutDepthValueData(DrillingProgramPrameter drillingParameter, CoordinateValue valueData)
        {
            return valueData with { Value = AddDecimalPoint(drillingParameter.CutDepth.ToString()) };
        }

        [Logging]
        private static IValueData RewriteDrillDepthValueData(decimal thickness, DrillingProgramPrameter drillingParameter, CoordinateValue valueData)
        {
            // 板厚＋刃先の長さ
            return valueData with { Value = AddDecimalPoint(Convert.ToString(-(thickness + drillingParameter.DrillTipLength))) };
        }

        [Logging]
        private static IValueData RewriteSpinValueData(MaterialType material, DrillingProgramPrameter drillingParameter, NumericalValue valueData)
        {
            string spinValue = material switch
            {
                MaterialType.Aluminum => drillingParameter.SpinForAluminum.ToString(),
                MaterialType.Iron => drillingParameter.SpinForIron.ToString(),
                _ => throw new AggregateException(nameof(material)),
            };
            return valueData with { Value = spinValue };
        }
    }
}
