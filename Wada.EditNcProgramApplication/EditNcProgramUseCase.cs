using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.ParameterRewriter;
using Wada.UseCase.DataClass;

namespace Wada.EditNcProgramApplication
{
    public interface IEditNcProgramUseCase
    {
        Task<EditNcProgramDto> ExecuteAsync(EditNcProgramParam NcProgramAggregation);
    }

    public class EditNcProgramUseCase : IEditNcProgramUseCase
    {
        private readonly IMainProgramSequenceBuilder _crystalReamingParameterRewriter;
        private readonly IMainProgramSequenceBuilder _skillReamingParameterRewriter;
        private readonly IMainProgramSequenceBuilder _tappingParameterRewriter;
        private readonly IMainProgramSequenceBuilder _drillingParameterRewriter;
        private readonly Dictionary<RewriterSelectorAttempt, IMainProgramSequenceBuilder> _rewriter;

        public EditNcProgramUseCase(
            CrystalReamingSequenceBuilder crystalReamingParameterRewriter,
            SkillReamingSequenceBuilder skillReamingParameterRewriter,
            TappingSequenceBuilder tappingParameterRewriter,
            DrillingSequenceBuilder drillingParameterRewriter)
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
        public async Task<EditNcProgramDto> ExecuteAsync(EditNcProgramParam editNcProgramParam)
        {
            var rewriteByToolRecord = editNcProgramParam.ToRewriteByToolRecord();

            try
            {
                return await Task.Run(
                    () => new EditNcProgramDto(
                        _rewriter[editNcProgramParam.RewriterSelector].RewriteByTool(rewriteByToolRecord)
                        .Select(x => NcProgramCodeAttempt.Parse(x))));
            }
            catch (DomainException ex)
            {
                throw new EditNcProgramUseCaseException(ex.Message, ex);
            }
        }
    }

    /// <summary>
    /// 引数用データクラス
    /// </summary>
    /// <param name="DirectedOperation">サブプログラム中の作業指示</param>
    /// <param name="SubProgramNumger">サブプログラム名</param>
    /// <param name="DirectedOperationToolDiameter">ツール径</param>
    /// <param name="RewritableCodeds">NCプログラム</param>
    /// <param name="Material">素材</param>
    /// <param name="Reamer">RewriterSelectorAttemptの判断用</param>
    /// <param name="Thickness">板厚</param>
    /// <param name="MainNcProgramParameters">パラメータ</param>
    public record class EditNcProgramParam(
        DirectedOperationTypeAttempt DirectedOperation,
        string SubProgramNumger,
        decimal DirectedOperationToolDiameter,
        IEnumerable<NcProgramCodeAttempt> RewritableCodeds,
        MaterialTypeAttempt Material,
        ReamerTypeAttempt Reamer,
        DrillingMethodAttempt HoleType,
        string BlindPilotHoleDepth,
        string BlindHoleDepth,
        decimal Thickness,
        MainNcProgramParametersAttempt MainNcProgramParameters)
    {
        internal RewriteByToolRecord ToRewriteByToolRecord() => new(
            RewritableCodeds.Select(x => x.Convert()),
            (MaterialType)Material,
            Thickness,
            SubProgramNumger,
            DirectedOperationToolDiameter,
            MainNcProgramParameters.CrystalReamerParameters.Select(x => x.Convert()),
            MainNcProgramParameters.SkillReamerParameters.Select(x => x.Convert()),
            MainNcProgramParameters.TapParameters.Select(x => x.Convert()),
            MainNcProgramParameters.DrillingParameters.Select(x => x.Convert()));

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

    public class TestEditNcProgramParamFactory
    {
        public static EditNcProgramParam Create(
            DirectedOperationTypeAttempt directedOperation = DirectedOperationTypeAttempt.Drilling,
            string subProgramNumger = "8000",
            decimal directedOperationToolDiameter = 13.2m,
            IEnumerable<NcProgramCodeAttempt>? rewritableCodes = default,
            MaterialTypeAttempt material = MaterialTypeAttempt.Aluminum,
            ReamerTypeAttempt reamer = ReamerTypeAttempt.Crystal,
            DrillingMethodAttempt holeType = DrillingMethodAttempt.ThroughHole,
            string blindPilotHoleDepth = "0",
            string blindHoleDepth = "0",
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

            mainNcProgramParameters ??= TestMainNcProgramParametersParamFactory.Create();

            return new(directedOperation,
                       subProgramNumger,
                       directedOperationToolDiameter,
                       rewritableCodes,
                       material,
                       reamer,
                       holeType,
                       blindPilotHoleDepth,
                       blindHoleDepth,
                       thickness,
                       mainNcProgramParameters);
        }
    }

    public enum ReamerTypeAttempt
    {
        Undefined,
        Crystal,
        Skill
    }

    public enum DrillingMethodAttempt
    {
        Undefined,
        // 通し穴
        ThroughHole,
        // 止まり穴
        BlindHole,
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
