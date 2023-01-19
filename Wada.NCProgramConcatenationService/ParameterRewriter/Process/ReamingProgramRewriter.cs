using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Process
{
    internal class ReamingProgramRewriter
    {
        /// <summary>
        /// リーマのメインプログラムを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="reamer"></param>
        /// <param name="thickness"></param>
        /// <param name="rewritingParameter">対象のパラメータ</param>
        /// <returns></returns>
        [Logging]
        internal static NCProgramCode Rewrite(
            NCProgramCode rewritableCode,
            MaterialType material,
            ReamerType reamer,
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

                            NCWord ncWord = (NCWord)y;
                            return ncWord.Address.Value switch
                            {
                                'S' => RewriteSpin(material, reamer, rewritingParameter.TargetToolDiameter, ncWord),
                                'Z' => RewriteReamingDepth(thickness, ncWord),
                                'F' => RewriteFeed(material, reamer, rewritingParameter.TargetToolDiameter, ncWord),
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
        private static INCWord RewriteFeed(MaterialType material, ReamerType reamer, decimal diameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var spin = new NumericalValue("*");
            var spinValue = RewriteSpinValueData(material, reamer, diameter, spin);
            var feed = (NumericalValue)ncWord.ValueData;
            return ncWord with
            {
                ValueData = RewriteFeedValueData(material, reamer, spinValue.Number, feed)
            };
        }

        [Logging]
        private static INCWord RewriteReamingDepth(decimal thickness, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var depth = (CoordinateValue)ncWord.ValueData;
            return ncWord with { ValueData = RewriteReamingDepthValueData(thickness, depth) };
        }

        [Logging]
        private static INCWord RewriteSpin(MaterialType material, ReamerType reamer, decimal diameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var spin = (NumericalValue)ncWord.ValueData;
            return ncWord with { ValueData = RewriteSpinValueData(material, reamer, diameter, spin) };
        }

        [Logging]
        private static IValueData RewriteFeedValueData(MaterialType material, ReamerType reamer, decimal spin, NumericalValue valueData)
        {
            decimal figures = material switch
            {
                MaterialType.Aluminum => reamer switch
                {
                    ReamerType.CrystalReamerParameter => spin * 0.2m,
                    ReamerType.SkillReamerParameter => spin * 0.12m,
                    _ => throw new NotImplementedException(),
                },
                MaterialType.Iron => reamer switch
                {
                    ReamerType.CrystalReamerParameter => spin * 0.15m,
                    ReamerType.SkillReamerParameter => spin * 0.12m,
                    _ => throw new NotImplementedException(),
                },
                _ => throw new AggregateException(nameof(material)),
            };
            var feedValue = Math.Round(figures, 2, MidpointRounding.AwayFromZero).ToString();
            return valueData with { Value = feedValue };
        }

        [Logging]
        private static IValueData RewriteReamingDepthValueData(decimal thickness, CoordinateValue valueData)
        {
            return valueData with { Value = Convert.ToString(-(thickness + 5m)) };
        }

        [Logging]
        private static IValueData RewriteSpinValueData(MaterialType material, ReamerType reamer, decimal diameter, NumericalValue valueData)
        {
            decimal figures = material switch
            {
                MaterialType.Aluminum => reamer switch
                {
                    ReamerType.CrystalReamerParameter => 5100 / diameter,
                    ReamerType.SkillReamerParameter => 15000 / diameter,
                    _ => throw new NotImplementedException(),
                },
                MaterialType.Iron => reamer switch
                {
                    ReamerType.CrystalReamerParameter => 3800 / diameter,
                    ReamerType.SkillReamerParameter => 4800 / diameter,
                    _ => throw new NotImplementedException(),
                },
                _ => throw new AggregateException(nameof(material)),
            };
            var spinValue = Math.Round(figures, 2, MidpointRounding.AwayFromZero).ToString();
            return valueData with { Value = spinValue };
        }
    }
}
