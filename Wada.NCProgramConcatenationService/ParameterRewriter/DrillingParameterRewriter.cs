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
                DrillingProgramPrameter drillingParameter;
                try
                {
                    drillingParameter = drillingParameters
                        .First(x => x.TargetToolDiameter == rewriteByToolRecord.TargetToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"ドリル径 {rewriteByToolRecord.TargetToolDiameter}のリストがありません", ex);
                }

                switch (rewritableCode.MainProgramClassification)
                {
                    case NCProgramType.CenterDrilling:
                        rewritedNCPrograms.Add(CenterDrillingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, drillingParameter, rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NCProgramType.Drilling:
                        rewritedNCPrograms.Add(DrillingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, rewriteByToolRecord.Thickness, drillingParameter, rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NCProgramType.Chamfering:
                        rewritedNCPrograms.Add(ChamferingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, drillingParameter, rewriteByToolRecord.SubProgramNumber));
                        break;
                    default:
                        // 何もしない
                        break;
                }
            }
            return rewritedNCPrograms;
        }
    }
}
