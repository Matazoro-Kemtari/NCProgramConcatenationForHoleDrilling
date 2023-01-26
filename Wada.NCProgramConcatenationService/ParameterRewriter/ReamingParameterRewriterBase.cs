using System.Data;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter.Process;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
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
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        [Logging]
        private static List<NCProgramCode> RewriteCNCProgramForDrilling(
            NCProgramCode rewritableCode,
            MaterialType material,
            decimal thickness,
            IEnumerable<DrillingProgramPrameter> drillingParameters,
            ReamingProgramPrameter reamingParameter)
        {
            List<NCProgramCode> ncPrograms = new();
            // 下穴 1回目
            var fastDrillingParameter = drillingParameters
                .Where(x => x.TargetToolDiameter <= reamingParameter.PreparedHoleDiameter)
                .MaxBy(x => x.TargetToolDiameter);
            if (fastDrillingParameter == null)
                throw new NCProgramConcatenationServiceException(
                    $"穴径に該当するリストがありません 穴径: {reamingParameter.PreparedHoleDiameter}");
            ncPrograms.Add(DrillingProgramRewriter.Rewrite(rewritableCode, material, reamingParameter.PreparedHoleDiameter, thickness, fastDrillingParameter));

            // 下穴 2回目
            var secondDrillingParameter = drillingParameters
                .Where(x => x.TargetToolDiameter <= reamingParameter.SecondPreparedHoleDiameter)
                .MaxBy(x => x.TargetToolDiameter);
            if (secondDrillingParameter == null)
                throw new NCProgramConcatenationServiceException(
                    $"穴径に該当するリストがありません 穴径: {reamingParameter.SecondPreparedHoleDiameter}");
            ncPrograms.Add(DrillingProgramRewriter.Rewrite(rewritableCode, material, reamingParameter.SecondPreparedHoleDiameter, thickness, secondDrillingParameter));

            return ncPrograms;
        }

        [Logging]
        public virtual IEnumerable<NCProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord)
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
            List<NCProgramCode> rewritedNCPrograms = new();
            foreach (var rewritableCode in rewriteByToolRecord.RewritableCodes)
            {
                ReamingProgramPrameter reamingParameter;
                try
                {
                    reamingParameter = reamingParameters.First(x => x.TargetToolDiameter == rewriteByToolRecord.TargetToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"リーマ径 {rewriteByToolRecord.TargetToolDiameter}のリストがありません", ex);
                }

                switch (rewritableCode.MainProgramClassification)
                {
                    case NCProgramType.CenterDrilling:
                        rewritedNCPrograms.Add(CenterDrillingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, reamingParameter));
                        break;
                    case NCProgramType.Drilling:
                        rewritedNCPrograms.AddRange(RewriteCNCProgramForDrilling(rewritableCode, rewriteByToolRecord.Material, rewriteByToolRecord.Thickness, drillingParameters, reamingParameter));
                        break;
                    case NCProgramType.Chamfering:
                        if (reamingParameter.ChamferingDepth != null)
                            rewritedNCPrograms.Add(ChamferingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, reamingParameter));
                        break;
                    case NCProgramType.Reaming:
                        rewritedNCPrograms.Add(ReamingProgramRewriter.Rewrite(rewritableCode, rewriteByToolRecord.Material, _reamerType, rewriteByToolRecord.Thickness, reamingParameter));
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