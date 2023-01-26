using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService.MainProgramCombiner;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.UseCase.DataClass;

namespace Wada.CombineMainNCProgramApplication.Tests
{
    [TestClass()]
    public class CombineMainNCProgramUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとドメインサービスが実行されること()
        {
            // given
            // when
            Mock<IMainProgramCombiner> mock_comviner = new();
            mock_comviner.Setup(x => x.Combine(It.IsAny<IEnumerable<NCProgramCode>>()))
                .Returns(TestNCProgramCodeFactory.Create());
            ICombineMainNCProgramUseCase useCase = new CombineMainNCProgramUseCase(mock_comviner.Object);

            List<NCProgramCodeAttempt> combinableCodesInUseCase = new()
            {
                TestNCProgramCodeAttemptFactory.Create(),
            };
            _ = await useCase.ExecuteAsync(combinableCodesInUseCase);

            // then
            mock_comviner.Verify(x => x.Combine(It.IsAny<IEnumerable<NCProgramCode>>()), Times.Once());
        }
    }
}