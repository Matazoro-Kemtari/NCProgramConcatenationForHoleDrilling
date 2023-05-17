using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter
{
    public class DrillingParameterRewriter : IMainProgramParameterRewriter
    {
        [Logging]
        public virtual IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord)
        {
            if (rewriteByToolRecord.Material == MaterialType.Undefined)
                throw new ArgumentException("素材が未定義です");

            // ドリルのパラメータを受け取る
            var drillingParameters = rewriteByToolRecord.DrillingPrameters;

            // メインプログラムを工程ごとに取り出す
            List<NcProgramCode> rewritedNCPrograms = new();
            foreach (var rewritableCode in rewriteByToolRecord.RewritableCodes)
            {
                var maxDiameter = drillingParameters.MaxBy(x => x.DirectedOperationToolDiameter)
                    ?.DirectedOperationToolDiameter;
                if (maxDiameter == null
                    || maxDiameter + 0.5m < rewriteByToolRecord.DirectedOperationToolDiameter)
                    throw new DomainException(
                        $"ドリル径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません\n" +
                        $"リストの最大ドリル径({maxDiameter})を超えています");

                DrillingProgramPrameter drillingParameter = drillingParameters
                    .Where(x => x.DirectedOperationToolDiameter <= rewriteByToolRecord.DirectedOperationToolDiameter)
                    .MaxBy(x => x.DirectedOperationToolDiameter)
                    ?? throw new DomainException(
                        $"ドリル径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません");

                switch (rewritableCode.MainProgramClassification)
                {
                    case NcProgramType.CenterDrilling:
                        rewritedNCPrograms.Add(CenterDrillingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            drillingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NcProgramType.Drilling:
                        rewritedNCPrograms.Add(DrillingProgramRewriter.Rewrite(
                                rewritableCode,
                                rewriteByToolRecord.Material,
                                rewriteByToolRecord.Thickness,
                                drillingParameter,
                                rewriteByToolRecord.SubProgramNumber,
                                rewriteByToolRecord.DirectedOperationToolDiameter));
                        break;
                    case NcProgramType.Chamfering:
                        rewritedNCPrograms.Add(ReplaceLastM1ToM30(
                                ChamferingProgramRewriter.Rewrite(
                                    rewritableCode,
                                    rewriteByToolRecord.Material,
                                    drillingParameter,
                                    rewriteByToolRecord.SubProgramNumber)));
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
        public static NcProgramCode ReplaceLastM1ToM30(NcProgramCode ncProgramCode)
        {
            if (ncProgramCode.MainProgramClassification != NcProgramType.Chamfering)
                throw new ArgumentException("引数に面取り以外のプログラムコードが指定されました");

            bool hasFinded1stWord = false;
            var rewritedNCBlocks = ncProgramCode.NcBlocks
                .Reverse()
                .Select(x =>
                {
                    if (x == null)
                        return null;

                    var rewitedNCWords = x.NCWords
                        .Select(y =>
                        {
                            INcWord resuld;
                            if (hasFinded1stWord == false
                            && y.GetType() == typeof(NcWord))
                            {
                                hasFinded1stWord = true;

                                NcWord ncWord = (NcWord)y;
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
                NcBlocks = rewritedNCBlocks
            };
        }
    }
}
