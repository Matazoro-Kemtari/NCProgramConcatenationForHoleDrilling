using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter.Process;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    public class DrillingParameterRewriter : IMainProgramParameterRewriter
    {
        [Logging]
        public IEnumerable<NCProgramCode> RewriteByTool(
            Dictionary<MainProgramType, NCProgramCode> rewritableCodes,
            MaterialType material,
            decimal thickness,
            decimal targetToolDiameter,
            MainProgramParametersRecord prameterRecord)
        {
            if (material == MaterialType.Undefined)
                throw new ArgumentException("素材が未定義です");

            // ドリルのパラメータを受け取る
            if (!prameterRecord.Parameters.TryGetValue(ParameterType.DrillParameter, out var drillingParameters)
            || drillingParameters == null)
                throw new NCProgramConcatenationServiceException(
                    $"パラメータが受け取れません ParameterType: {nameof(ParameterType.DrillParameter)}");

            // メインプログラムを工程ごとに取り出す
            List<NCProgramCode> ncPrograms = new();
            foreach (var (key, value) in rewritableCodes)
            {
                DrillingProgramPrameter drillingParameter;
                try
                {
                    drillingParameter = drillingParameters
                        .Cast<DrillingProgramPrameter>()
                        .First(x => x.TargetToolDiameter == targetToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"ドリル径 {targetToolDiameter}のリストがありません", ex);
                }

                switch (key)
                {
                    case MainProgramType.CenterDrilling:
                        ncPrograms.Add(CenterDrillingProgramRewriter.Rewrite(value, material, drillingParameter));
                        break;
                    case MainProgramType.Drilling:
                        ncPrograms.Add(DrillingProgramRewriter.Rewrite(value, material, targetToolDiameter, thickness, drillingParameter));
                        break;
                    case MainProgramType.Chamfering:
                        ncPrograms.Add(ChamferingProgramRewriter.Rewrite(value, material, drillingParameter));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return ncPrograms;
        }
    }
}
