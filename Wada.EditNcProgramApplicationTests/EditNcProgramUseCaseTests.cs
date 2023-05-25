using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NcProgramConcatenationService.ParameterRewriter;
using Wada.UseCase.DataClass;

namespace Wada.EditNcProgramApplication.Tests
{
    [TestClass()]
    public class EditNcProgramUseCaseTests
    {
        [DataTestMethod()]
        [DataRow(DirectedOperationTypeAttempt.Tapping, ReamerTypeAttempt.Undefined)]
        [DataRow(DirectedOperationTypeAttempt.Reaming, ReamerTypeAttempt.Crystal)]
        [DataRow(DirectedOperationTypeAttempt.Reaming, ReamerTypeAttempt.Skill)]
        [DataRow(DirectedOperationTypeAttempt.Drilling, ReamerTypeAttempt.Undefined)]
        public async Task 正常系_ユースケースを実行するとドメインサービスが実行されること(
            DirectedOperationTypeAttempt directedOperation,
            ReamerTypeAttempt reamer)
        {
            // given
            // when
            Mock<CrystalReamingSequenceBuilder> mock_crystal = new();
            Mock<SkillReamingSequenceBuilder> mock_skill = new();
            Mock<TappingSequenceBuilder> mock_tap = new();
            Mock<DrillingSequenceBuilder> mock_drill = new();

            var editNcProgramParam = TestEditNcProgramParamFactory.Create(
                directedOperation: directedOperation,
                reamer: reamer);

            IEditNcProgramUseCase editNcProgramUseCase =
                 new EditNcProgramUseCase(
                     mock_crystal.Object,
                     mock_skill.Object,
                     mock_tap.Object,
                     mock_drill.Object);
            _ = await editNcProgramUseCase.ExecuteAsync(editNcProgramParam);

            // then
            mock_crystal.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolArg>()),
                reamer == ReamerTypeAttempt.Crystal && directedOperation == DirectedOperationTypeAttempt.Reaming ? Times.Once() : Times.Never());
            mock_skill.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolArg>()),
                reamer == ReamerTypeAttempt.Skill && directedOperation == DirectedOperationTypeAttempt.Reaming ? Times.Once() : Times.Never());
            mock_tap.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolArg>()),
                directedOperation == DirectedOperationTypeAttempt.Tapping ? Times.Once() : Times.Never());
            mock_drill.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolArg>()),
                directedOperation == DirectedOperationTypeAttempt.Drilling ? Times.Once() : Times.Never());
        }
    }
}