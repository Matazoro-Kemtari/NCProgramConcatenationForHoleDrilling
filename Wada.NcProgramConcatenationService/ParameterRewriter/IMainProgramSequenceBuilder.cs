using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

public interface IMainProgramSequenceBuilder
{
    /// <summary>
    /// メインプログラムのパラメータを書き換える
    /// </summary>
    /// <param name="rewriteByToolRecord"></param>
    /// <returns></returns>
    IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord);
}

/// <summary>
/// RewriteByToolの引数用データクラス
/// </summary>
/// <param name="RewritableCodes">書き換え元NCプログラム</param>
/// <param name="Material">素材</param>
/// <param name="Thickness">板厚</param>
/// <param name="SubProgramNumber">サブプログラム番号</param>
/// <param name="DirectedOperationToolDiameter">目標工具径 :サブプログラムで指定した工具径</param>
/// <param name="DrillingMethod">穴加工方法</param>
/// <param name="BlindHoleDepth">止まり穴深さ</param>
/// <param name="CrystalReamerParameters">クリスタルリーマパラメータ</param>
/// <param name="SkillReamerParameters">スキルリーマパラメータ</param>
/// <param name="TapParameters">タップパラメータ</param>
/// <param name="DrillingParameters">ドリルパラメータ</param>
public record class RewriteByToolRecord(
    IEnumerable<NcProgramCode> RewritableCodes,
    MaterialType Material,
    decimal Thickness,
    string SubProgramNumber,
    decimal DirectedOperationToolDiameter,
    DrillingMethod DrillingMethod,
    decimal BlindPilotHoleDepth,
    decimal BlindHoleDepth,
    IEnumerable<ReamingProgramParameter> CrystalReamerParameters,
    IEnumerable<ReamingProgramParameter> SkillReamerParameters,
    IEnumerable<TappingProgramParameter> TapParameters,
    IEnumerable<DrillingProgramParameter> DrillingParameters)
{
    /// <summary>
    /// 止まり穴深さ
    /// </summary>
    public decimal BlindHoleDepth { get; } = DrillingMethod switch
    {
        DrillingMethod.BlindHole => BlindHoleDepth,
        _ => default,
    };
}

public class TestRewriteByToolRecordFactory
{
    public static RewriteByToolRecord Create(
        IEnumerable<NcProgramCode>? rewritableCodes = default,
        MaterialType material = MaterialType.Aluminum,
        decimal thickness = 12.3m,
        string subProgramNumber = "1000",
        decimal directedOperationToolDiameter = 13.3m,
        DrillingMethod drillingMethod = DrillingMethod.ThroughHole,
        decimal blindPilotHoleDepth = 0m,
        decimal blindHoleDepth = 0m,
        IEnumerable<ReamingProgramParameter>? crystalReamerParameters = default,
        IEnumerable<ReamingProgramParameter>? skillReamerParameters = default,
        IEnumerable<TappingProgramParameter>? tapParameters = default,
        IEnumerable<DrillingProgramParameter>? drillingParameters = default)
    {
        rewritableCodes ??= new List<NcProgramCode>
        {
            TestNcProgramCodeFactory.Create(mainProgramType: NcProgramRole.CenterDrilling),
            TestNcProgramCodeFactory.Create(mainProgramType: NcProgramRole.Drilling),
            TestNcProgramCodeFactory.Create(mainProgramType: NcProgramRole.Chamfering),
            TestNcProgramCodeFactory.Create(mainProgramType: NcProgramRole.Reaming),
            TestNcProgramCodeFactory.Create(mainProgramType: NcProgramRole.Tapping),
        };
        crystalReamerParameters ??= new List<ReamingProgramParameter>
        {
            TestReamingProgramParameterFactory.Create(),
        };
        skillReamerParameters ??= new List<ReamingProgramParameter>
        {
            TestReamingProgramParameterFactory.Create(),
        };
        tapParameters ??= new List<TappingProgramParameter>
        {
            TestTappingProgramParameterFactory.Create(),
        };
        drillingParameters ??= new List<DrillingProgramParameter>
        {
            TestDrillingProgramParameterFactory.Create(
                DiameterKey: "9.1",
                CenterDrillDepth: -1.5m,
                CutDepth: 2.5m,
                SpinForAluminum: 1100,
                FeedForAluminum: 130,
                SpinForIron: 710,
                FeedForIron: 100),
            TestDrillingProgramParameterFactory.Create(
                DiameterKey: "11.1",
                CenterDrillDepth: -1.5m,
                CutDepth: 3,
                SpinForAluminum: 870,
                FeedForAluminum: 110,
                SpinForIron: 580,
                FeedForIron: 80),
            TestDrillingProgramParameterFactory.Create(),
            TestDrillingProgramParameterFactory.Create(
                DiameterKey: "15.3",
                CenterDrillDepth: -1.5m,
                CutDepth: 3.5m,
                SpinForAluminum: 740,
                FeedForAluminum: 100,
                SpinForIron: 490,
                FeedForIron: 70),
        };

        return new(rewritableCodes,
                   material,
                   thickness,
                   subProgramNumber,
                   directedOperationToolDiameter,
                   drillingMethod,
                   blindPilotHoleDepth,
                   blindHoleDepth,
                   crystalReamerParameters,
                   skillReamerParameters,
                   tapParameters,
                   drillingParameters);
    }
}
