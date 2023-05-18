using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NcProgramConcatenationService.MainProgramCombiner;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.UseCase.DataClass;

namespace Wada.CombineMainNcProgramApplication.Tests
{
    [TestClass()]
    public class CombineMainNcProgramUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとドメインサービスが実行されること()
        {
            // given
            // when
            Mock<IMainProgramCombiner> mock_comviner = new();
            mock_comviner.Setup(x => x.Combine(It.IsAny<IEnumerable<NcProgramCode>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(TestNcProgramCodeFactory.Create());
            ICombineMainNcProgramUseCase useCase = new CombineMainNcProgramUseCase(mock_comviner.Object);

            List<NcProgramCodeAttempt> combinableCodesInUseCase = new()
            {
                TestNcProgramCodeAttemptFactory.Create(),
            };
            CombineMainNcProgramParam param = new(combinableCodesInUseCase, MachineToolTypeAttempt.RB250F, MaterialTypeAttempt.Aluminum);
            _ = await useCase.ExecuteAsync(param);

            // then
            mock_comviner.Verify(x => x.Combine(It.IsAny<IEnumerable<NcProgramCode>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
    }
}
