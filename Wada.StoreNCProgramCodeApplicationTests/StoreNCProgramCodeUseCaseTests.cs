using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.UseCase.DataClass;

namespace Wada.StoreNCProgramCodeApplication.Tests
{
    [TestClass()]
    public class StoreNCProgramCodeUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            // when
            Mock<IStreamWriterOpener> mock_writer = new();
            Mock<INCProgramRepository> mock_nc = new();

            IStoreNCProgramCodeUseCase useCase =
                new StoreNCProgramCodeUseCase(mock_writer.Object, mock_nc.Object);
            var path = "testfile";
            var ncProgram = TestNCProgramCodeAttemptFactory.Create();
            await useCase.ExecuteAsync(path, ncProgram);

            // then
            mock_writer.Verify(x => x.Open(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            mock_nc.Verify(x => x.WriteAllAsync(It.IsAny<StreamWriter>(), It.IsAny<NCProgramCode>()), Times.Once);
        }
    }
}