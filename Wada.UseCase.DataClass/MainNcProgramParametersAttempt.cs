using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.UseCase.DataClass;

public record class MainNcProgramParametersAttempt
{
    private readonly Dictionary<DirectedOperationTypeAttempt, Func<decimal, bool>> _parameterPolicies;

    public MainNcProgramParametersAttempt(
        IEnumerable<ReamingProgramParameterAttempt> crystalReamerParameters,
        IEnumerable<ReamingProgramParameterAttempt> skillReamerParameters,
        IEnumerable<TappingProgramParameterAttempt> tapParameters,
        IEnumerable<DrillingProgramParameterAttempt> drillingParameters)
    {
        CrystalReamerParameters = crystalReamerParameters;
        SkillReamerParameters = skillReamerParameters;
        TapParameters = tapParameters;
        DrillingParameters = drillingParameters;

        _parameterPolicies = new()
        {
            { DirectedOperationTypeAttempt.Reaming, ExistsReamingProgramParameter },
            { DirectedOperationTypeAttempt.Tapping, ExistsTappingProgramParameter },
            { DirectedOperationTypeAttempt.Drilling, ExistsDrillingProgramParameter },
        };
    }

    public bool CheckParameterPresence(OperationDirecterAttemp operationDirecter)
        => _parameterPolicies[operationDirecter.DirectedOperationClassification](operationDirecter.DirectedOperationToolDiameter);

    private bool ExistsReamingProgramParameter(decimal diameter)
        => CrystalReamerParameters.Union(SkillReamerParameters)
                                  .Any(x => x.DirectedOperationToolDiameter == diameter);

    private bool ExistsTappingProgramParameter(decimal diameter)
        => TapParameters.Any(x => x.DirectedOperationToolDiameter == diameter);

    private bool ExistsDrillingProgramParameter(decimal diameter)
        => DrillingParameters.Any(x => x.DirectedOperationToolDiameter == diameter);

    public IEnumerable<ReamingProgramParameterAttempt> CrystalReamerParameters { get; init; }
    public IEnumerable<ReamingProgramParameterAttempt> SkillReamerParameters { get; init; }
    public IEnumerable<TappingProgramParameterAttempt> TapParameters { get; init; }
    public IEnumerable<DrillingProgramParameterAttempt> DrillingParameters { get; init; }
}

public class TestMainNcProgramParametersParamFactory
{
    public static MainNcProgramParametersAttempt Create(
        IEnumerable<ReamingProgramParameterAttempt>? crystalReamerParameters = default,
        IEnumerable<ReamingProgramParameterAttempt>? skillReamerParameters = default,
        IEnumerable<TappingProgramParameterAttempt>? tapParameters = default,
        IEnumerable<DrillingProgramParameterAttempt>? drillingParameters = default)
    {
        decimal reamerDiameter = 13.3m;
        decimal fastDrill = 10m;
        decimal secondDrill = 11.8m;
        decimal centerDrillDepth = -1.5m;
        decimal? chamferingDepth = -6.1m;

        crystalReamerParameters ??= new List<ReamingProgramParameterAttempt>
        {
            new(reamerDiameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth),
        };
        skillReamerParameters ??= new List<ReamingProgramParameterAttempt>
        {
            new(reamerDiameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth),
        };
        tapParameters ??= new List<TappingProgramParameterAttempt>
        {
            new(DiameterKey: "M12",
                PilotHoleDiameter: fastDrill,
                CenterDrillDepth: centerDrillDepth,
                ChamferingDepth: -6.3m,
                SpinForAluminum: 160,
                FeedForAluminum: 280,
                SpinForIron: 120,
                FeedForIron: 210),
        };
        drillingParameters ??= new List<DrillingProgramParameterAttempt>
        {
            new(DiameterKey: fastDrill.ToString(),
                CenterDrillDepth: -1.5m,
                CutDepth: 3m,
                SpinForAluminum: 960,
                FeedForAluminum: 130,
                SpinForIron: 640,
                FeedForIron: 90),
            new(DiameterKey: secondDrill.ToString(),
                CenterDrillDepth: -1.5m,
                CutDepth: 3.5m,
                SpinForAluminum: 84,
                FeedForAluminum: 110,
                SpinForIron: 560,
                FeedForIron: 80),
        };

        return new(crystalReamerParameters, skillReamerParameters, tapParameters, drillingParameters);
    }
}

public record class ReamingProgramParameterAttempt(
    string DiameterKey,
    decimal PilotHoleDiameter,
    decimal SecondaryPilotHoleDiameter,
    decimal CenterDrillDepth,
    decimal? ChamferingDepth)
{
    public ReamingProgramParameter Convert() => new(DiameterKey, PilotHoleDiameter, SecondaryPilotHoleDiameter, CenterDrillDepth, ChamferingDepth);

    public static ReamingProgramParameterAttempt Parse(ReamingProgramParameter mainProgramParameter)
    => new(mainProgramParameter.DiameterKey,
           mainProgramParameter.PilotHoleDiameter,
           mainProgramParameter.SecondaryPilotHoleDiameter,
           mainProgramParameter.CenterDrillDepth,
           mainProgramParameter.ChamferingDepth);

    public decimal DirectedOperationToolDiameter
    {
        get => Convert().DirectedOperationToolDiameter;
    }
}

public record class TappingProgramParameterAttempt(
    string DiameterKey,
    decimal PilotHoleDiameter,
    decimal CenterDrillDepth,
    decimal? ChamferingDepth,
    int SpinForAluminum,
    int FeedForAluminum,
    int SpinForIron,
    int FeedForIron)
{
    public TappingProgramParameter Convert() => new(DiameterKey, PilotHoleDiameter, CenterDrillDepth, ChamferingDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);

    public static TappingProgramParameterAttempt Parse(TappingProgramParameter mainProgramParameter)
    => new(mainProgramParameter.DiameterKey,
           mainProgramParameter.PilotHoleDiameter,
           mainProgramParameter.CenterDrillDepth,
           mainProgramParameter.ChamferingDepth,
           mainProgramParameter.SpinForAluminum,
           mainProgramParameter.FeedForAluminum,
           mainProgramParameter.SpinForIron,
           mainProgramParameter.FeedForIron);

    public decimal DirectedOperationToolDiameter
    {
        get => Convert().DirectedOperationToolDiameter;
    }
}

public record class DrillingProgramParameterAttempt(
    string DiameterKey,
    decimal CenterDrillDepth,
    decimal CutDepth,
    int SpinForAluminum,
    int FeedForAluminum,
    int SpinForIron,
    int FeedForIron)
{
    public DrillingProgramParameter Convert() => new(DiameterKey, CenterDrillDepth, CutDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);

    public static DrillingProgramParameterAttempt Parse(DrillingProgramParameter mainProgramParameter)
    => new(mainProgramParameter.DiameterKey,
           mainProgramParameter.CenterDrillDepth,
           mainProgramParameter.CutDepth,
           mainProgramParameter.SpinForAluminum,
           mainProgramParameter.FeedForAluminum,
           mainProgramParameter.SpinForIron,
           mainProgramParameter.FeedForIron);

    public decimal DirectedOperationToolDiameter
    {
        get => Convert().DirectedOperationToolDiameter;
    }
}