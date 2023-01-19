using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter.Process;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    public class TappingParameterRewriter : IMainProgramParameterRewriter
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

            // タップのパラメータを受け取る
            if (!prameterRecord.Parameters.TryGetValue(ParameterType.TapParameter, out var tappingParameters)
            || tappingParameters == null)
                throw new NCProgramConcatenationServiceException(
                    $"パラメータが受け取れません ParameterType: {nameof(ParameterType.TapParameter)}");

            // ドリルのパラメータを受け取る
            if (!prameterRecord.Parameters.TryGetValue(ParameterType.DrillParameter, out var drillingParameters)
            || drillingParameters == null)
                throw new NCProgramConcatenationServiceException(
                    $"パラメータが受け取れません ParameterType: {nameof(ParameterType.DrillParameter)}");

            // メインプログラムを工程ごとに取り出す
            List<NCProgramCode> ncPrograms = new();
            foreach (var (key, value) in rewritableCodes)
            {
                IMainProgramPrameter tappingParameter;
                try
                {
                    tappingParameter = tappingParameters
                        .First(x => x.TargetToolDiameter == targetToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"リーマ径 {targetToolDiameter}のリストがありません", ex);
                }

                switch (key)
                {
                    case MainProgramType.CenterDrilling:
                        ncPrograms.Add(CenterDrillingProgramRewriter.Rewrite(value, material, tappingParameter));
                        break;
                    case MainProgramType.Drilling:
                        ncPrograms.Add(RewriteCNCProgramForDrilling(value, material, thickness, drillingParameters, tappingParameter));
                        break;
                    case MainProgramType.Chamfering:
                        if (tappingParameter.ChamferingDepth != null)
                            ncPrograms.Add(ChamferingProgramRewriter.Rewrite(value, material, tappingParameter));
                        break;
                    case MainProgramType.Tapping:
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return ncPrograms;
        }

        /// <summary>
        /// 下穴のパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="thickness"></param>
        /// <param name="drillingParameters"></param>
        /// <param name="tappingParameter"></param>
        /// <returns></returns>
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        private static NCProgramCode RewriteCNCProgramForDrilling(NCProgramCode rewritableCode, MaterialType material, decimal thickness, IEnumerable<IMainProgramPrameter> drillingParameters, IMainProgramPrameter tappingParameter)
        {
            TappingProgramPrameter tapping = (TappingProgramPrameter)tappingParameter;
            DrillingProgramPrameter? drillingParameter = drillingParameters
                .Cast<DrillingProgramPrameter>()
                .Where(x => x.TargetToolDiameter <= tapping.PreparedHoleDiameter)
                .MaxBy(x => x.TargetToolDiameter);
            if (drillingParameter == null)
                throw new NCProgramConcatenationServiceException(
                    $"穴径に該当するリストがありません 穴径: {tapping.PreparedHoleDiameter}");
            var hoge = DrillingProgramRewriter.Rewrite(rewritableCode, material, tapping.PreparedHoleDiameter, thickness, drillingParameter);
            return hoge;
        }
    }
}
