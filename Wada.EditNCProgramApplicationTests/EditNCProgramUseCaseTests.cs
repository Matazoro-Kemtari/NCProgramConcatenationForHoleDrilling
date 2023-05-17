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
            Mock<CrystalReamingParameterRewriter> mock_crystal = new();
            Mock<SkillReamingParameterRewriter> mock_skill = new();
            Mock<TappingParameterRewriter> mock_tap = new();
            Mock<DrillingParameterRewriter> mock_drill = new();

            var editNcProgramPram = TestEditNcProgramPramFactory.Create(
                directedOperation: directedOperation,
                reamer: reamer);

            IEditNcProgramUseCase editNcProgramUseCase =
                 new EditNcProgramUseCase(
                     mock_crystal.Object,
                     mock_skill.Object,
                     mock_tap.Object,
                     mock_drill.Object);
            _ = await editNcProgramUseCase.ExecuteAsync(editNcProgramPram);

            // then
            mock_crystal.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolRecord>()),
                reamer == ReamerTypeAttempt.Crystal && directedOperation == DirectedOperationTypeAttempt.Reaming ? Times.Once() : Times.Never());
            mock_skill.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolRecord>()),
                reamer == ReamerTypeAttempt.Skill && directedOperation == DirectedOperationTypeAttempt.Reaming ? Times.Once() : Times.Never());
            mock_tap.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolRecord>()),
                directedOperation == DirectedOperationTypeAttempt.Tapping ? Times.Once() : Times.Never());
            mock_drill.Verify(x => x.RewriteByTool(It.IsAny<RewriteByToolRecord>()),
                directedOperation == DirectedOperationTypeAttempt.Drilling ? Times.Once() : Times.Never());
        }
    }
}