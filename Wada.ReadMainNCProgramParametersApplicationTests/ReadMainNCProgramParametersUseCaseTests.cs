using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.MainProgramPrameterSpreadSheet;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.UseCase.DataClass;

namespace Wada.ReadMainNcProgramParametersApplication.Tests
{
    [TestClass()]
    public class ReadMainNcProgramParametersUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            Mock<IStreamOpener> mock_stream = new();

            Mock<ReamingPrameterReader> mock_reamer = new();
            mock_reamer.Setup(x => x.ReadAllAsync(It.IsAny<Stream>()))
                .ReturnsAsync(new List<ReamingProgramPrameter>());

            Mock<TappingPrameterReader> mock_tap = new();
            mock_tap.Setup(x => x.ReadAllAsync(It.IsAny<Stream>()))
                .ReturnsAsync(new List<TappingProgramPrameter>());

            Mock<DrillingParameterReader> mock_drill = new();
            mock_drill.Setup(x => x.ReadAllAsync(It.IsAny<Stream>()))
                .ReturnsAsync(new List<DrillingProgramPrameter>());

            Mock<IDrillSizeDataReader> drillSizeReaderMock = new();

            // when
            IReadMainNcProgramParametersUseCase useCase =
                new ReadMainNcProgramParametersUseCase(
                    mock_stream.Object,
                    mock_reamer.Object,
                    mock_tap.Object,
                    mock_drill.Object,
                    drillSizeReaderMock.Object);
            var actual = await useCase.ExecuteAsync();

            // then
            Assert.IsInstanceOfType(actual.CrystalReamerParameters, typeof(IEnumerable<ReamingProgramPrameterAttempt>));
            Assert.IsInstanceOfType(actual.SkillReamerParameters, typeof(IEnumerable<ReamingProgramPrameterAttempt>));
            Assert.IsInstanceOfType(actual.TapParameters, typeof(IEnumerable<TappingProgramPrameterAttempt>));
            mock_stream.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(5));
            mock_reamer.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Exactly(2));
            mock_tap.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Once());
            mock_drill.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Once());
        }
    }
}