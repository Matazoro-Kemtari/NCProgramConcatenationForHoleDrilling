using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.ReadMainNCProgramApplication.Tests
{
    [TestClass()]
    public class ReadMainNCProgramUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            Mock<IStreamReaderOpener> mock_reader = new();
            Mock<INCProgramRepository> mock_nc = new();
            mock_nc.Setup(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<NCProgramType>(), It.IsAny<string>()))
                .ReturnsAsync(TestNCProgramCodeFactory.Create());

            // when
            IReadMainNCProgramUseCase readSubNCProgramUseCase = new ReadMainNCProgramUseCase(mock_reader.Object, mock_nc.Object);
            _ = await readSubNCProgramUseCase.ExecuteAsync();

            // then
            mock_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(15));
            mock_nc.Verify(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<NCProgramType>(), It.IsAny<string>()), Times.Exactly(15));
        }
    }
}