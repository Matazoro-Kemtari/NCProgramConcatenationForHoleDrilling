using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService;

namespace Wada.ReadSubNCProgramApplication.Tests
{
    [TestClass()]
    public class ReadSubNCProgramUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            Mock<IStreamReaderOpener> mock_reader = new();
            Mock<INCProgramRepository> mock_nc = new();

            // when
            IReadSubNCProgramUseCase readSubNCProgramUseCase = new ReadSubNCProgramUseCase(mock_reader.Object, mock_nc.Object);
            _ = await readSubNCProgramUseCase.ExecuteAsync(string.Empty);

            // then
            mock_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Once);
            mock_nc.Verify(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod()]
        public async Task 異常系_リポジトリで例外が起きた時例外を返すこと()
        {
            // given
            Mock<IStreamReaderOpener> mock_reader = new();
            Mock<INCProgramRepository> mock_nc = new();
            mock_nc.Setup(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<string>()))
                .Throws<ReadSubNCProgramApplicationException>();

            // when
            IReadSubNCProgramUseCase readSubNCProgramUseCase = new ReadSubNCProgramUseCase(mock_reader.Object, mock_nc.Object);
            async Task targetAsync()
            {
                _ = await readSubNCProgramUseCase.ExecuteAsync(string.Empty);
            }

            // then
            _ = await Assert.ThrowsExceptionAsync<ReadSubNCProgramApplicationException>(targetAsync);
            mock_nc.Verify(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<string>()), Times.Once);
        }
    }
}