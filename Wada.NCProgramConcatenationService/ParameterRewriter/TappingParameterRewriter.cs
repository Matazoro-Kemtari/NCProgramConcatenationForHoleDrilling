using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter.Process;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    public class TappingParameterRewriter : IMainProgramParameterRewriter
    {
        [Logging]
        public virtual IEnumerable<NCProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord)
        {
            if (rewriteByToolRecord.Material == MaterialType.Undefined)
                throw new ArgumentException("素材が未定義です");

            // タップのパラメータを受け取る
            var tappingParameters = rewriteByToolRecord.TapParameters;

            // ドリルのパラメータを受け取る
            var drillingParameters = rewriteByToolRecord.DrillingPrameters;

            // メインプログラムを工程ごとに取り出す
            List<NCProgramCode> ncPrograms = new();
            foreach (var rewritableCode in rewriteByToolRecord.RewritableCodes)
            {
                TappingProgramPrameter tappingParameter;
                try
                {
                    tappingParameter = tappingParameters
                        .First(x => x.DirectedOperationToolDiameter == rewriteByToolRecord.DirectedOperationToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"タップ径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません", ex);
                }

                switch (rewritableCode.MainProgramClassification)
                {
                    case NCProgramType.CenterDrilling:
                        ncPrograms.Add(CenterDrillingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            tappingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NCProgramType.Drilling:
                        ncPrograms.Add(RewriteCNCProgramForDrilling(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            rewriteByToolRecord.Thickness,
                            drillingParameters,
                            tappingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NCProgramType.Chamfering:
                        ncPrograms.Add(ChamferingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            tappingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NCProgramType.Tapping:
                        ncPrograms.Add(TappingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            rewriteByToolRecord.Thickness,
                            tappingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    default:
                        // 何もしない
                        break;
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
        private static NCProgramCode RewriteCNCProgramForDrilling(NCProgramCode rewritableCode, MaterialType material, decimal thickness, IEnumerable<DrillingProgramPrameter> drillingParameters, TappingProgramPrameter tappingParameter, string subProgramNumber)
        {
            var drillingParameter = drillingParameters
                .Where(x => x.DirectedOperationToolDiameter <= tappingParameter.PreparedHoleDiameter)
                .MaxBy(x => x.DirectedOperationToolDiameter);
            if (drillingParameter == null)
                throw new NCProgramConcatenationServiceException(
                    $"穴径に該当するリストがありません 穴径: {tappingParameter.PreparedHoleDiameter}");

            return DrillingProgramRewriter.Rewrite(
                rewritableCode,
                material,
                thickness,
                drillingParameter,
                subProgramNumber,
                tappingParameter.PreparedHoleDiameter);
        }
    }
}
