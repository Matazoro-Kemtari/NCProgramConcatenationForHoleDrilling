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
                    SpinForAluminum: 160,
                    FeedForAluminum: 280,
                    SpinForIron: 120,
                    FeedForIron: 210),
            };
            drillingPrameters ??= new List<DrillingProgramPrameterAttempt>
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
        int SpinForAluminum,
        int FeedForAluminum,
        int SpinForIron,
        int FeedForIron)
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
        int SpinForAluminum,
        int FeedForAluminum,
        int SpinForIron,
        int FeedForIron)
    {
        public DrillingProgramPrameter Convert() => new(DiameterKey, CenterDrillDepth, CutDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);

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