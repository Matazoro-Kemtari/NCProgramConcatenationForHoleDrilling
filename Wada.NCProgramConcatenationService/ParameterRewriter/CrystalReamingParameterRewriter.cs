using System.Collections.Generic;
using System.Data;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

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
                throw new ArgumentException("素材が未確定です");

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
            List<NCProgramCode> ncPrograms = new List<NCProgramCode>();
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

                if (key == MainProgramType.CenterDrilling)
                    ncPrograms.Add(CenterDrillingProgramRewriter.Rewrite(
                            value,
                            material,
                            reamingParameter));
                else if (key == MainProgramType.Drilling)
                {
                    ReamingProgramPrameter reaming = (ReamingProgramPrameter)reamingParameter;
                    // 下穴 1回目
                    DrillingProgramPrameter? fastDrillingParameter = drillingParameters
                        .Cast<DrillingProgramPrameter>()
                        .Where(x => x.TargetToolDiameter <= reaming.PreparedHoleDiameter)
                        .MaxBy(x => x.TargetToolDiameter);
                    if (fastDrillingParameter == null)
                        throw new NCProgramConcatenationServiceException(
                            $"穴径に該当するリストがありません 穴径: {reaming.PreparedHoleDiameter}");
                    ncPrograms.Add(DrillingProgramRewriter.Rewrite(value, material, reaming.PreparedHoleDiameter, thickness, fastDrillingParameter));

                    // 下穴 2回目
                    DrillingProgramPrameter? secondDrillingParameter = drillingParameters
                        .Cast<DrillingProgramPrameter>()
                        .Where(x => x.TargetToolDiameter <= reaming.SecondPreparedHoleDiameter)
                        .MaxBy(x => x.TargetToolDiameter);
                    if (secondDrillingParameter == null)
                        throw new NCProgramConcatenationServiceException(
                            $"穴径に該当するリストがありません 穴径: {reaming.SecondPreparedHoleDiameter}");
                    ncPrograms.Add(DrillingProgramRewriter.Rewrite(value, material, reaming.SecondPreparedHoleDiameter, thickness, secondDrillingParameter));
                }
                else
                    throw new NotImplementedException();
            }
            return ncPrograms;
        }
    }
}
