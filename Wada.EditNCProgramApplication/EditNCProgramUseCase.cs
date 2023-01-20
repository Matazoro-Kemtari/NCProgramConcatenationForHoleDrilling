using Wada.AOP.Logging;
using Wada.Extension;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter;
using Wada.NCProgramConcatenationService.ValueObjects;


namespace Wada.EditNCProgramApplication
{
    public interface IEditNCProgramUseCase
    {
        Task<IEnumerable<NCProgramCode>> ExecuteAsync(EditNCProgramPram editNCProgramPram);
    }

    public class EditNCProgramUseCase : IEditNCProgramUseCase
    {
        private readonly IMainProgramParameterRewriter _crystalReamingParameterRewriter;
        private readonly IMainProgramParameterRewriter _skillReamingParameterRewriter;
        private readonly IMainProgramParameterRewriter _tappingParameterRewriter;
        private readonly IMainProgramParameterRewriter _drillingParameterRewriter;

        public EditNCProgramUseCase(
            CrystalReamingParameterRewriter crystalReamingParameterRewriter,
            SkillReamingParameterRewriter skillReamingParameterRewriter,
            TappingParameterRewriter tappingParameterRewriter,
            DrillingParameterRewriter drillingParameterRewriter)
        {
            _crystalReamingParameterRewriter = crystalReamingParameterRewriter;
            _skillReamingParameterRewriter = skillReamingParameterRewriter;
            _tappingParameterRewriter = tappingParameterRewriter;
            _drillingParameterRewriter = drillingParameterRewriter;
        }

        [Logging]
        public async Task<IEnumerable<NCProgramCode>> ExecuteAsync(EditNCProgramPram editNCProgramPram)
        {
            var taskRewriters = editNCProgramPram.rewritableCodeDic.Select(x => Task.Run(() =>
            {
                switch (x.Key)
                {
                    case MainProgramType.CenterDrilling:

                }
                NCProgramCode ncProgramCode = x.Value;
                throw new NotImplementedException();
                //return _mainParameterRewriter.RewriteProgramParameter();
            }));
            // TODO: domain service完成後に直し
            throw new NotImplementedException();
            //return await Task.WhenAll(taskRewriters);
        }
    }

    public record class EditNCProgramPram(
        DirectedOperationType DirectedOperation,
        string SubProgramNumger,
        Dictionary<MainProgramType, NCProgramCodePram> rewritableCodeDic,
        MaterialType Material,
        ReamerType Reamer,
        decimal Thickness,
        MainNCProgramParametersPram MainNCProgramParameters);

    public class TestEditNCProgramPramFactory
    {
        public static EditNCProgramPram Create(
            DirectedOperationType directedOperation = DirectedOperationType.Drilling,
            string subProgramNumger = "8000",
            Dictionary<MainProgramType, NCProgramCodePram>? rewritableCodes = default,
            MaterialType material = MaterialType.Aluminum,
            ReamerType reamer = ReamerType.Crystal,
            decimal thickness = 15,
            MainNCProgramParametersPram? mainNCProgramParameters = default)
        {
            rewritableCodes ??= new Dictionary<MainProgramType, NCProgramCodePram>()
            {
                {
                    MainProgramType.CenterDrilling,
                    TestNCProgramCodeFactory.Create(
                        programName: "O1000",
                        new List<NCBlock>
                        {
                            TestNCBlockFactory.Create(
                                new List<INCWord>
                                {
                                    TestNCWordFactory.Create(
                                        TestAddressFactory.Create(),
                                        TestNumericalValueFactory.Create()),
                                }),
                        })
                },
                {
                    MainProgramType.Drilling,
                    TestNCProgramCodeFactory.Create(
                        programName: "O2000",
                        new List<NCBlock>
                        {
                            TestNCBlockFactory.Create(
                                new List<INCWord>
                                {
                                    TestNCWordFactory.Create(
                                        TestAddressFactory.Create(),
                                        TestNumericalValueFactory.Create()),
                                }),
                        })
                },
                {
                    MainProgramType.Chamfering,
                    TestNCProgramCodeFactory.Create(
                        programName: "O3000",
                        new List<NCBlock>
                        {
                            TestNCBlockFactory.Create(
                                new List<INCWord>
                                {
                                    TestNCWordFactory.Create(
                                        TestAddressFactory.Create(),
                                        TestNumericalValueFactory.Create()),
                                }),
                        })
                },
                {
                    MainProgramType.Reaming,
                    TestNCProgramCodeFactory.Create(
                        programName: "O4000",
                        new List<NCBlock>
                        {
                            TestNCBlockFactory.Create(
                                new List<INCWord>
                                {
                                    TestNCWordFactory.Create(
                                        TestAddressFactory.Create(),
                                        TestNumericalValueFactory.Create()),
                                }),
                        })
                },
                {
                    MainProgramType.Tapping,
                    TestNCProgramCodeFactory.Create(
                        programName: "O5000",
                        new List<NCBlock>
                        {
                            TestNCBlockFactory.Create(
                                new List<INCWord>
                                {
                                    TestNCWordFactory.Create(
                                        TestAddressFactory.Create(),
                                        TestNumericalValueFactory.Create()),
                                }),
                        })
                },
            };

            mainNCProgramParameters ??= TestMainNCProgramParametersPramFactory.Create();

            return new(directedOperation,
                       subProgramNumger,
                       rewritableCodes,
                       material,
                       reamer,
                       thickness,
                       mainNCProgramParameters);
        }
    }

