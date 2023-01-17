using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService;

namespace Wada.EditNCProgramApplication.Tests
{
    [TestClass()]
    [Ignore]
    public class EditNCProgramUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            Mock<IMainProgramParameterRewriter> mock_rewriter = new();

            // when
            IEditNCProgramUseCase editNCProgramUseCase =
                new EditNCProgramUseCase(
                    mock_rewriter.Object);
            EditNCProgramPram editNCProgramPram = TestEditNCProgramPramFactory.Create();
            _ = await editNCProgramUseCase.ExecuteAsync(editNCProgramPram);

            // then
            //mock_rewriter.Verify(x => x.RewriteProgramParameter(), Times.Exactly(editNCProgramPram.MainProgramCodes.Count));
        }
    }
}