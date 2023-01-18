using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    public interface IMainProgramParameterRewriter
    {
        /// <summary>
        /// メインプログラムのパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCodes">元NCプログラム</param>
        /// <param name="material">素材</param>
        /// <param name="thickness">板厚</param>
        /// <param name="targetToolDiameter">目標工具径 :サブプログラムで指定した工具径</param>
        /// <param name="prameters">パラメータ</param>
        /// <returns></returns>
        IEnumerable<NCProgramCode> RewriteByTool(
            Dictionary<MainProgramType, NCProgramCode> rewritableCodes,
            MaterialType material,
            decimal thickness,
            decimal targetToolDiameter,
            MainProgramParametersRecord prameters);
    }

    public record class MainProgramParametersRecord(Dictionary<ParameterType, IEnumerable<IMainProgramPrameter>> Parameters);

    public class TestMainProgramParametersRecordFactory
    {
        public static MainProgramParametersRecord Create(
            Dictionary<ParameterType, IEnumerable<IMainProgramPrameter>>? parameters = default)
        {
            decimal reamerDiameter = 15m;
            decimal fastDrill = 10m;
            decimal secondDrill = 11.8m;
            decimal centerDrillDepth = -1.5m;
            decimal? chamferingDepth = -6.1m;
            parameters ??= new()
            {
                {
                    ParameterType.CrystalReamerParameter,
                    new List<IMainProgramPrameter>
                    {
                        new ReamingProgramPrameter(reamerDiameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth)
                    }
                },
                {
                    ParameterType.DrillParameter,
                    new List<IMainProgramPrameter>
                    {
                        new DrillingProgramPrameter(DiameterKey: fastDrill.ToString(),
                                                    CenterDrillDepth: -1.5m,
                                                    CutDepth: 3m,
                                                    SpinForAluminum: 960m,
                                                    FeedForAluminum: 130m,
                                                    SpinForIron: 640m,
                                                    FeedForIron: 90m),
                        new DrillingProgramPrameter(DiameterKey: secondDrill.ToString(),
                                                    CenterDrillDepth: -1.5m,
                                                    CutDepth: 3.5m,
                                                    SpinForAluminum: 84m,
                                                    FeedForAluminum: 110m,
                                                    SpinForIron: 560m,
                                                    FeedForIron: 80m)
                    }
                }
            };
            return new(parameters);
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
