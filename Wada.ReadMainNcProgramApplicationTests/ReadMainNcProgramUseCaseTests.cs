using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
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
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "applicationConfiguration:MainNcProgramDirectory", @"..\メインプログラム"},
                { "applicationConfiguration:MachineNames:0", "RB250F" },
                { "applicationConfiguration:MachineNames:1", "RB260" },
                { "applicationConfiguration:MachineNames:2", "3軸立型" },
                { "applicationConfiguration:CenterDrillingProgramName", "CD.txt" },
                { "applicationConfiguration:DrillingProgramName", "DR.txt" },
                { "applicationConfiguration:ChamferingProgramName", "MENTORI.txt" },
                { "applicationConfiguration:ReamingProgramName", "REAMER.txt" },
                { "applicationConfiguration:TappingProgramName", "TAP.txt" },
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            Mock<IStreamReaderOpener> mock_reader = new();
            Mock<INcProgramReadWriter> mock_nc = new();
            mock_nc.Setup(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<NcProgramType>(), It.IsAny<string>()))
                .ReturnsAsync(TestNcProgramCodeFactory.Create());

            // when
            IReadMainNcProgramUseCase readSubNcProgramUseCase = new ReadMainNcProgramUseCase(configuration, mock_reader.Object, mock_nc.Object);
            _ = await readSubNcProgramUseCase.ExecuteAsync();

            // then
            mock_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Exactly(15));
            mock_nc.Verify(x => x.ReadAllAsync(It.IsAny<StreamReader>(), It.IsAny<NcProgramType>(), It.IsAny<string>()), Times.Exactly(15));
        }
    }
}
