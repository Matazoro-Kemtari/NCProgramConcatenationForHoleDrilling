using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter.Process;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    public class DrillingParameterRewriter : IMainProgramParameterRewriter
    {
        [Logging]
        public virtual IEnumerable<NCProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord)
        {
            if (rewriteByToolRecord.Material == MaterialType.Undefined)
                throw new ArgumentException("素材が未定義です");

            // ドリルのパラメータを受け取る
            var drillingParameters = rewriteByToolRecord.DrillingPrameters;

            // メインプログラムを工程ごとに取り出す
            List<NCProgramCode> rewritedNCPrograms = new();
            foreach (var rewritableCode in rewriteByToolRecord.RewritableCodes)
            {
                var maxDiameter = drillingParameters.MaxBy(x => x.DirectedOperationToolDiameter)
                    ?.DirectedOperationToolDiameter;
                if (maxDiameter == null
                    || maxDiameter + 0.5m < rewriteByToolRecord.DirectedOperationToolDiameter)
                    throw new NCProgramConcatenationServiceException(
                        $"ドリル径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません\n" +
                        $"リストの最大ドリル径({maxDiameter})を超えています");

                DrillingProgramPrameter drillingParameter = drillingParameters
                    .Where(x => x.DirectedOperationToolDiameter <= rewriteByToolRecord.DirectedOperationToolDiameter)
                    .MaxBy(x => x.DirectedOperationToolDiameter)
                    ?? throw new NCProgramConcatenationServiceException(
                        $"ドリル径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません");

                switch (rewritableCode.MainProgramClassification)
                {
                    case NCProgramType.CenterDrilling:
                        rewritedNCPrograms.Add(CenterDrillingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, drillingParameter, rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NCProgramType.Drilling:
                        rewritedNCPrograms.Add(DrillingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, rewriteByToolRecord.Thickness, drillingParameter, rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NCProgramType.Chamfering:
                        rewritedNCPrograms.Add(
                            ReplaceLastM1ToM30(
                                ChamferingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, drillingParameter, rewriteByToolRecord.SubProgramNumber)));
                        break;
                    default:
                        // 何もしない
                        break;
                }
            }
            return rewritedNCPrograms;
        }

        /// <summary>
        /// ドリリングの作業指示の時だけ面取りの最後をM1からM30に書き換える
        /// </summary>
        /// <param name="ncProgramCode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [Logging]
        public static NCProgramCode ReplaceLastM1ToM30(NCProgramCode ncProgramCode)
        {
            if (ncProgramCode.MainProgramClassification != NCProgramType.Chamfering)
                throw new ArgumentException("引数に面取り以外のプログラムコードが指定されました");

            bool hasFinded1stWord = false;
            var rewritedNCBlocks = ncProgramCode.NCBlocks
                .Reverse()
                .Select(x =>
                {
                    if (x == null)
                        return null;

                    var rewitedNCWords = x.NCWords
                        .Select(y =>
                        {
                            INCWord resuld;
                            if (hasFinded1stWord == false
                            && y.GetType() == typeof(NCWord))
                            {
                                hasFinded1stWord = true;

                                NCWord ncWord = (NCWord)y;
                                if (ncWord.Address.Value == 'M'
                                && ncWord.ValueData.Number == 1)
                                    resuld = ncWord with
                                    {
                                        ValueData = new NumericalValue("30")
                                    };
                                else
                                    resuld = y;
                            }
                            else
                                resuld = y;

                            return resuld;
                        })
                        // ここで遅延実行を許すとUnitTestで失敗する
                        .ToList();

                    return x with { NCWords = rewitedNCWords };
                })
                .Reverse()
                // ここで遅延実行を許すとプレビューで変更が反映されない
                .ToList();

            return ncProgramCode with
            {
                NCBlocks = rewritedNCBlocks
            };
        }
    }
}
