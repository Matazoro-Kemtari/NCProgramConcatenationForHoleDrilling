using Wada.Extensions;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

/// <summary>
/// RewriteByToolの引数用データクラス
/// </summary>
public record class ToolParameter
{
    private readonly Dictionary<RewriterSelector, IEnumerable<IMainProgramParameter>> _toolParameters;
    private readonly Dictionary<RewriterSelector, NcProgramCode> _rewritableCodes;

    public ToolParameter(IEnumerable<NcProgramCode> rewritableCodes,
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

        _toolParameters = new()
        {
            { RewriterSelector.CrystalReaming, crystalReamerParameters },
            { RewriterSelector.SkillReaming, skillReamerParameters },
            { RewriterSelector.Tapping, tapParameters },
            { RewriterSelector.Drilling, drillingParameters },
        };

        _rewritableCodes = new()
        {
            { RewriterSelector.CrystalReaming, RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.Reaming) },
            { RewriterSelector.SkillReaming, RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.Reaming) },
            { RewriterSelector.Tapping, RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.Tapping) },
            { RewriterSelector.Drilling, RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.Drilling) },
        };
    }

    internal CenterDrillingRewriteParameter ToCenterDrillingRewriteParameter(RewriterSelector rewriterSelector)
    {
        IMainProgramParameter mainProgramParameter = LookForToolParameter(rewriterSelector);

        return new CenterDrillingRewriteParameter(
            RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.CenterDrilling),
            Material,
            mainProgramParameter,
            SubProgramNumber);
    }

    internal DrillingRewriteParameter ToDrillingRewriteParameter()
    {
        // リスト中の最大工具径を取得
        var maxDiameter = DrillingParameters.MaxBy(x => x.DirectedOperationToolDiameter)
            ?.DirectedOperationToolDiameter;

        // サブプログラム指定の工具径がリストの範囲を超えるか
        if (maxDiameter == null
            || maxDiameter + 0.5m < DirectedOperationToolDiameter)
            throw new DomainException(
                $"ドリル径 {DirectedOperationToolDiameter}のリストがありません\n" +
                $"リストの最大ドリル径({maxDiameter})を超えています");

        // サブプログラム指定の工具径に一致するリストを取得
        var drillingParameter = DrillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= DirectedOperationToolDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"ドリル径 {DirectedOperationToolDiameter}のリストがありません");

        return new DrillingRewriteParameter(
            RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.Drilling),
            Material,
            DrillingMethod switch
            {
                DrillingMethod.ThroughHole => Thickness + drillingParameter.DrillTipLength,
                DrillingMethod.BlindHole => BlindHoleDepth,
                _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
            },
            drillingParameter,
            SubProgramNumber,
            DirectedOperationToolDiameter);
    }

    internal DrillingRewriteParameter ToPilotDrillingRewriteParameter(RewriterSelector rewriterSelector)
    {
        if (rewriterSelector is not RewriterSelector.CrystalReaming and not RewriterSelector.SkillReaming and not RewriterSelector.Tapping)
            throw new AggregateException("このメソッドはリーマー・タップ用です");

        // 対象工具のパラメーターを取得する
        IPilotHoleDrilledParameter toolParameter = (IPilotHoleDrilledParameter)LookForToolParameter(rewriterSelector);

        // 下穴用のパラメーターを取得する
        var drillingParameter = DrillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= toolParameter.PilotHoleDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {toolParameter.PilotHoleDiameter}");

        //　下穴深さを取得する
        var drillingDepth = DrillingMethod switch
        {
            DrillingMethod.ThroughHole => Thickness + drillingParameter.DrillTipLength,
            DrillingMethod.BlindHole => BlindPilotHoleDepth,
            _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
        };

        return new DrillingRewriteParameter(
            RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.Drilling),
            Material,
            drillingDepth,
            drillingParameter,
            SubProgramNumber,
            toolParameter.PilotHoleDiameter);
    }

    internal DrillingRewriteParameter ToSecondaryPilotDrillingRewriteParameter(RewriterSelector rewriterSelector)
    {
        if (rewriterSelector is not RewriterSelector.CrystalReaming and not RewriterSelector.SkillReaming)
            throw new AggregateException("このメソッドはリーマー用です");

        // 対象工具のパラメーターを取得する
        ReamingProgramParameter toolParameter = (ReamingProgramParameter)LookForToolParameter(rewriterSelector);

        // 下穴用のパラメーターを取得する
        var drillingParameter = DrillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= toolParameter.SecondaryPilotHoleDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {toolParameter.SecondaryPilotHoleDiameter}");

        //　下穴深さを取得する
        var secondDrillingDepth = DrillingMethod switch
        {
            DrillingMethod.ThroughHole => Thickness + drillingParameter.DrillTipLength,
            DrillingMethod.BlindHole => BlindPilotHoleDepth,
            _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
        };

        return new DrillingRewriteParameter(
            RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.Drilling),
            Material,
            secondDrillingDepth,
            drillingParameter,
            SubProgramNumber,
            toolParameter.SecondaryPilotHoleDiameter);
    }

    internal ChamferingRewriteParameter ToChamferingRewriteParameter(RewriterSelector rewriterSelector)
    {
        // 対象工具のパラメーターを取得する
        IMainProgramParameter mainProgramParameter = LookForToolParameter(rewriterSelector);

        return new ChamferingRewriteParameter(
            RewritableCodes.Single(x => x.MainProgramClassification == NcProgramRole.Chamfering),
            Material,
            mainProgramParameter,
            SubProgramNumber);
    }

    internal ReamingRewriteParameter ToReamingRewriteParameter(RewriterSelector rewriterSelector)
    {
        if (rewriterSelector is not RewriterSelector.CrystalReaming and not RewriterSelector.SkillReaming)
            throw new AggregateException("このメソッドはリーマー用です");

        // 対象工具のパラメーターを取得する
        IMainProgramParameter mainProgramParameter = LookForToolParameter(rewriterSelector);

        return new ReamingRewriteParameter(
            _rewritableCodes[rewriterSelector],
            Material,
            rewriterSelector switch
            {
                RewriterSelector.CrystalReaming => ReamerType.CrystalReamerParameter,
                RewriterSelector.SkillReaming => ReamerType.SkillReamerParameter,
                _ => throw new NotImplementedException($"{nameof(rewriterSelector)}の値が想定外です"),
            },
            DrillingMethod switch
            {
                DrillingMethod.ThroughHole => Thickness + 5m,
                DrillingMethod.BlindHole => BlindHoleDepth,
                _ => throw new NotImplementedException($"{nameof(DrillingMethod)}の値が想定外の値です"),
            },
            mainProgramParameter,
            SubProgramNumber);
    }
    internal TappingRewriteParameter ToTappingRewriteParameter()
    {
        // 対象工具のパラメーターを取得する
        IMainProgramParameter mainProgramParameter = LookForToolParameter(RewriterSelector.Tapping);

        return new TappingRewriteParameter(
            _rewritableCodes[RewriterSelector.Tapping],
            Material,
            DrillingMethod switch
            {
                DrillingMethod.ThroughHole => Thickness + 5m,
                DrillingMethod.BlindHole => BlindHoleDepth,
                _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
            },
            mainProgramParameter,
            SubProgramNumber);
    }


    private IMainProgramParameter LookForToolParameter(RewriterSelector rewriterSelector)
    {
        IMainProgramParameter mainProgramParameter;
        try
        {
            mainProgramParameter = _toolParameters[rewriterSelector]
                .First(x => x.DirectedOperationToolDiameter == DirectedOperationToolDiameter);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainException(
                $"{rewriterSelector.GetEnumDisplayName()}径 {DirectedOperationToolDiameter}のリストがありません", ex);
        }

        return mainProgramParameter;
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
    public static ToolParameter Create(
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
