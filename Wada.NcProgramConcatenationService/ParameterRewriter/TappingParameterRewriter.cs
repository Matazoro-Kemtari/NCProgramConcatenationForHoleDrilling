using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter
{
    public class TappingParameterRewriter : IMainProgramParameterRewriter
    {
        [Logging]
        public virtual IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord)
        {
            if (rewriteByToolRecord.Material == MaterialType.Undefined)
                throw new ArgumentException("素材が未定義です");

            // タップのパラメータを受け取る
            var tappingParameters = rewriteByToolRecord.TapParameters;

            // ドリルのパラメータを受け取る
            var drillingParameters = rewriteByToolRecord.DrillingParameters;

            // メインプログラムを工程ごとに取り出す
            List<NcProgramCode> ncPrograms = new();
            foreach (var rewritableCode in rewriteByToolRecord.RewritableCodes)
            {
                TappingProgramParameter tappingParameter;
                try
                {
                    tappingParameter = tappingParameters
                        .First(x => x.DirectedOperationToolDiameter == rewriteByToolRecord.DirectedOperationToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new DomainException(
                        $"タップ径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません", ex);
                }

                switch (rewritableCode.MainProgramClassification)
                {
                    case NcProgramType.CenterDrilling:
                        ncPrograms.Add(CenterDrillingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            tappingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NcProgramType.Drilling:
                        ncPrograms.Add(RewriteCncProgramForDrilling(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            rewriteByToolRecord.Thickness,
                            drillingParameters,
                            tappingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NcProgramType.Chamfering:
                        ncPrograms.Add(ChamferingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            tappingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NcProgramType.Tapping:
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
        /// <exception cref="DomainException"></exception>
        private static NcProgramCode RewriteCncProgramForDrilling(NcProgramCode rewritableCode, MaterialType material, decimal thickness, IEnumerable<DrillingProgramParameter> drillingParameters, TappingProgramParameter tappingParameter, string subProgramNumber)
        {
            var drillingParameter = drillingParameters
                .Where(x => x.DirectedOperationToolDiameter <= tappingParameter.PreparedHoleDiameter)
                .MaxBy(x => x.DirectedOperationToolDiameter);
            if (drillingParameter == null)
                throw new DomainException(
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
