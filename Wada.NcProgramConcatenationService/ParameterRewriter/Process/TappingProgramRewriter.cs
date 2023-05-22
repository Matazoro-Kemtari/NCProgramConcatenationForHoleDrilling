using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process
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
        internal static NcProgramCode Rewrite(
            NcProgramCode rewritableCode,
            MaterialType material,
            decimal thickness,
            IMainProgramParameter rewritingParameter,
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
                            INcWord result;
                            if (y.GetType() == typeof(NcComment))
                            {
                                NcComment nCComment = (NcComment)y;
                                if (nCComment.Comment == "TAP")
                                    result = new NcComment(
                                        string.Concat(
                                            nCComment.Comment,
                                            " M",
                                            rewritingParameter.DirectedOperationToolDiameter));
                                else
                                    result = y;
                            }
                            else if (y.GetType() == typeof(NcWord))
                            {
                                TappingProgramParameter tappingProgramParameter = (TappingProgramParameter)rewritingParameter;
                                NcWord ncWord = (NcWord)y;
                                if (ncWord.ValueData.Indefinite)
                                    result = ncWord.Address.Value switch
                                    {
                                        'S' => RewriteSpin(material, tappingProgramParameter, ncWord),
                                        'Z' => RewriteTappingDepth(thickness, ncWord),
                                        'F' => RewriteFeed(material, tappingProgramParameter, ncWord),
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
        private static INcWord RewriteFeed(MaterialType material, TappingProgramParameter tappingProgramParameter, NcWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            var feedValue = material switch
            {
                MaterialType.Aluminum => tappingProgramParameter.FeedForAluminum.ToString(),
                MaterialType.Iron => tappingProgramParameter.FeedForIron.ToString(),
                _ => throw new AggregateException(nameof(material)),
            };

            return ncWord with { ValueData = new NumericalValue(feedValue) };
        }

        [Logging]
        private static INcWord RewriteTappingDepth(decimal thickness, NcWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            return ncWord with
            {
                ValueData = new CoordinateValue(
                    AddDecimalPoint(Convert.ToString(-(thickness + 5m))))
            };
        }

        [Logging]
        private static INcWord RewriteSpin(MaterialType material, TappingProgramParameter tappingParameter, NcWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            var spinValue = material switch
            {
                MaterialType.Aluminum => tappingParameter.SpinForAluminum.ToString(),
                MaterialType.Iron => tappingParameter.SpinForIron.ToString(),
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
