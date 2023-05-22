using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
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
            var drillingParameters = rewriteByToolRecord.DrillingParameters;

            // メインプログラムを工程ごとに取り出す
            List<NcProgramCode> rewrittenNcPrograms = new();
            foreach (var rewritableCode in rewriteByToolRecord.RewritableCodes)
            {
                var maxDiameter = drillingParameters.MaxBy(x => x.DirectedOperationToolDiameter)
                    ?.DirectedOperationToolDiameter;
                if (maxDiameter == null
                    || maxDiameter + 0.5m < rewriteByToolRecord.DirectedOperationToolDiameter)
                    throw new DomainException(
                        $"ドリル径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません\n" +
                        $"リストの最大ドリル径({maxDiameter})を超えています");

                DrillingProgramParameter drillingParameter = drillingParameters
                    .Where(x => x.DirectedOperationToolDiameter <= rewriteByToolRecord.DirectedOperationToolDiameter)
                    .MaxBy(x => x.DirectedOperationToolDiameter)
                    ?? throw new DomainException(
                        $"ドリル径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません");

                switch (rewritableCode.MainProgramClassification)
                {
                    case NcProgramType.CenterDrilling:
                        rewrittenNcPrograms.Add(CenterDrillingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            drillingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NcProgramType.Drilling:
                        rewrittenNcPrograms.Add(DrillingProgramRewriter.Rewrite(
                                rewritableCode,
                                rewriteByToolRecord.Material,
                                rewriteByToolRecord.Thickness,
                                drillingParameter,
                                rewriteByToolRecord.SubProgramNumber,
                                rewriteByToolRecord.DirectedOperationToolDiameter));
                        break;
                    case NcProgramType.Chamfering:
                        rewrittenNcPrograms.Add(ReplaceLastM1ToM30(
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
            return rewrittenNcPrograms;
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
            var rewrittenNcBlocks = ncProgramCode.NcBlocks
                .Reverse()
                .Select(x =>
                {
                    if (x == null)
                        return null;

                    var rewitedNcWords = x.NcWords
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

                    return x with { NcWords = rewitedNcWords };
                })
                .Reverse()
                // ここで遅延実行を許すとプレビューで変更が反映されない
                .ToList();

            return ncProgramCode with
            {
                NcBlocks = rewrittenNcBlocks
            };
        }
    }
}
