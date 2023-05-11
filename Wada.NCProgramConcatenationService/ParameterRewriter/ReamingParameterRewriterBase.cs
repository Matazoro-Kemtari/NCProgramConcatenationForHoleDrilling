using System.Data;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter
{
    internal enum ReamerType
    {
        CrystalReamerParameter,
        SkillReamerParameter,
    }

    public abstract class ReamingParameterRewriterBase : IMainProgramParameterRewriter
    {
        private readonly ParameterType _parameterType;
        private readonly ReamerType _reamerType;

        private protected ReamingParameterRewriterBase(ParameterType parameterType, ReamerType reamerType)
        {
            _parameterType = parameterType;
            _reamerType = reamerType;
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
        /// <exception cref="DomainException"></exception>
        [Logging]
        private static List<NcProgramCode> RewriteCNCProgramForDrilling(
            NcProgramCode rewritableCode,
            MaterialType material,
            decimal thickness,
            IEnumerable<DrillingProgramPrameter> drillingParameters,
            ReamingProgramPrameter reamingParameter,
            string subProgramNumber)
        {
            List<NcProgramCode> ncPrograms = new();
            // 下穴 1回目
            var fastDrillingParameter = drillingParameters
                .Where(x => x.DirectedOperationToolDiameter <= reamingParameter.PreparedHoleDiameter)
                .MaxBy(x => x.DirectedOperationToolDiameter);
            if (fastDrillingParameter == null)
                throw new DomainException(
                    $"穴径に該当するリストがありません 穴径: {reamingParameter.PreparedHoleDiameter}");
            ncPrograms.Add(DrillingProgramRewriter.Rewrite(
                rewritableCode,
                material,
                thickness,
                fastDrillingParameter,
                subProgramNumber,
                reamingParameter.PreparedHoleDiameter));

            // 下穴 2回目
            var secondDrillingParameter = drillingParameters
                .Where(x => x.DirectedOperationToolDiameter <= reamingParameter.SecondPreparedHoleDiameter)
                .MaxBy(x => x.DirectedOperationToolDiameter);
            if (secondDrillingParameter == null)
                throw new DomainException(
                    $"穴径に該当するリストがありません 穴径: {reamingParameter.SecondPreparedHoleDiameter}");
            ncPrograms.Add(DrillingProgramRewriter.Rewrite(
                rewritableCode,
                material,
                thickness,
                secondDrillingParameter,
                subProgramNumber,
                reamingParameter.SecondPreparedHoleDiameter));

            return ncPrograms;
        }

        [Logging]
        public virtual IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord)
        {
            if (rewriteByToolRecord.Material == MaterialType.Undefined)
                throw new ArgumentException("素材が未定義です");

            // _parameterTypeリーマのパラメータを受け取る
            IEnumerable<ReamingProgramPrameter> reamingParameters;
            if (_parameterType == ParameterType.CrystalReamerParameter)
                reamingParameters = rewriteByToolRecord.CrystalReamerParameters;
            else
                reamingParameters = rewriteByToolRecord.SkillReamerParameters;

            // ドリルのパラメータを受け取る
            var drillingParameters = rewriteByToolRecord.DrillingPrameters;

            // メインプログラムを工程ごとに取り出す
            List<NcProgramCode> rewritedNCPrograms = new();
            foreach (var rewritableCode in rewriteByToolRecord.RewritableCodes)
            {
                ReamingProgramPrameter reamingParameter;
                try
                {
                    reamingParameter = reamingParameters.First(x => x.DirectedOperationToolDiameter == rewriteByToolRecord.DirectedOperationToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new DomainException(
                        $"リーマ径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません", ex);
                }

                switch (rewritableCode.MainProgramClassification)
                {
                    case NcProgramType.CenterDrilling:
                        rewritedNCPrograms.Add(CenterDrillingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            reamingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NcProgramType.Drilling:
                        rewritedNCPrograms.AddRange(RewriteCNCProgramForDrilling(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            rewriteByToolRecord.Thickness,
                            drillingParameters,
                            reamingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NcProgramType.Chamfering:
                        if (reamingParameter.ChamferingDepth != null)
                            rewritedNCPrograms.Add(ChamferingProgramRewriter.Rewrite(
                                rewritableCode,
                                rewriteByToolRecord.Material,
                                reamingParameter,
                                rewriteByToolRecord.SubProgramNumber));
                        break;
                    case NcProgramType.Reaming:
                        rewritedNCPrograms.Add(ReamingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            _reamerType,
                            rewriteByToolRecord.Thickness,
                            reamingParameter,
                            rewriteByToolRecord.SubProgramNumber));
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