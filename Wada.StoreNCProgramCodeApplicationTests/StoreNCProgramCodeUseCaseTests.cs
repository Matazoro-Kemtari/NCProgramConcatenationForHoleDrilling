using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NcProgramConcatenationService;
using Wada.UseCase.DataClass;

namespace Wada.StoreNcProgramCodeApplication.Tests
{
    [TestClass()]
    public class StoreNcProgramCodeUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            // when
            Mock<IStreamWriterOpener> mock_writer = new();
            Mock<INcProgramRepository> mock_nc = new();

            IStoreNcProgramCodeUseCase useCase =
                new StoreNcProgramCodeUseCase(mock_writer.Object, mock_nc.Object);
            var path = "testfile";
            var ncProgram = TestNcProgramCodeAttemptFactory.Create();
            await useCase.ExecuteAsync(path, ncProgram);

            // then
            mock_writer.Verify(x => x.Open(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            mock_nc.Verify(x => x.WriteAllAsync(It.IsAny<StreamWriter>(), It.IsAny<string>()), Times.Once);
        }
    }
}