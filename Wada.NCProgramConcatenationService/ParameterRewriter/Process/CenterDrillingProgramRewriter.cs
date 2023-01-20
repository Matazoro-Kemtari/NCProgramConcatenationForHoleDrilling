﻿using Wada.AOP.Logging;
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
                                    'S' => RewriteSpin(material, ncWord),
                                    'Z' => RewriteCenterDrillDepth(rewritingParameter.CenterDrillDepth, ncWord),
                                    'F' => RewriteFeed(material, ncWord),
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
        private static INCWord RewriteFeed(MaterialType material, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var feed = (NumericalValue)ncWord.ValueData;
            return ncWord with { ValueData = RewriteFeedValueData(material, feed) };
        }

        [Logging]
        private static INCWord RewriteCenterDrillDepth(decimal centerDrillDepth, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var depth = (CoordinateValue)ncWord.ValueData;
            return ncWord with { ValueData = RewriteCenterDrillDepthValueData(centerDrillDepth, depth) };
        }

        [Logging]
        private static INCWord RewriteSpin(MaterialType material, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            var spin = (NumericalValue)ncWord.ValueData;
            return ncWord with { ValueData = RewriteSpinValueData(material, spin) };
        }

        [Logging]
        private static IValueData RewriteFeedValueData(MaterialType material, NumericalValue valueData)
        {
            string feedValue = material switch
            {
                MaterialType.Aluminum => "150",
                MaterialType.Iron => "100",
                _ => throw new AggregateException(nameof(material)),
            };
            return valueData with { Value = feedValue };
        }

        [Logging]
        private static IValueData RewriteCenterDrillDepthValueData(decimal centerDrillDepth, CoordinateValue valueData)
        {
            return valueData with { Value = Convert.ToString(centerDrillDepth) };
        }

        [Logging]
        private static IValueData RewriteSpinValueData(MaterialType material, NumericalValue valueData)
        {
            string spinValue = material switch
            {
                MaterialType.Aluminum => "2000",
                MaterialType.Iron => "1500",
                _ => throw new AggregateException(nameof(material)),
            };
            return valueData with { Value = spinValue };
        }
    }
}
