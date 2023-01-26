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

                            TappingProgramPrameter tappingProgramPrameter = (TappingProgramPrameter)rewritingParameter;
                            NCWord ncWord = (NCWord)y;
                            if (!ncWord.ValueData.Indefinite)
                                return y;

                            return ncWord.Address.Value switch
                            {
                                'S' => RewriteSpin(material, tappingProgramPrameter, ncWord),
                                'Z' => RewriteTappingDepth(thickness, ncWord),
                                'F' => RewriteFeed(material, tappingProgramPrameter, ncWord),
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
        private static INCWord RewriteFeed(MaterialType material, TappingProgramPrameter tappingProgramPrameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            return ncWord with
            {
                ValueData = RewriteFeedValueData(material, tappingProgramPrameter)
            };
        }

        [Logging]
        private static INCWord RewriteTappingDepth(decimal thickness, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            return ncWord with { ValueData = RewriteTappingDepthValueData(thickness) };
        }

        [Logging]
        private static INCWord RewriteSpin(MaterialType material, TappingProgramPrameter TappingParameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            return ncWord with { ValueData = RewriteSpinValueData(material, TappingParameter) };
        }

        [Logging]
        private static IValueData RewriteFeedValueData(MaterialType material, TappingProgramPrameter tappingProgramPrameter)
        {
            var feedValue = material switch
            {
                MaterialType.Aluminum => tappingProgramPrameter.FeedForAluminum.ToString(),
                MaterialType.Iron => tappingProgramPrameter.FeedForIron.ToString(),
                _ => throw new AggregateException(nameof(material)),
            };
            return new NumericalValue(feedValue);
        }

        [Logging]
        private static IValueData RewriteTappingDepthValueData(decimal thickness)
        {
            return new CoordinateValue(Convert.ToString(-(thickness + 5m)));
        }

        [Logging]
        private static IValueData RewriteSpinValueData(MaterialType material, TappingProgramPrameter tappingProgramPrameter)
        {
            var spinValue = material switch
            {
                MaterialType.Aluminum => tappingProgramPrameter.SpinForAluminum.ToString(),
                MaterialType.Iron => tappingProgramPrameter.SpinForIron.ToString(),
                _ => throw new AggregateException(nameof(material)),
            };
            return new NumericalValue(spinValue);
        }
    }
}
