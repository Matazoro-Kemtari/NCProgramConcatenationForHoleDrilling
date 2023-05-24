using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter
{
    public class TappingSequenceBuilder : IMainProgramSequenceBuilder
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

            // タップの工程
            NcProgramType[] machiningSequences = new[]
            {
                NcProgramType.CenterDrilling,
                NcProgramType.Drilling,
                NcProgramType.Chamfering,
                NcProgramType.Tapping,
            };

            // メインプログラムを工程ごとに取り出す
            var rewrittenNcPrograms = machiningSequences.Select(machiningSequence => machiningSequence switch
            {
                NcProgramType.CenterDrilling => CenterDrillingProgramRewriter.Rewrite(
                    rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == machiningSequence),
                    rewriteByToolRecord.Material,
                    tappingParameter,
                    rewriteByToolRecord.SubProgramNumber),

                NcProgramType.Drilling => RewriteCncProgramForDrilling(
                    rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == machiningSequence),
                    rewriteByToolRecord.Material,
                    rewriteByToolRecord.Thickness,
                    rewriteByToolRecord.DrillingMethod,
                    rewriteByToolRecord.BlindPilotHoleDepth,
                    drillingParameters,
                    tappingParameter,
                    rewriteByToolRecord.SubProgramNumber),

                NcProgramType.Chamfering => ChamferingProgramRewriter.Rewrite(
                    rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == machiningSequence),
                    rewriteByToolRecord.Material,
                    tappingParameter,
                    rewriteByToolRecord.SubProgramNumber),

                NcProgramType.Tapping => TappingProgramRewriter.Rewrite(
                    rewriteByToolRecord.RewritableCodes.Single(x => x.MainProgramClassification == machiningSequence),
                    rewriteByToolRecord.Material,
                    rewriteByToolRecord.DrillingMethod switch
                    {
                        DrillingMethod.ThroughHole => rewriteByToolRecord.Thickness + 5m,
                        DrillingMethod.BlindHole => rewriteByToolRecord.BlindHoleDepth,
                        _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
                    },
                    tappingParameter,
                    rewriteByToolRecord.SubProgramNumber),

                _ => throw new NotImplementedException(),
            });
            
            return rewrittenNcPrograms;
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
        private static NcProgramCode RewriteCncProgramForDrilling(
            NcProgramCode rewritableCode,
            MaterialType material,
            decimal thickness,
            DrillingMethod drillingMethod,
            decimal blindPilotHoleDepth,
            IEnumerable<DrillingProgramParameter> drillingParameters,
            TappingProgramParameter tappingParameter,
            string subProgramNumber)
        {
            var drillingParameter = drillingParameters
                .Where(x => x.DirectedOperationToolDiameter <= tappingParameter.PreparedHoleDiameter)
                .MaxBy(x => x.DirectedOperationToolDiameter)
                ?? throw new DomainException(
                    $"穴径に該当するリストがありません 穴径: {tappingParameter.PreparedHoleDiameter}");

            var drillingDepth = drillingMethod switch
            {
                DrillingMethod.ThroughHole => thickness + drillingParameter.DrillTipLength,
                DrillingMethod.BlindHole => blindPilotHoleDepth,
                _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
            };

            return DrillingProgramRewriter.Rewrite(
                rewritableCode,
                material,
                drillingDepth,
                drillingParameter,
                subProgramNumber,
                tappingParameter.PreparedHoleDiameter);
        }
    }
}
