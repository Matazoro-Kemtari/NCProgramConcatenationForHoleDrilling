using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.ReadMainNcProgramApplication.Tests
{
    [TestClass()]
    public class ReadMainNcProgramUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            Mock<IStreamReaderOpener> mock_reader = new();
            Mock<INcProgramReadWriter> mock_nc = new();
            mock_nc.Setup(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<NcProgramType>(), It.IsAny<string>()))
                .ReturnsAsync(TestNCProgramCodeFactory.Create());

            // when
            IReadMainNcProgramUseCase readSubNCProgramUseCase = new ReadMainNcProgramUseCase(mock_reader.Object, mock_nc.Object);
            _ = await readSubNCProgramUseCase.ExecuteAsync();

            // then
            mock_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(15));
            mock_nc.Verify(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<NcProgramType>(), It.IsAny<string>()), Times.Exactly(15));
        }
    }
}