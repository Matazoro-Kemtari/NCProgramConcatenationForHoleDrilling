using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService;

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

            // when
            IReadMainNCProgramUseCase readSubNCProgramUseCase = new ReadMainNCProgramUseCase(mock_reader.Object, mock_nc.Object);
            _ = await readSubNCProgramUseCase.ExecuteAsync(string.Empty);

            // then
            mock_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Once);
            mock_nc.Verify(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<string>()), Times.Once);
        }
    }
}