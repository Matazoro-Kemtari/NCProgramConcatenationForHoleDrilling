﻿using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.ReadSubNcProgramApplication.Tests
{
    [TestClass()]
    public class ReadSubNcProgramUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースを実行するとリポジトリが実行されること()
        {
            // given
            Mock<IConfiguration> configMock = new();
            configMock.Setup(x => x["applicationConfiguration:ListDirectory"])
                .Returns(@"..\リスト");
            configMock.Setup(x => x["applicationConfiguration:InchTable"])
                .Returns("インチ.xlsx");
            Mock<IStreamReaderOpener> mock_reader = new();
            Mock<INcProgramReadWriter> mock_nc = new();
            mock_nc.Setup(x => x.ReadSubProgramAll(It.IsAny<StreamReader>(), It.IsAny<string>()))
                .ReturnsAsync(TestNcProgramCodeFactory.Create(
                    ncBlocks: new List<NcBlock>
                    {
                        TestNcBlockFactory.Create(new List<INcWord> { new NcComment("3-M10") })
                    }));
            Mock<IStreamOpener> streamMock = new();
            Mock<IDrillSizeDataReader> drillSizeMock = new();

            // when
            var readSubNcProgramUseCase = new ReadSubNcProgramUseCase(configMock.Object,
                                                                      mock_reader.Object,
                                                                      mock_nc.Object,
                                                                      streamMock.Object,
                                                                      drillSizeMock.Object);
            _ = await readSubNcProgramUseCase.ExecuteAsync(string.Empty);

            // then
            mock_reader.Verify(x => x.Open(It.IsAny<string>()), Times.Once);
            mock_nc.Verify(x => x.ReadSubProgramAll(It.IsAny<StreamReader>(), It.IsAny<string>()), Times.Once);
        }
    }
}
