using System;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process
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
        internal static NcProgramCode Rewrite(
            NcProgramCode rewritableCode,
            MaterialType material,
            ReamerType reamer,
            decimal thickness,
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
                            INcWord result;
                            if (y.GetType() == typeof(NcComment))
                            {
                                NcComment nCComment = (NcComment)y;
                                if (nCComment.Comment == "REAMER")
                                    result = new NcComment(
                                        string.Concat(
                                            nCComment.Comment,
                                            ' ',
                                            rewritingParameter.DirectedOperationToolDiameter));
                                else
                                    result = y;
                            }
                            else if (y.GetType() == typeof(NcWord))
                            {
                                NcWord ncWord = (NcWord)y;
                                if (ncWord.ValueData.Indefinite)
                                    result = ncWord.Address.Value switch
                                    {
                                        'S' => RewriteSpin(material, reamer, rewritingParameter.DirectedOperationToolDiameter, ncWord),
                                        'Z' => RewriteReamingDepth(thickness, ncWord),
                                        'F' => RewriteFeed(material, reamer, rewritingParameter.DirectedOperationToolDiameter, ncWord),
                                        'P' => RewriteSubProgramNumber(subProgramNumber, ncWord),
                                        _ => y
                                    };
                                else
                                    result = y;
                            }
                            else
                                return y;

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
        private static INcWord RewriteFeed(MaterialType material, ReamerType reamer, decimal diameter, NcWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            var spinValue = CalculateReamerSpin(material, reamer, diameter);

            int feedValue = CalculateReamerSpinFeed(material, reamer, spinValue);

            return ncWord with { ValueData = new NumericalValue(feedValue.ToString()) };
        }

        [Logging]
        private static INcWord RewriteReamingDepth(decimal thickness, NcWord ncWord)
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
        private static INcWord RewriteSpin(MaterialType material, ReamerType reamer, decimal diameter, NcWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            var spinValue = CalculateReamerSpin(material, reamer, diameter);

            return ncWord with { ValueData = new NumericalValue(spinValue.ToString()) };
        }

        [Logging]
        private static int CalculateReamerSpinFeed(MaterialType material, ReamerType reamer, decimal spin)
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

            // 整数10の位(decimals: -1)で丸め
            var feedValue = (int)Round(figures, -1, MidpointRounding.AwayFromZero);
            return feedValue;
        }

        [Logging]
        private static int CalculateReamerSpin(MaterialType material, ReamerType reamer, decimal diameter)
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

            // 整数10の位(decimals: -1)で丸め
            var spinValue = (int)Round(figures, -1, MidpointRounding.AwayFromZero);
            return spinValue;
        }

        /// <summary>
        /// 値を四捨五入します
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="decimals">小数部桁数</param>
        /// <param name="mode">丸める方法</param>
        /// <returns>丸められた値</returns>
        [Logging]
        private static decimal Round(decimal value, int decimals,
            MidpointRounding mode)
        {
            // 小数部桁数の10の累乗を取得
            decimal pow = (decimal)Math.Pow(10, decimals);
            return Math.Round(value * pow, mode) / pow;
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
