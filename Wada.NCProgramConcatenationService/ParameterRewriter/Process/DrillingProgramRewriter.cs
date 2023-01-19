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
                                    RewriteSpinParameter(material, drillingParameter, (NumericalValue)ncWord.ValueData)
                                    : ncWord.ValueData
                                };
                            }
                            else if (ncWord.Address.Value == 'Z')
                            {
                                // Drill深さ
                                return ncWord with
                                {
                                    ValueData = ncWord.ValueData.Indefinite ?
                                    RewriteDrillDepthParameter(thickness, drillingParameter, (CoordinateValue)ncWord.ValueData)
                                    : ncWord.ValueData
                                };
                            }
                            else if (ncWord.Address.Value == 'Q')
                            {
                                // 切込
                                return ncWord with
                                {
                                    ValueData = ncWord.ValueData.Indefinite ?
                                    RewriteCutDepthParameter(drillingParameter, (CoordinateValue)ncWord.ValueData)
                                    : ncWord.ValueData
                                };
                            }
                            else if (ncWord.Address.Value == 'F')
                            {
                                // 送り
                                return ncWord with
                                {
                                    ValueData = ncWord.ValueData.Indefinite ?
                                    RewriteFeedParameter(material, drillingParameter, (NumericalValue)ncWord.ValueData)
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
        private static IValueData RewriteFeedParameter(MaterialType material, DrillingProgramPrameter drillingParameter, NumericalValue valueData)
        {
            string feedValue;
            switch (material)
            {
                case MaterialType.Aluminum:
                    feedValue = drillingParameter.FeedForAluminum.ToString();
                    break;
                case MaterialType.Iron:
                    feedValue = drillingParameter.FeedForIron.ToString();
                    break;
                default:
                    throw new AggregateException(nameof(material));
            }
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
        private static IValueData RewriteCutDepthParameter(DrillingProgramPrameter drillingParameter, CoordinateValue valueData)
        {
            return valueData with { Value = AddDecimalPoint(drillingParameter.CutDepth.ToString()) };
        }

        [Logging]
        private static IValueData RewriteDrillDepthParameter(decimal thickness, DrillingProgramPrameter drillingParameter, CoordinateValue valueData)
        {
            // 板厚＋刃先の長さ
            return valueData with { Value = AddDecimalPoint(Convert.ToString(-(thickness + drillingParameter.DrillTipLength))) };
        }

        [Logging]
        private static IValueData RewriteSpinParameter(MaterialType material, DrillingProgramPrameter drillingParameter, NumericalValue valueData)
        {
            string spinValue;
            switch (material)
            {
                case MaterialType.Aluminum:
                    spinValue = drillingParameter.SpinForAluminum.ToString();
                    break;
                case MaterialType.Iron:
                    spinValue = drillingParameter.SpinForIron.ToString();
                    break;
                default:
                    throw new AggregateException(nameof(material));
            }
            return valueData with { Value = spinValue };
        }
    }
}
