using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.MainProgramPrameterSpreadSheet;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.UseCase.DataClass;

namespace Wada.ReadMainNCProgramParametersApplication.Tests
{
    [TestClass()]
    public class ReadMainNCProgramParametersUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            Mock<IStreamOpener> mock_stream = new();
            Mock<ReamingPrameterRepository> mock_reamer = new();
            mock_reamer.Setup(x => x.ReadAll(It.IsAny<Stream>()))
                .Returns(new List<ReamingProgramPrameter>());
            Mock<TappingPrameterRepository> mock_tap = new();
            mock_tap.Setup(x => x.ReadAll(It.IsAny<Stream>()))
                .Returns(new List<TappingProgramPrameter>());
            Mock<DrillingParameterRepositoy> mock_drill=new();
            mock_drill.Setup(x => x.ReadAll(It.IsAny<Stream>()))
                .Returns(new List<DrillingProgramPrameter>());

            // when
            IReadMainNCProgramParametersUseCase useCase =
                new ReadMainNCProgramParametersUseCase(
                    mock_stream.Object,
                    mock_reamer.Object,
                    mock_tap.Object,
                    mock_drill.Object);
            var actual = await useCase.ExecuteAsync();

            // then
            Assert.IsInstanceOfType(actual.CrystalReamerParameters, typeof(IEnumerable<ReamingProgramPrameterAttempt>));
            Assert.IsInstanceOfType(actual.SkillReamerParameters,typeof(IEnumerable<ReamingProgramPrameterAttempt>));
            Assert.IsInstanceOfType(actual.TapParameters, typeof(IEnumerable<TappingProgramPrameterAttempt>));
            mock_stream.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(4));
            mock_reamer.Verify(x => x.ReadAll(It.IsAny<Stream>()), Times.Exactly(2));
            mock_tap.Verify(x => x.ReadAll(It.IsAny<Stream>()), Times.Once());
            mock_drill.Verify(x => x.ReadAll(It.IsAny<Stream>()), Times.Once());
        }
    }
}