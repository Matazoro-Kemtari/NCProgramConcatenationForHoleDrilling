using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Process
{
    internal class ChamferingProgramRewriter
    {
        /// <summary>
        /// 面取りのメインプログラムを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="rewritingParameter">対象のパラメータ</param>
        /// <returns></returns>
        [Logging]
        internal static NCProgramCode Rewrite(
            NCProgramCode rewritableCode,
            MaterialType material,
            IMainProgramPrameter rewritingParameter,
            string subProgramNumber)
        {
            // NCプログラムを走査して書き換え対象を探す
            var rewritedNCBlocks = rewritableCode.NCBlocks
                .Select(x =>
                {
                    if (x == null)
                        return null;

                    if (rewritingParameter.ChamferingDepth == null)
                        throw new InvalidOperationException("面取りが無いのに呼び出された");

                    var rewritedNCWords = x.NCWords
                        .Select(y =>
                        {
                            if (y.GetType() != typeof(NCWord))
                                return y;

                            NCWord ncWord = (NCWord)y;
                            if (!ncWord.ValueData.Indefinite)
                                return y;

                            return ncWord.Address.Value switch
                            {
                                'S' => RewriteSpin(material, ncWord),
                                'Z' => RewriteChamferingDepth(rewritingParameter.ChamferingDepth.Value, ncWord),
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
        private static INCWord RewriteChamferingDepth(decimal chamferDepth, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;

            return ncWord with { ValueData = new CoordinateValue(Convert.ToString(chamferDepth)) };
        }

        [Logging]
        private static INCWord RewriteSpin(MaterialType material, NCWord ncWord)
        {
            if (!ncWord.ValueData.Indefinite)
                return ncWord;
            
            string spinValue = material switch
            {
                MaterialType.Aluminum => "1400",
                MaterialType.Iron => "1100",
                _ => throw new AggregateException(nameof(material)),
            };

            return ncWord with { ValueData = new NumericalValue(spinValue) };
        }
    }
}