    public record class NCProgramCodePram(string ID, string ProgramName, IEnumerable<NCBlockPram?> NCBlocks);

    public record class NCBlockPram(IEnumerable<INCWord> NCWords, OptionalBlockSkip HasBlockSkip);

    public enum DirectedOperationType
    {
        Tapping,
        Reaming,
        Drilling,
    }

    public enum MainProgramType
    {
        CenterDrilling,
        Drilling,
        Chamfering,
        Reaming,
        Tapping,
    }

    public enum MachineToolType
    {
        Undefined,
        RB250F,
        RB260,
        Triaxial,
    }

    public enum MaterialType
    {
        Undefined,
        Aluminum,
        Iron,
    }

    public enum ReamerType
    {
        Undefined,
        Crystal,
        Skill
    }

    public record class MainNCProgramParametersPram(
        IEnumerable<ReamingProgramPrameterPram> CrystalReamerParameters,
        IEnumerable<ReamingProgramPrameterPram> SkillReamerParameters,
        IEnumerable<TappingProgramPrameterPram> TapParameters,
        IEnumerable<DrillingProgramPrameter> DrillingPrameters);

    public class TestMainNCProgramParametersPramFactory
    {
        public static MainNCProgramParametersPram Create(
            IEnumerable<ReamingProgramPrameterPram>? crystalReamerParameters = default,
            IEnumerable<ReamingProgramPrameterPram>? skillReamerParameters = default,
            IEnumerable<TappingProgramPrameterPram>? tapParameters = default,
            IEnumerable<DrillingProgramPrameter>? drillingPrameters = default)
        {
            decimal reamerDiameter = 13.3m;
            decimal fastDrill = 10m;
            decimal secondDrill = 11.8m;
            decimal centerDrillDepth = -1.5m;
            decimal? chamferingDepth = -6.1m;

            crystalReamerParameters ??= new List<ReamingProgramPrameterPram>
            {
                new(reamerDiameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth),
            };
            skillReamerParameters ??= new List<ReamingProgramPrameterPram>
            {
                new(reamerDiameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth),
            };
            tapParameters ??= new List<TappingProgramPrameterPram>
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
            drillingPrameters ??= new List<DrillingProgramPrameter>
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
    public record class ReamingProgramPrameterPram(
        string DiameterKey,
        decimal PreparedHoleDiameter,
        decimal SecondPreparedHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth);

    public record class TappingProgramPrameterPram(
        string DiameterKey,
        decimal PreparedHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth,
        decimal SpinForAluminum,
        decimal FeedForAluminum,
        decimal SpinForIron,
        decimal FeedForIron);

    public record class DrillingProgramPrameterPram(
        string DiameterKey,
        decimal CenterDrillDepth,
        decimal CutDepth,
        decimal SpinForAluminum,
        decimal FeedForAluminum,
        decimal SpinForIron,
        decimal FeedForIron);
}