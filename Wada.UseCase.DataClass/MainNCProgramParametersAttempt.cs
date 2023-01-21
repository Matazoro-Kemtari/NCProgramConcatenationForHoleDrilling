using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.UseCase.DataClass
{
    public record class MainNCProgramParametersAttempt(
        IEnumerable<ReamingProgramPrameterAttempt> CrystalReamerParameters,
        IEnumerable<ReamingProgramPrameterAttempt> SkillReamerParameters,
        IEnumerable<TappingProgramPrameterAttempt> TapParameters,
        IEnumerable<DrillingProgramPrameterAttempt> DrillingPrameters);

    public class TestMainNCProgramParametersPramFactory
    {
        public static MainNCProgramParametersAttempt Create(
            IEnumerable<ReamingProgramPrameterAttempt>? crystalReamerParameters = default,
            IEnumerable<ReamingProgramPrameterAttempt>? skillReamerParameters = default,
            IEnumerable<TappingProgramPrameterAttempt>? tapParameters = default,
            IEnumerable<DrillingProgramPrameterAttempt>? drillingPrameters = default)
        {
            decimal reamerDiameter = 13.3m;
            decimal fastDrill = 10m;
            decimal secondDrill = 11.8m;
            decimal centerDrillDepth = -1.5m;
            decimal? chamferingDepth = -6.1m;

            crystalReamerParameters ??= new List<ReamingProgramPrameterAttempt>
            {
                new(reamerDiameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth),
            };
            skillReamerParameters ??= new List<ReamingProgramPrameterAttempt>
            {
                new(reamerDiameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth),
            };
            tapParameters ??= new List<TappingProgramPrameterAttempt>
            {
                new(DiameterKey: "M12",
                    PreparedHoleDiameter: fastDrill,
                    CenterDrillDepth: centerDrillDepth,
                    ChamferingDepth: -6.3m,
                    SpinForAluminum: 160m,
                    FeedForAluminum: 280m,
                    SpinForIron: 120m,
                    FeedForIron: 210m),
            };
            drillingPrameters ??= new List<DrillingProgramPrameterAttempt>
            {
                new(DiameterKey: fastDrill.ToString(),
                    CenterDrillDepth: -1.5m,
                    CutDepth: 3m,
                    SpinForAluminum: 960m,
                    FeedForAluminum: 130m,
                    SpinForIron: 640m,
                    FeedForIron: 90m),
                new(DiameterKey: secondDrill.ToString(),
                    CenterDrillDepth: -1.5m,
                    CutDepth: 3.5m,
                    SpinForAluminum: 84m,
                    FeedForAluminum: 110m,
                    SpinForIron: 560m,
                    FeedForIron: 80m)
            };

            return new(crystalReamerParameters, skillReamerParameters, tapParameters, drillingPrameters);
        }
    }

    public record class ReamingProgramPrameterAttempt(
        string DiameterKey,
        decimal PreparedHoleDiameter,
        decimal SecondPreparedHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth)
    {
        public ReamingProgramPrameter Convert() => new(DiameterKey, PreparedHoleDiameter, SecondPreparedHoleDiameter, CenterDrillDepth, ChamferingDepth);

        public static ReamingProgramPrameterAttempt Parse(ReamingProgramPrameter mainProgramPrameter)
        => new(mainProgramPrameter.DiameterKey,
               mainProgramPrameter.PreparedHoleDiameter,
               mainProgramPrameter.SecondPreparedHoleDiameter,
               mainProgramPrameter.CenterDrillDepth,
               mainProgramPrameter.ChamferingDepth);
    }

    public record class TappingProgramPrameterAttempt(
        string DiameterKey,
        decimal PreparedHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth,
        decimal SpinForAluminum,
        decimal FeedForAluminum,
        decimal SpinForIron,
        decimal FeedForIron)
    {
        public TappingProgramPrameter Convert() => new(DiameterKey, PreparedHoleDiameter, CenterDrillDepth, ChamferingDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);

        public static TappingProgramPrameterAttempt Parse(TappingProgramPrameter mainProgramPrameter)
        => new(mainProgramPrameter.DiameterKey,
               mainProgramPrameter.PreparedHoleDiameter,
               mainProgramPrameter.CenterDrillDepth,
               mainProgramPrameter.ChamferingDepth,
               mainProgramPrameter.SpinForAluminum,
               mainProgramPrameter.FeedForAluminum,
               mainProgramPrameter.SpinForIron,
               mainProgramPrameter.FeedForIron);
    }

    public record class DrillingProgramPrameterAttempt(
        string DiameterKey,
        decimal CenterDrillDepth,
        decimal CutDepth,
        decimal SpinForAluminum,
        decimal FeedForAluminum,
        decimal SpinForIron,
        decimal FeedForIron)
    {
        public DrillingProgramPrameter Convert() => new DrillingProgramPrameter(DiameterKey, CenterDrillDepth, CutDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);

        public static DrillingProgramPrameterAttempt Parse(DrillingProgramPrameter mainProgramPrameter)
        => new(mainProgramPrameter.DiameterKey,
               mainProgramPrameter.CenterDrillDepth,
               mainProgramPrameter.CutDepth,
               mainProgramPrameter.SpinForAluminum,
               mainProgramPrameter.FeedForAluminum,
               mainProgramPrameter.SpinForIron,
               mainProgramPrameter.FeedForIron);
    }
}