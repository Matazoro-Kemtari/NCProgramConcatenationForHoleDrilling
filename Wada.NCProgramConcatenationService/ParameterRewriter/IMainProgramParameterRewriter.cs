using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    public interface IMainProgramParameterRewriter
    {
        /// <summary>
        /// メインプログラムのパラメータを書き換える
        /// </summary>
        /// <param name="rewriteByToolRecord"></param>
        /// <returns></returns>
        IEnumerable<NCProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord);
    }

    /// <summary>
    /// RewriteByToolの引数用データクラス
    /// </summary>
    /// <param name="RewritableCodes">書き換え元NCプログラム</param>
    /// <param name="Material">素材</param>
    /// <param name="Thickness">板厚</param>
    /// <param name="SubProgramNumber">サブプログラム番号</param>
    /// <param name="DirectedOperationToolDiameter">目標工具径 :サブプログラムで指定した工具径</param>
    /// <param name="CrystalReamerParameters">クリスタルリーマパラメータ</param>
    /// <param name="SkillReamerParameters">スキルリーマパラメータ</param>
    /// <param name="TapParameters">タップパラメータ</param>
    /// <param name="DrillingPrameters">ドリルパラメータ</param>
    public record class RewriteByToolRecord(
        IEnumerable<NCProgramCode> RewritableCodes,
        MaterialType Material,
        decimal Thickness,
        string SubProgramNumber,
        decimal DirectedOperationToolDiameter,
        IEnumerable<ReamingProgramPrameter> CrystalReamerParameters,
        IEnumerable<ReamingProgramPrameter> SkillReamerParameters,
        IEnumerable<TappingProgramPrameter> TapParameters,
        IEnumerable<DrillingProgramPrameter> DrillingPrameters);

    public class TestRewriteByToolRecordFactory
    {
        public static RewriteByToolRecord Create(
            IEnumerable<NCProgramCode>? rewritableCodes = default,
            MaterialType material = MaterialType.Aluminum,
            decimal thickness = 12.3m,
            string subProgramNumber = "1000",
            decimal directedOperationToolDiameter = 13.3m,
            IEnumerable<ReamingProgramPrameter>? crystalReamerParameters = default,
            IEnumerable<ReamingProgramPrameter>? skillReamerParameters = default,
            IEnumerable<TappingProgramPrameter>? tapParameters = default,
            IEnumerable<DrillingProgramPrameter>? drillingPrameters = default)
        {
            rewritableCodes ??= new List<NCProgramCode>
            {
                TestNCProgramCodeFactory.Create(mainProgramType: NCProgramType.CenterDrilling),
                TestNCProgramCodeFactory.Create(mainProgramType: NCProgramType.Drilling),
                TestNCProgramCodeFactory.Create(mainProgramType: NCProgramType.Chamfering),
                TestNCProgramCodeFactory.Create(mainProgramType: NCProgramType.Reaming),
                TestNCProgramCodeFactory.Create(mainProgramType: NCProgramType.Tapping),
            };
            crystalReamerParameters ??= new List<ReamingProgramPrameter>
            {
                TestReamingProgramPrameterFactory.Create(),
            };
            skillReamerParameters ??= new List<ReamingProgramPrameter>
            {
                TestReamingProgramPrameterFactory.Create(),
            };
            tapParameters ??= new List<TappingProgramPrameter>
            {
                TestTappingProgramPrameterFactory.Create(),
            };
            drillingPrameters ??= new List<DrillingProgramPrameter>
            {
                TestDrillingProgramPrameterFactory.Create(
                    DiameterKey: "9.1",
                    CenterDrillDepth: -1.5m,
                    CutDepth: 2.5m,
                    SpinForAluminum: 1100m,
                    FeedForAluminum: 130m,
                    SpinForIron: 710m,
                    FeedForIron: 100m),
                TestDrillingProgramPrameterFactory.Create(
                    DiameterKey: "11.1",
                    CenterDrillDepth: -1.5m,
                    CutDepth: 3,
                    SpinForAluminum: 870,
                    FeedForAluminum: 110,
                    SpinForIron: 580,
                    FeedForIron: 80),
                TestDrillingProgramPrameterFactory.Create(),
                TestDrillingProgramPrameterFactory.Create(
                    DiameterKey: "15.3",
                    CenterDrillDepth: -1.5m,
                    CutDepth: 3.5m,
                    SpinForAluminum: 740m,
                    FeedForAluminum: 100m,
                    SpinForIron: 490m,
                    FeedForIron: 70m),
            };

            return new(rewritableCodes, material, thickness, subProgramNumber, directedOperationToolDiameter, crystalReamerParameters, skillReamerParameters, tapParameters, drillingPrameters);
        }
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