using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.ParameterRewriter;
using Wada.UseCase.DataClass;

namespace Wada.EditNCProgramApplication
{
    public interface IEditNCProgramUseCase
    {
        Task<EditNCProgramDTO> ExecuteAsync(EditNCProgramPram editNCProgramPram);
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
        public async Task<EditNCProgramDTO> ExecuteAsync(EditNCProgramPram editNCProgramPram)
        {
            // EditNCProgramPramのValidateで不整合状態は確認済み
            IMainProgramParameterRewriter rewriter = editNCProgramPram.DirectedOperation switch
            {
                DirectedOperationTypeAttempt.Tapping => _tappingParameterRewriter,
                DirectedOperationTypeAttempt.Reaming => editNCProgramPram.Reamer == ReamerTypeAttempt.Crystal ? _crystalReamingParameterRewriter : _skillReamingParameterRewriter,
                DirectedOperationTypeAttempt.Drilling => _drillingParameterRewriter,
                _ => throw new NotImplementedException(),
            };

            RewriteByToolRecord param = new(
                editNCProgramPram.RewritableCodeds.Select(x => x.Convert()),
                (MaterialType)editNCProgramPram.Material,
                editNCProgramPram.Thickness,
                editNCProgramPram.SubProgramNumger,
                editNCProgramPram.DirectedOperationToolDiameter,
                editNCProgramPram.MainNCProgramParameters.CrystalReamerParameters.Select(x => x.Convert()),
                editNCProgramPram.MainNCProgramParameters.SkillReamerParameters.Select(x => x.Convert()),
                editNCProgramPram.MainNCProgramParameters.TapParameters.Select(x => x.Convert()),
                editNCProgramPram.MainNCProgramParameters.DrillingPrameters.Select(x => x.Convert()));

            try
            {
                return await Task.Run(
                    () => new EditNCProgramDTO(rewriter.RewriteByTool(param)
                        .Select(x => NCProgramCodeAttempt.Parse(x))));
            }
            catch (NCProgramConcatenationServiceException ex)
            {
                throw new EditNCProgramApplicationException(ex.Message, ex);
            }
        }
    }

    public record class EditNCProgramPram(
        DirectedOperationTypeAttempt DirectedOperation,
        string SubProgramNumger,
        decimal DirectedOperationToolDiameter,
        IEnumerable<NCProgramCodeAttempt> RewritableCodeds,
        MaterialTypeAttempt Material,
        ReamerTypeAttempt Reamer,
        decimal Thickness,
        MainNCProgramParametersAttempt MainNCProgramParameters)
    {
        private static ReamerTypeAttempt Validate(DirectedOperationTypeAttempt directedOperation, ReamerTypeAttempt reamer)
        {
            if (directedOperation == DirectedOperationTypeAttempt.Reaming
                && reamer == ReamerTypeAttempt.Undefined)
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
            decimal directedOperationToolDiameter = 13.2m,
            IEnumerable<NCProgramCodeAttempt>? rewritableCodes = default,
            MaterialTypeAttempt material = MaterialTypeAttempt.Aluminum,
            ReamerTypeAttempt reamer = ReamerTypeAttempt.Crystal,
            decimal thickness = 15,
            MainNCProgramParametersAttempt? mainNCProgramParameters = default)
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
                       directedOperationToolDiameter,
                       rewritableCodes,
                       material,
                       reamer,
                       thickness,
                       mainNCProgramParameters);
        }
    }

    public record class EditNCProgramDTO(IEnumerable<NCProgramCodeAttempt> NCProgramCodes);
}
