using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ParameterRewriter;

namespace Wada.EditNCProgramApplication.Tests
{
    [TestClass()]
    public class EditNCProgramUseCaseTests
    {
        [DataTestMethod()]
        [DataRow(DirectedOperationType.Tapping, ReamerType.Undefined)]
        [DataRow(DirectedOperationType.Reaming, ReamerType.Crystal)]
        [DataRow(DirectedOperationType.Reaming, ReamerType.Skill)]
        [DataRow(DirectedOperationType.Drilling, ReamerType.Undefined)]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること(DirectedOperationType directedOperation, ReamerType reamer)
        {
            // given
            // when
            Mock<IMainProgramParameterRewriter> mock_crystal = new();
            Mock<IMainProgramParameterRewriter> mock_skill = new();
            Mock<IMainProgramParameterRewriter> mock_tap = new();
            Mock<IMainProgramParameterRewriter> mock_drill = new();

            var editNCProgramPram = TestEditNCProgramPramFactory.Create(
                directedOperation: directedOperation);

            IEditNCProgramUseCase editNCProgramUseCase =
                 new EditNCProgramUseCase(
                     (CrystalReamingParameterRewriter)mock_crystal.Object,
                     (SkillReamingParameterRewriter)mock_skill.Object,
                     (TappingParameterRewriter)mock_tap.Object,
                     (DrillingParameterRewriter)mock_drill.Object);
            _ = await editNCProgramUseCase.ExecuteAsync(editNCProgramPram);

            // then
            mock_crystal.Verify(x => x.RewriteByTool(
                It.IsAny<Dictionary<NCProgramConcatenationService.ParameterRewriter.MainProgramType, NCProgramCode>>(),
                It.IsAny<NCProgramConcatenationService.ParameterRewriter.MaterialType>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<MainProgramParametersRecord>()),
                reamer == ReamerType.Crystal && directedOperation == DirectedOperationType.Reaming ? Times.Once() : Times.Never());
            mock_skill.Verify(x => x.RewriteByTool(
                It.IsAny<Dictionary<NCProgramConcatenationService.ParameterRewriter.MainProgramType, NCProgramCode>>(),
                It.IsAny<NCProgramConcatenationService.ParameterRewriter.MaterialType>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<MainProgramParametersRecord>()),
                reamer == ReamerType.Skill && directedOperation == DirectedOperationType.Reaming ? Times.Once() : Times.Never());
            mock_tap.Verify(x => x.RewriteByTool(
                It.IsAny<Dictionary<NCProgramConcatenationService.ParameterRewriter.MainProgramType, NCProgramCode>>(),
                It.IsAny<NCProgramConcatenationService.ParameterRewriter.MaterialType>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<MainProgramParametersRecord>()),
                directedOperation == DirectedOperationType.Tapping ? Times.Once() : Times.Never());
            mock_drill.Verify(x => x.RewriteByTool(
                It.IsAny<Dictionary<NCProgramConcatenationService.ParameterRewriter.MainProgramType, NCProgramCode>>(),
                It.IsAny<NCProgramConcatenationService.ParameterRewriter.MaterialType>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<MainProgramParametersRecord>()),
                directedOperation == DirectedOperationType.Drilling ? Times.Once() : Times.Never());
        }
    }
}