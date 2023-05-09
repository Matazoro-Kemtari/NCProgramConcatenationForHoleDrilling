using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet.Tests
{
    [TestClass()]
    public class DrillSizeDataReaderTests
    {
        [TestMethod()]
        public async Task 正常系_有効なストリームが与えられた場合正しいドリルサイズデータを取得できること()
        {
            // given
            using var workbook = MakeTestBook();
            using Stream xlsStream = new MemoryStream();
            workbook.SaveAs(xlsStream);

            // when
            var reader = new DrillSizeDataReader();
            var actual = await reader.ReadAllAsync(xlsStream);

            // then
            Assert.IsNotNull(actual);
            var expected = TestDrillSizeDatas();
            CollectionAssert.AreEquivalent(expected.ToArray(), actual.ToArray());
        }

        [TestMethod()]
        public void 異常系_無効なストリームが与えられた場合ArgumentNullExceptionがスローされること()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void 異常系_不正なドリルサイズデータが含まれるストリームが与えられた場合FormatExceptionがスローされること()
        {
            Assert.Fail();
        }

        private static IEnumerable<DrillSizeData> TestDrillSizeDatas() => new List<DrillSizeData>
        {
            TestDrillSizeDataFactory.Create("#60", 0.0399d, 1.01d),
            TestDrillSizeDataFactory.Create("#59", 0.043d, 1.09d),
            TestDrillSizeDataFactory.Create("#58", 0.0461d, 1.17d),
            TestDrillSizeDataFactory.Create("#57", 0.0491d, 1.25d),
            TestDrillSizeDataFactory.Create("#56", 0.0522d, 1.33d),
            TestDrillSizeDataFactory.Create("1/16", 0.0625d, 1.59d),
            TestDrillSizeDataFactory.Create("5/64", 0.0781d, 1.98d),
            TestDrillSizeDataFactory.Create("3/32", 0.0938d, 2.38d),
            TestDrillSizeDataFactory.Create("7/64", 0.1094d, 2.78d),
            TestDrillSizeDataFactory.Create("1/8", 0.1250d, 3.18d),
            TestDrillSizeDataFactory.Create("#A", 0.234d, 5.94d),
            TestDrillSizeDataFactory.Create("#B", 0.238d, 6.05d),
            TestDrillSizeDataFactory.Create("#C", 0.242d, 6.15d),
            TestDrillSizeDataFactory.Create("#D", 0.246d, 6.25d),
            TestDrillSizeDataFactory.Create("#E", 0.25d, 6.35d),
        };

        private static IXLWorkbook MakeTestBook()
        {
            XLWorkbook workbook = new();
            var sht = workbook.AddWorksheet();
            sht.Cell(1, 1).SetValue("ANSI Number(Gauge)");
            sht.Cell(1, 2).SetValue("Inches");
            sht.Cell(1, 3).SetValue("ISO Metric drill size(㎜)");
            sht.Cell(1, 4).SetValue("Fraction");
            sht.Cell(1, 5).SetValue("Inches");
            sht.Cell(1, 6).SetValue("ISO Metric drill size(㎜)");
            sht.Cell(1, 7).SetValue("Lettr size");
            sht.Cell(1, 8).SetValue("Inches");
            sht.Cell(1, 9).SetValue("ISO Metric drill size(㎜)");

            var testDatas = TestDrillSizeDatas();
            Enumerable.Range(0, 3).ToList().ForEach(coefficient =>
            {
                var offset = 3 * coefficient;
                testDatas.Skip(5 * coefficient).Take(5).Select((v, i) => (v, i)).ToList().ForEach(x =>
                {
                    sht.Cell(x.i + 2, 1 + offset).SetValue(x.v.SizeIdentifier);
                    sht.Cell(x.i + 2, 2 + offset).SetValue(x.v.Inch);
                    sht.Cell(x.i + 2, 3 + offset).SetValue(x.v.Millimeter);
                });
            });
            return workbook;
        }
    }
}