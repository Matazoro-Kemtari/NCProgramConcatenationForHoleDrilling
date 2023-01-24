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
        public IEnumerable<NCProgramCode> RewriteByTool(RewriteByToolRecord RewriteByToolRecord)
        {
            if (RewriteByToolRecord.Material == MaterialType.Undefined)
                throw new ArgumentException("素材が未定義です");

            // ドリルのパラメータを受け取る
            var drillingParameters = RewriteByToolRecord.DrillingPrameters;

            // メインプログラムを工程ごとに取り出す
            List<NCProgramCode> rewritedNCPrograms = new();
            foreach (var rewritableCode in RewriteByToolRecord.RewritableCodes)
            {
                DrillingProgramPrameter drillingParameter;
                try
                {
                    drillingParameter = drillingParameters
                        .First(x => x.TargetToolDiameter == RewriteByToolRecord.TargetToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"ドリル径 {RewriteByToolRecord.TargetToolDiameter}のリストがありません", ex);
                }

                switch (rewritableCode.MainProgramClassification)
                {
                    case NCProgramType.CenterDrilling:
                        rewritedNCPrograms.Add(CenterDrillingProgramRewriter.Rewrite(rewritableCode, RewriteByToolRecord.Material, drillingParameter));
                        break;
                    case NCProgramType.Drilling:
                        rewritedNCPrograms.Add(DrillingProgramRewriter.Rewrite(rewritableCode, RewriteByToolRecord.Material, RewriteByToolRecord.TargetToolDiameter, RewriteByToolRecord.Thickness, drillingParameter));
                        break;
                    case NCProgramType.Chamfering:
                        rewritedNCPrograms.Add(ChamferingProgramRewriter.Rewrite(rewritableCode, RewriteByToolRecord.Material, drillingParameter));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return rewritedNCPrograms;
        }
    }
}
