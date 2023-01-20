using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Process
{
    internal class TappingProgramRewriter
    {
        /// <summary>
        /// タップのメインプログラムを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="thickness"></param>
        /// <param name="rewritingParameter">対象のパラメータ</param>
        /// <returns></returns>
        [Logging]
        internal static NCProgramCode Rewrite(
            NCProgramCode rewritableCode,
            MaterialType material,
            decimal thickness,
            IMainProgramPrameter rewritingParameter)
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

                            TappingProgramPrameter tappingProgramPrameter = (TappingProgramPrameter)rewritingParameter;
                            NCWord ncWord = (NCWord)y;
                            return ncWord.Address.Value switch
                            {
                                'S' => RewriteSpin(material, tappingProgramPrameter, ncWord),
                                'Z' => RewriteTappingDepth(thickness, ncWord),
                                'F' => RewriteFeed(material, tappingProgramPrameter, ncWord),
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
        private static INCWord RewriteFeed(MaterialType material, TappingProgramPrameter tappingProgramPrameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var feed = (NumericalValue)ncWord.ValueData;
            return ncWord with
            {
                ValueData = RewriteFeedValueData(material, tappingProgramPrameter, feed)
            };
        }

        [Logging]
        private static INCWord RewriteTappingDepth(decimal thickness, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var depth = (CoordinateValue)ncWord.ValueData;
            return ncWord with { ValueData = RewriteTappingDepthValueData(thickness, depth) };
        }

        [Logging]
        private static INCWord RewriteSpin(MaterialType material, TappingProgramPrameter TappingParameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var spin = (NumericalValue)ncWord.ValueData;
            return ncWord with { ValueData = RewriteSpinValueData(material, TappingParameter, spin) };
        }

        [Logging]
        private static IValueData RewriteFeedValueData(MaterialType material, TappingProgramPrameter tappingProgramPrameter, NumericalValue valueData)
        {
            var feedValue = material switch
            {
                MaterialType.Aluminum => tappingProgramPrameter.FeedForAluminum.ToString(),
                MaterialType.Iron => tappingProgramPrameter.FeedForIron.ToString(),
                _ => throw new AggregateException(nameof(material)),
            };
            return valueData with { Value = feedValue };
        }

        [Logging]
        private static IValueData RewriteTappingDepthValueData(decimal thickness, CoordinateValue valueData)
        {
            return valueData with { Value = Convert.ToString(-(thickness + 5m)) };
        }

        [Logging]
        private static IValueData RewriteSpinValueData(MaterialType material, TappingProgramPrameter tappingProgramPrameter, NumericalValue valueData)
        {
            var spinValue = material switch
            {
                MaterialType.Aluminum => tappingProgramPrameter.SpinForAluminum.ToString(),
                MaterialType.Iron => tappingProgramPrameter.SpinForIron.ToString(),
                _ => throw new AggregateException(nameof(material)),
            };
            return valueData with { Value = spinValue };
        }
    }
}
