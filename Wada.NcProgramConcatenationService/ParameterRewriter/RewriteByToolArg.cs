using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

/// <summary>
/// RewriteByToolの引数用データクラス
/// </summary>
public record class RewriteByToolArg
{
    public RewriteByToolArg(IEnumerable<NcProgramCode> rewritableCodes,
                            MaterialType material,
                            decimal thickness,
                            string subProgramNumber,
                            decimal directedOperationToolDiameter,
                            DrillingMethod drillingMethod,
                            decimal blindPilotHoleDepth,
                            decimal blindHoleDepth,
                            IEnumerable<ReamingProgramParameter> crystalReamerParameters,
                            IEnumerable<ReamingProgramParameter> skillReamerParameters,
                            IEnumerable<TappingProgramParameter> tapParameters,
                            IEnumerable<DrillingProgramParameter> drillingParameters)
    {
        RewritableCodes = rewritableCodes ?? throw new ArgumentNullException(nameof(rewritableCodes));
        Material = material;
        Thickness = thickness;
        SubProgramNumber = subProgramNumber ?? throw new ArgumentNullException(nameof(subProgramNumber));
        DirectedOperationToolDiameter = directedOperationToolDiameter;
        DrillingMethod = drillingMethod;
        BlindPilotHoleDepth = blindPilotHoleDepth;
        CrystalReamerParameters = crystalReamerParameters ?? throw new ArgumentNullException(nameof(crystalReamerParameters));
        SkillReamerParameters = skillReamerParameters ?? throw new ArgumentNullException(nameof(skillReamerParameters));
        TapParameters = tapParameters ?? throw new ArgumentNullException(nameof(tapParameters));
        DrillingParameters = drillingParameters ?? throw new ArgumentNullException(nameof(drillingParameters));
        BlindHoleDepth = drillingMethod switch
        {
            DrillingMethod.BlindHole => blindHoleDepth,
            _ => default,
        };

    // TODO: DrillingSequenceBuilderでやってるチェックをここでやる
        //var maxDiameter = drillingParameters.MaxBy(x => x.DirectedOperationToolDiameter)
        //    ?.DirectedOperationToolDiameter;
        //if (maxDiameter == null
        //    || maxDiameter + 0.5m < directedOperationToolDiameter)
        //    throw new DomainException(
        //        $"ドリル径 {directedOperationToolDiameter}のリストがありません\n" +
        //        $"リストの最大ドリル径({maxDiameter})を超えています");

        //DrillingProgramParameter drillingParameter = drillingParameters
        //    .Where(x => x.DirectedOperationToolDiameter <= directedOperationToolDiameter)
        //    .MaxBy(x => x.DirectedOperationToolDiameter)
        //    ?? throw new DomainException(
        //        $"ドリル径 {directedOperationToolDiameter}のリストがありません");
    }

    /// <summary>
    /// 書き換え元NCプログラム
    /// </summary>
    public IEnumerable<NcProgramCode> RewritableCodes { get; init; }

    /// <summary>
    /// 素材
    /// </summary>
    public MaterialType Material { get; init; }

    /// <summary>
    /// 板厚
    /// </summary>
    public decimal Thickness { get; init; }

    /// <summary>
    /// サブプログラム番号
    /// </summary>
    public string SubProgramNumber { get; init; }

    /// <summary>
    /// 目標工具径 :サブプログラムで指定した工具径
    /// </summary>
    public decimal DirectedOperationToolDiameter { get; init; }

    /// <summary>
    /// 穴加工方法
    /// </summary>
    public DrillingMethod DrillingMethod { get; init; }

    /// <summary>
    /// 止まり穴下穴深さ
    /// </summary>
    public decimal BlindPilotHoleDepth { get; init; }
    /// <summary>
    /// 止まり穴深さ
    /// </summary>
    public decimal BlindHoleDepth { get; init; }

    /// <summary>
    /// クリスタルリーマパラメータ
    /// </summary>
    public IEnumerable<ReamingProgramParameter> CrystalReamerParameters { get; init; }

    /// <summary>
    /// スキルリーマパラメータ
    /// </summary>
    public IEnumerable<ReamingProgramParameter> SkillReamerParameters { get; init; }

    /// <summary>
    /// タップパラメータ
    /// </summary>
    public IEnumerable<TappingProgramParameter> TapParameters { get; init; }

    /// <summary>
    /// ドリルパラメータ
    /// </summary>
    public IEnumerable<DrillingProgramParameter> DrillingParameters { get; init; }
}

public class TestRewriteByToolArgFactory
{
    public static RewriteByToolArg Create(
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
