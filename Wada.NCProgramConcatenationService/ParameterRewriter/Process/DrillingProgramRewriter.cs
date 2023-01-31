using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Process
{
    internal class DrillingProgramRewriter
    {
        /// <summary>
        /// 下穴ドリルのメインプログラムを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="diameter"></param>
        /// <param name="thickness"></param>
        /// <param name="drillingParameter"></param>
        /// <param name="subProgramNumber"></param>
        /// <param name="drillDiameter">実際に使用するドリル径</param>
        /// <returns></returns>
        [Logging]
        internal static NCProgramCode Rewrite(
            NCProgramCode rewritableCode,
            MaterialType material,
            decimal thickness,
            DrillingProgramPrameter drillingParameter,
            string subProgramNumber,
            decimal drillDiameter)
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
                                INCWord result;
                                if (y.GetType() == typeof(NCComment))
                                {
                                    NCComment nCComment = (NCComment)y;
                                    if (nCComment.Comment == "DR")
                                        result = new NCComment(
                                            string.Concat(
                                                nCComment.Comment,
                                                ' ',
                                                drillDiameter));
                                    else
                                        result = y;
                                }
                                else if (y.GetType() == typeof(NCWord))
                                {
                                    NCWord ncWord = (NCWord)y;
                                    if (ncWord.ValueData.Indefinite)
                                        result = ncWord.Address.Value switch
                                        {
                                            'S' => RewriteSpin(material, drillingParameter, ncWord),
                                            'Z' => RewriteDrillingDepth(thickness, drillingParameter, ncWord),
                                            'Q' => RewriteCutDepth(drillingParameter, ncWord),
                                            'F' => RewriteFeed(material, drillingParameter, ncWord),
                                            'P' => RewriteSubProgramNumber(subProgramNumber, ncWord),
                                            _ => y
                                        };
                                    else
                                        result = y;
                                }
                                else
                                    result = y;

                                return result;
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
        private static INCWord RewriteFeed(MaterialType material, DrillingProgramPrameter drillingParameter, NCWord ncWord)
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
        private static INCWord RewriteCutDepth(DrillingProgramPrameter drillingParameter, NCWord ncWord)
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
        private static INCWord RewriteDrillingDepth(decimal thickness, DrillingProgramPrameter drillingParameter, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            return ncWord with
            {
                // 板厚＋刃先の長さ
                ValueData = new CoordinateValue(
                    AddDecimalPoint(
                        Convert.ToString(-(thickness + drillingParameter.DrillTipLength))))
            };
        }

        [Logging]
        private static INCWord RewriteSpin(MaterialType material, DrillingProgramPrameter drillingParameter, NCWord ncWord)
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
}
