using System.Data;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter.Process;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    /// <summary>
    /// クリスタルリーマのパラメータを書き換える
    /// </summary>
    public class CrystalReamingParameterRewriter : IMainProgramParameterRewriter
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

            // クリスタルリーマのパラメータを受け取る
            if (!prameterRecord.Parameters.TryGetValue(ParameterType.CrystalReamerParameter, out var reamingParameters)
            || reamingParameters == null)
                throw new NCProgramConcatenationServiceException(
                    $"パラメータが受け取れません ParameterType: {nameof(ParameterType.CrystalReamerParameter)}");

            // ドリルのパラメータを受け取る
            if (!prameterRecord.Parameters.TryGetValue(ParameterType.DrillParameter, out var drillingParameters)
            || drillingParameters == null)
                throw new NCProgramConcatenationServiceException(
                    $"パラメータが受け取れません ParameterType: {nameof(ParameterType.DrillParameter)}");

            // メインプログラムを工程ごとに取り出す
            List<NCProgramCode> ncPrograms = new();
            foreach (var (key, value) in rewritableCodes)
            {
                IMainProgramPrameter reamingParameter;
                try
                {
                    reamingParameter = reamingParameters.First(x => x.TargetToolDiameter == targetToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"リーマ径 {targetToolDiameter}のリストがありません", ex);
                }

                switch (key)
                {
                    case MainProgramType.CenterDrilling:
                        ncPrograms.Add(CenterDrillingProgramRewriter.Rewrite(value, material, reamingParameter));
                        break;
                    case MainProgramType.Drilling:
                        ncPrograms.AddRange(RewriteCNCProgramForDrilling(value, material, thickness, drillingParameters, reamingParameter));
                        break;
                    case MainProgramType.Chamfering:
                        if (reamingParameter.ChamferingDepth != null)
                            ncPrograms.Add(ChamferingProgramRewriter.Rewrite(value, material, reamingParameter));
                        break;
                    case MainProgramType.Reaming:
                        ncPrograms.Add(ReamingProgramRewriter.Rewrite(value, material, ReamerType.CrystalReamerParameter, thickness, reamingParameter));
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
        /// 2回下穴のパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="thickness"></param>
        /// <param name="drillingParameters"></param>
        /// <param name="reamingParameter"></param>
        /// <returns></returns>
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        private static List<NCProgramCode> RewriteCNCProgramForDrilling(
            NCProgramCode rewritableCode,
            MaterialType material,
            decimal thickness,
            IEnumerable<IMainProgramPrameter> drillingParameters,
            IMainProgramPrameter reamingParameter)
        {
            List<NCProgramCode> ncPrograms = new();
            ReamingProgramPrameter reaming = (ReamingProgramPrameter)reamingParameter;
            // 下穴 1回目
            DrillingProgramPrameter? fastDrillingParameter = drillingParameters
                .Cast<DrillingProgramPrameter>()
                .Where(x => x.TargetToolDiameter <= reaming.PreparedHoleDiameter)
                .MaxBy(x => x.TargetToolDiameter);
            if (fastDrillingParameter == null)
                throw new NCProgramConcatenationServiceException(
                    $"穴径に該当するリストがありません 穴径: {reaming.PreparedHoleDiameter}");
            ncPrograms.Add(DrillingProgramRewriter.Rewrite(rewritableCode, material, reaming.PreparedHoleDiameter, thickness, fastDrillingParameter));

            // 下穴 2回目
            DrillingProgramPrameter? secondDrillingParameter = drillingParameters
                .Cast<DrillingProgramPrameter>()
                .Where(x => x.TargetToolDiameter <= reaming.SecondPreparedHoleDiameter)
                .MaxBy(x => x.TargetToolDiameter);
            if (secondDrillingParameter == null)
                throw new NCProgramConcatenationServiceException(
                    $"穴径に該当するリストがありません 穴径: {reaming.SecondPreparedHoleDiameter}");
            ncPrograms.Add(DrillingProgramRewriter.Rewrite(rewritableCode, material, reaming.SecondPreparedHoleDiameter, thickness, secondDrillingParameter));

            return ncPrograms;
        }
    }
}
