using Microsoft.Extensions.Configuration;
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
            Mock<IConfiguration> configMock = new();
            configMock.Setup(x => x["applicationConfiguration:ListDirectory"])
                .Returns("リスト");
            configMock.Setup(x => x["applicationConfiguration:CrystalRemmerTable"])
                .Returns("クリスタルリーマー.xlsx");
            configMock.Setup(x => x["applicationConfiguration:SkillRemmerTable"])
                .Returns("スキルリーマー.xlsx");
            configMock.Setup(x => x["applicationConfiguration:TapTable"])
                .Returns("タップ.xlsx");
            configMock.Setup(x => x["applicationConfiguration:DrillTable"])
                .Returns("ドリル.xlsx");

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

            // when
            IReadMainNcProgramParametersUseCase useCase =
                new ReadMainNcProgramParametersUseCase(
                    configMock.Object,
                    mock_stream.Object,
                    mock_reamer.Object,
                    mock_tap.Object,
                    mock_drill.Object);
            var actual = await useCase.ExecuteAsync();

            // then
            Assert.IsInstanceOfType(actual.CrystalReamerParameters, typeof(IEnumerable<ReamingProgramPrameterAttempt>));
            Assert.IsInstanceOfType(actual.SkillReamerParameters, typeof(IEnumerable<ReamingProgramPrameterAttempt>));
            Assert.IsInstanceOfType(actual.TapParameters, typeof(IEnumerable<TappingProgramPrameterAttempt>));
            mock_stream.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(4));
            mock_reamer.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Exactly(2));
            mock_tap.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Once());
            mock_drill.Verify(x => x.ReadAllAsync(It.IsAny<Stream>()), Times.Once());
        }
    }
}