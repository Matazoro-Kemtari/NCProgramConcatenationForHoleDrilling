using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    internal class CenterDrillingProgramRewriter
    {
        /// <summary>
        /// センタードリルのパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="rewritingParameter">対象のパラメータ</param>
        /// <returns></returns>
        [Logging]
        internal static NCProgramCode Rewrite(
            NCProgramCode rewritableCode,
            MaterialType material,
            IMainProgramPrameter rewritingParameter)
        {
            // NCプログラムを走査して書き換え対象を探す
            var rewritedNCBlocks = rewritableCode.NCBlocks
                .Select(x =>
                {
                    return x == null
                        ? null
                        : new NCBlock(
                        x.NCWords.Select(y =>
                        {
                            if (y.GetType() != typeof(NCWord))
                                return y;

                            NCWord ncWord = (NCWord)y;
                            if (ncWord.Address.Value == 'S')
                            {
                                // 回転
                                return ncWord with
                                {
                                    ValueData = ncWord.ValueData.Indefinite ?
                                    RewriteSpinParameter(material, (NumericalValue)ncWord.ValueData)
                                    : ncWord.ValueData
                                };
                            }
                            else if (ncWord.Address.Value == 'Z')
                            {
                                // C/D深さ
                                return ncWord with
                                {
                                    ValueData = ncWord.ValueData.Indefinite ?
                                    RewriteCenterDrillDepthParameter(rewritingParameter.CenterDrillDepth, (CoordinateValue)ncWord.ValueData)
                                    : ncWord.ValueData
                                };
                            }
                            else if (ncWord.Address.Value == 'F')
                            {
                                // 送り
                                return ncWord with
                                {
                                    ValueData = ncWord.ValueData.Indefinite ?
                                    RewriteFeedParameter(material, (NumericalValue)ncWord.ValueData)
                                    : ncWord.ValueData
                                };
                            }
                            else
                                return y;
                        }),
                        x.HasBlockSkip);
                });

            return rewritableCode with
            {
                NCBlocks = rewritedNCBlocks
            };
        }

        [Logging]
        private static IValueData RewriteFeedParameter(MaterialType material, NumericalValue valueData)
        {
            string feedValue;
            switch (material)
            {
                case MaterialType.Aluminum:
                    feedValue = "150";
                    break;
                case MaterialType.Iron:
                    feedValue = "100";
                    break;
                default:
                    throw new AggregateException(nameof(material));
            }
            return valueData with { Value = feedValue };
        }

        [Logging]
        private static IValueData RewriteCenterDrillDepthParameter(decimal centerDrillDepth, CoordinateValue valueData)
        {
            return valueData with { Value = centerDrillDepth.ToString() };
        }

        [Logging]
        private static IValueData RewriteSpinParameter(MaterialType material, NumericalValue valueData)
        {
            string spinValue;
            switch (material)
            {
                case MaterialType.Aluminum:
                    spinValue = "2000";
                    break;
                case MaterialType.Iron:
                    spinValue = "1500";
                    break;
                default:
                    throw new AggregateException(nameof(material));
            }
            return valueData with { Value = spinValue };
        }
    }
}
