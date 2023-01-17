using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService
{
    public interface IMainProgramParameterRewriter
    {
        /// <summary>
        /// メインプログラムのパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCodes">元NCプログラム</param>
        /// <param name="materialType">素材</param>
        /// <param name="targetToolDiameter">目標工具径 :サブプログラムで指定した工具径</param>
        /// <param name="prameters">パラメータ</param>
        /// <returns></returns>
        IEnumerable<NCProgramCode> RewriteProgramParameter(
            Dictionary<MainProgramType, NCProgramCode> rewritableCodes,
            MaterialType materialType,
            double targetToolDiameter,
            MainProgramParametersRecord prameters);
    }

    public record class MainProgramParametersRecord(Dictionary<ParameterType, IEnumerable<IMainProgramPrameter>> Parameters);

    /// <summary>
    /// クリスタルリーマのパラメータを書き換える
    /// </summary>
    public class CrystalReamingParameterRewriter : IMainProgramParameterRewriter
    {
        [Logging]
        public IEnumerable<NCProgramCode> RewriteProgramParameter(
            Dictionary<MainProgramType, NCProgramCode> rewritableCodes,
            MaterialType material,
            double targetToolDiameter,
            MainProgramParametersRecord prameterRecord)
        {
            if (material == MaterialType.Undefined)
                throw new AggregateException(nameof(material));

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
            return rewritableCodes.Select(dic =>
            {
                IMainProgramPrameter reamingParameter;
                try
                {
                    reamingParameter = reamingParameters.First(x => x.TargetToolDiameter == targetToolDiameter);
                }
                catch (InvalidOperationException ex)
                {
                    throw new NCProgramConcatenationServiceException(
                        $"リーマ径 {targetToolDiameter}Φのリストがありません");
                }

                return dic.Key switch
                {
                    MainProgramType.CenterDrilling => MainProgramRewriter.RewriteProgramParameterForCenterDrilling(
                        dic.Value,
                        material,
                        reamingParameter,
                        drillingParameters),
                    MainProgramType.Drilling => MainProgramRewriter.RewriteProgramParameterForDrilling(),
                    MainProgramType.Chamfering => MainProgramRewriter.RewriteProgramParameterForChamfering(),
                    MainProgramType.Reaming => MainProgramRewriter.RewriteProgramParameterForReaming(),
                    MainProgramType.Tapping => MainProgramRewriter.RewriteProgramParameterForTapping(),
                    _ => throw new NotImplementedException()
                };
            });
        }
    }

    internal class MainProgramRewriter
    {
        [Logging]
        internal static NCProgramCode RewriteProgramParameterForTapping()
        {
            throw new NotImplementedException();
        }

        [Logging]
        internal static NCProgramCode RewriteProgramParameterForReaming()
        {
            throw new NotImplementedException();
        }

        [Logging]
        internal static NCProgramCode RewriteProgramParameterForChamfering()
        {
            throw new NotImplementedException();
        }

        [Logging]
        internal static NCProgramCode RewriteProgramParameterForDrilling()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// センタードリルのパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="material"></param>
        /// <param name="rewritingParameter">対象のパラメータ</param>
        /// <param name="drillingParameters">ドリルのパラメータリスト</param>
        /// <returns></returns>
        [Logging]
        internal static NCProgramCode RewriteProgramParameterForCenterDrilling(
            NCProgramCode rewritableCode,
            MaterialType material,
            IMainProgramPrameter rewritingParameter,
            IEnumerable<IMainProgramPrameter>? drillingParameters)
        {
            // NCプログラムを走査して書き換え対象を探す
            var rewritedNCBlocks = rewritableCode.NCBlocks
                .Select(x =>
                {
                    return x == null
                        ? null
                        : new NCBlock(
                        x.NCWords.Select(y =>
                        {
                            if (y.GetType() != typeof(NCWord))
                                return y;

                            NCWord ncWord = (NCWord)y;
                            if (ncWord.Address.Value == 'S')
                            {
                                return ncWord with
                                {
                                    ValueData = ncWord.ValueData.Indefinite ?
                                    RewriteSpinParameter(material, ncWord.ValueData)
                                    : ncWord.ValueData
                                };
                            }
                            else if (ncWord.Address.Value == 'Z')
                            {
                                return ncWord with
                                {
                                    ValueData = ncWord.ValueData.Indefinite ?
                                    RewriteDepthParameter(material, ncWord.ValueData, drillingParameters)
                                    : ncWord.ValueData
                                };
                            }
                            else
                                return y;
                        }),
                        x.HasBlockSkip);
                });

            return rewritableCode with
            {
                NCBlocks = rewritedNCBlocks
            };
        }

        private static IValueData RewriteDepthParameter(MaterialType material, IValueData valueData, object drillingProgramPrameter)
        {
            throw new NotImplementedException();
        }

        [Logging]
        private static IValueData RewriteSpinParameter(MaterialType material, IValueData valueData)
        {
            string spinValue;
            switch (material)
            {
                case MaterialType.Aluminum:
                    spinValue = "2000";
                    break;
                case MaterialType.Iron:
                    spinValue = "1500";
                    break;
                default:
                    throw new AggregateException(nameof(material));
            }
            return  (NumericalValue)valueData with { Value = spinValue };
        }
    }

    public enum MainProgramType
    {
        /// <summary>
        /// センタードリル
        /// </summary>
        CenterDrilling,
        /// <summary>
        /// ドリル
        /// </summary>
        Drilling,
        /// <summary>
        /// 面取り
        /// </summary>
        Chamfering,
        /// <summary>
        /// リーマ
        /// </summary>
        Reaming,
        /// <summary>
        /// タップ
        /// </summary>
        Tapping,
    }

    public enum MaterialType
    {
        Undefined,
        Aluminum,
        Iron,
    }

    public enum ParameterType
    {
        DrillParameter,
        CrystalReamerParameter,
        SkillReamerParameter,
        TapParameter,
    }
}
