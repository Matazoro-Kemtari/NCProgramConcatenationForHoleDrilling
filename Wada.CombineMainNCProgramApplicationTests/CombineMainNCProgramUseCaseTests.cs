using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService.MainProgramCombiner;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

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
            ICombineMainNCProgramUseCase useCase = new CombineMainNCProgramUseCase(mock_comviner.Object);

            List<NCProgramCode> combinableCodes = new()
            {
                TestNCProgramCodeFactory.Create(),
            };
            _ = await useCase.ExecuteAsync(combinableCodes);

            // then
            mock_comviner.Verify(x => x.Combine(It.IsAny<IEnumerable<NCProgramCode>>()), Times.Once);
        }
    }
}