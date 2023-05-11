using System.Diagnostics;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.ParameterRewriter;
using Wada.UseCase.DataClass;

namespace Wada.EditNcProgramApplication
{
    public interface IEditNcProgramUseCase
    {
        Task<EditNcProgramDto> ExecuteAsync(EditNcProgramPram editNCProgramPram);
    }

    public class EditNcProgramUseCase : IEditNcProgramUseCase
    {
        private readonly IMainProgramParameterRewriter _crystalReamingParameterRewriter;
        private readonly IMainProgramParameterRewriter _skillReamingParameterRewriter;
        private readonly IMainProgramParameterRewriter _tappingParameterRewriter;
        private readonly IMainProgramParameterRewriter _drillingParameterRewriter;
        private readonly Dictionary<RewriterSelectorAttempt, IMainProgramParameterRewriter> _rewriter;

        public EditNcProgramUseCase(
            CrystalReamingParameterRewriter crystalReamingParameterRewriter,
            SkillReamingParameterRewriter skillReamingParameterRewriter,
            TappingParameterRewriter tappingParameterRewriter,
            DrillingParameterRewriter drillingParameterRewriter)
        {
            _crystalReamingParameterRewriter = crystalReamingParameterRewriter;
            _skillReamingParameterRewriter = skillReamingParameterRewriter;
            _tappingParameterRewriter = tappingParameterRewriter;
            _drillingParameterRewriter = drillingParameterRewriter;

            _rewriter = new()
            {
                { RewriterSelectorAttempt.Tapping, _tappingParameterRewriter },
                { RewriterSelectorAttempt.CrystalReaming, _crystalReamingParameterRewriter },
                { RewriterSelectorAttempt.SkillReaming, _skillReamingParameterRewriter },
                { RewriterSelectorAttempt.Drilling , _drillingParameterRewriter },
            };
        }

        [Logging]
        public async Task<EditNcProgramDto> ExecuteAsync(EditNcProgramPram editNCProgramPram)
        {
            RewriteByToolRecord param = new(
                editNCProgramPram.RewritableCodeds.Select(x => x.Convert()),
                (MaterialType)editNCProgramPram.Material,
                editNCProgramPram.Thickness,
                editNCProgramPram.SubProgramNumger,
                editNCProgramPram.DirectedOperationToolDiameter,
                editNCProgramPram.MainNcProgramParameters.CrystalReamerParameters.Select(x => x.Convert()),
                editNCProgramPram.MainNcProgramParameters.SkillReamerParameters.Select(x => x.Convert()),
                editNCProgramPram.MainNcProgramParameters.TapParameters.Select(x => x.Convert()),
                editNCProgramPram.MainNcProgramParameters.DrillingPrameters.Select(x => x.Convert()));

            try
            {
                return await Task.Run(
                    () => new EditNcProgramDto(
                        _rewriter[editNCProgramPram.RewriterSelector].RewriteByTool(param)
                        .Select(x => NcProgramCodeAttempt.Parse(x))));
            }
            catch (DomainException ex)
            {
                throw new EditNcProgramUseCaseException(ex.Message, ex);
            }
        }
    }

    public record class EditNcProgramPram(
        DirectedOperationTypeAttempt DirectedOperation,
        string SubProgramNumger,
        decimal DirectedOperationToolDiameter,
        IEnumerable<NcProgramCodeAttempt> RewritableCodeds,
        MaterialTypeAttempt Material,
        ReamerTypeAttempt Reamer,
        decimal Thickness,
        MainNcProgramParametersAttempt MainNcProgramParameters)
    {
        private RewriterSelectorAttempt GetRewriterSelection() => DirectedOperation switch
        {
            DirectedOperationTypeAttempt.Tapping => RewriterSelectorAttempt.Tapping,
            DirectedOperationTypeAttempt.Reaming => GetReamerRewriterSelection(DirectedOperation, Reamer),
            DirectedOperationTypeAttempt.Drilling => RewriterSelectorAttempt.Drilling,
            _ => throw new NotImplementedException(),
        };

        private static RewriterSelectorAttempt GetReamerRewriterSelection(DirectedOperationTypeAttempt directedOperation, ReamerTypeAttempt reamer)
        {
            if (directedOperation == DirectedOperationTypeAttempt.Reaming
                && reamer == ReamerTypeAttempt.Undefined)
                throw new InvalidOperationException($"指示が不整合です 作業指示: {directedOperation} リーマ: {reamer}");

            return reamer switch
            {
                ReamerTypeAttempt.Crystal => RewriterSelectorAttempt.CrystalReaming,
                ReamerTypeAttempt.Skill => RewriterSelectorAttempt.SkillReaming,
                _ => throw new NotImplementedException(),
            };
        }

        public RewriterSelectorAttempt RewriterSelector => GetRewriterSelection();
    }

    public class TestEditNcProgramPramFactory
    {
        public static EditNcProgramPram Create(
            DirectedOperationTypeAttempt directedOperation = DirectedOperationTypeAttempt.Drilling,
            string subProgramNumger = "8000",
            decimal directedOperationToolDiameter = 13.2m,
            IEnumerable<NcProgramCodeAttempt>? rewritableCodes = default,
            MaterialTypeAttempt material = MaterialTypeAttempt.Aluminum,
            ReamerTypeAttempt reamer = ReamerTypeAttempt.Crystal,
            decimal thickness = 15,
            MainNcProgramParametersAttempt? mainNcProgramParameters = default)
        {
            rewritableCodes ??= new List<NcProgramCodeAttempt>
            {
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.CenterDrilling,
                    ProgramName: "O1000",
                    new List<NcBlockAttempt>
                    {
                        TestNcBlockAttemptFactory.Create(),
                    }),
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.Drilling,
                    ProgramName: "O2000",
                    new List<NcBlockAttempt>
                    {
                        TestNcBlockAttemptFactory.Create(),
                    }),
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.Chamfering,
                    ProgramName: "O3000",
                    new List<NcBlockAttempt>
                    {
                        TestNcBlockAttemptFactory.Create(),
                    }),
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.Reaming,
                    ProgramName: "O4000",
                    new List<NcBlockAttempt>
                    {
                        TestNcBlockAttemptFactory.Create(),
                    }),
                new(Ulid.NewUlid().ToString(),
                    MainProgramTypeAttempt.Tapping,
                    ProgramName: "O5000",
                    new List<NcBlockAttempt>
                    {
                        TestNcBlockAttemptFactory.Create(),
                    }),
            };

            mainNcProgramParameters ??= TestMainNcProgramParametersPramFactory.Create();

            return new(directedOperation,
                       subProgramNumger,
                       directedOperationToolDiameter,
                       rewritableCodes,
                       material,
                       reamer,
                       thickness,
                       mainNcProgramParameters);
        }
    }

    public enum RewriterSelectorAttempt
    {
        Tapping,
        CrystalReaming,
        SkillReaming,
        Drilling,
    }

    public record class EditNcProgramDto(IEnumerable<NcProgramCodeAttempt> NcProgramCodes);
}
