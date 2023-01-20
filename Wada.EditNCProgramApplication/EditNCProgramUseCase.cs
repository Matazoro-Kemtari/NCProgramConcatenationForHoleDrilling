using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter;
using Wada.NCProgramConcatenationService.ValueObjects;
using Wada.UseCase.DataClass;

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
            // EditNCProgramPramのValidateで不整合状態は確認済み
            IMainProgramParameterRewriter rewriter = editNCProgramPram.DirectedOperation switch
            {
                DirectedOperationTypeAttempt.Tapping => _tappingParameterRewriter,
                DirectedOperationTypeAttempt.Reaming => editNCProgramPram.Reamer == ReamerTypeAttempt.Crystal ? _crystalReamingParameterRewriter : _skillReamingParameterRewriter,
                DirectedOperationTypeAttempt.Drilling => _drillingParameterRewriter,
                _ => throw new NotImplementedException(),
            };

            return await Task.Run(() => rewriter.RewriteByTool(
                editNCProgramPram.RewritableCodeDic,
                (NCProgramConcatenationService.ParameterRewriter.MaterialType)editNCProgramPram.Material,
                editNCProgramPram.Thickness,
                editNCProgramPram.TargetToolDiameter,
                editNCProgramPram.MainNCProgramParameters.ConvertMainProgramParametersRecord()));
        }
    }

    public record class EditNCProgramPram(
        DirectedOperationTypeAttempt DirectedOperation,
        string SubProgramNumger,
        decimal TargetToolDiameter,
        IEnumerable<NCProgramCodeAttempt> RewritableCoded,
        MaterialTypeAttempt Material,
        ReamerTypeAttempt Reamer,
        decimal Thickness,
        MainNCProgramParametersPram MainNCProgramParameters)
    {
        private static ReamerTypeAttempt Validate(DirectedOperationTypeAttempt directedOperation, ReamerTypeAttempt reamer)
        {
            if (directedOperation != DirectedOperationTypeAttempt.Reaming)
            {
                if (reamer != ReamerTypeAttempt.Undefined)
                    throw new InvalidOperationException($"指示が不整合です 作業指示: {directedOperation} リーマ: {reamer}");

                return reamer;
            }

            if (reamer == ReamerTypeAttempt.Undefined)
                throw new InvalidOperationException($"指示が不整合です 作業指示: {directedOperation} リーマ: {reamer}");

            return reamer;
        }

        public ReamerTypeAttempt Reamer { get; init; } = Validate(DirectedOperation, Reamer);
    }

    public class TestEditNCProgramPramFactory
    {
        public static EditNCProgramPram Create(
            DirectedOperationTypeAttempt directedOperation = DirectedOperationTypeAttempt.Drilling,
            string subProgramNumger = "8000",
            decimal targetToolDiameter = 13.2m,
            IEnumerable<NCProgramCodeAttempt>? rewritableCodes = default,
            MaterialTypeAttempt material = MaterialTypeAttempt.Aluminum,
            ReamerTypeAttempt reamer = ReamerTypeAttempt.Crystal,
            decimal thickness = 15,
            MainNCProgramParametersPram? mainNCProgramParameters = default)
        {
            rewritableCodes ??= new List<NCProgramCodeAttempt>
            {
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.CenterDrilling,
                    ProgramName: "O1000",
                    new List<NCBlockAttempt>
                    {
                        TestNCBlockAttemptFactory.Create(),
                    }),
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.Drilling,
                    ProgramName: "O2000",
                    new List<NCBlockAttempt>
                    {
                        TestNCBlockAttemptFactory.Create(),
                    }),
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.Chamfering,
                    ProgramName: "O3000",
                    new List<NCBlockAttempt>
                    {
                        TestNCBlockAttemptFactory.Create(),
                    }),
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.Reaming,
                    ProgramName: "O4000",
                    new List<NCBlockAttempt>
                    {
                        TestNCBlockAttemptFactory.Create(),
                    }),
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.Tapping,
                    ProgramName: "O5000",
                    new List<NCBlockAttempt>
                    {
                        TestNCBlockAttemptFactory.Create(),
                    }),
            };

            mainNCProgramParameters ??= TestMainNCProgramParametersPramFactory.Create();

            return new(directedOperation,
                       subProgramNumger,
                       targetToolDiameter,
                       rewritableCodes,
                       material,
                       reamer,
                       thickness,
                       mainNCProgramParameters);
        }
    }

    //public record class NCProgramCodePram(string ID, string ProgramName, IEnumerable<NCBlockPram?> NCBlocks)
    //{
    //    internal NCProgramCode Convert() => NCProgramCode.ReConstruct(ID, ProgramName, NCBlocks.Select(x => x?.Convert());
    //}

    //public record class NCBlockPram(IEnumerable<INCWord> NCWords, OptionalBlockSkip HasBlockSkip)
    //{
    //    internal NCBlock? Convert()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public enum DirectedOperationTypeAttempt
    {
        Tapping,
        Reaming,
        Drilling,
    }


    public enum MachineToolTypeAttempt
    {
        Undefined,
        RB250F,
        RB260,
        Triaxial,
    }

    public enum MaterialTypeAttempt
    {
        Undefined,
        Aluminum,
        Iron,
    }

    public enum ReamerTypeAttempt
    {
        Undefined,
        Crystal,
        Skill
    }

    public record class MainNCProgramParametersPram(
        IEnumerable<ReamingProgramPrameterPram> CrystalReamerParameters,
        IEnumerable<ReamingProgramPrameterPram> SkillReamerParameters,
        IEnumerable<TappingProgramPrameterPram> TapParameters,
        IEnumerable<DrillingProgramPrameterPram> DrillingPrameters)
    {
        public MainProgramParametersRecord ConvertMainProgramParametersRecord() => new(new()
        {
            { ParameterType.CrystalReamerParameter, CrystalReamerParameters.Select(x => x.ConvertReamingProgramPrameter()) },
            { ParameterType.SkillReamerParameter, SkillReamerParameters.Select(x => x.ConvertReamingProgramPrameter()) },
            { ParameterType.TapParameter, TapParameters.Select(x => x.ConvertTappingProgramPrameter()) },
            { ParameterType.DrillParameter, DrillingPrameters.Select(x => x.ConvertDrillingProgramPrameter()) },
        });
    }

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
        decimal? ChamferingDepth)
    {
        public ReamingProgramPrameter ConvertReamingProgramPrameter() => new(DiameterKey, PreparedHoleDiameter, SecondPreparedHoleDiameter, CenterDrillDepth, ChamferingDepth);
    }

    public record class TappingProgramPrameterPram(
        string DiameterKey,
        decimal PreparedHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth,
        decimal SpinForAluminum,
        decimal FeedForAluminum,
        decimal SpinForIron,
        decimal FeedForIron)
    {
        public TappingProgramPrameter ConvertTappingProgramPrameter() => new(DiameterKey, PreparedHoleDiameter, CenterDrillDepth, ChamferingDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);
    }

    public record class DrillingProgramPrameterPram(
        string DiameterKey,
        decimal CenterDrillDepth,
        decimal CutDepth,
        decimal SpinForAluminum,
        decimal FeedForAluminum,
        decimal SpinForIron,
        decimal FeedForIron)
    {
        public DrillingProgramPrameter ConvertDrillingProgramPrameter() => new DrillingProgramPrameter(DiameterKey, CenterDrillDepth, CutDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);
    }
}