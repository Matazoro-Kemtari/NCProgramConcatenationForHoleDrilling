using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Wada.NcProgramConcatenationService.NcProgramAggregation;

namespace Wada.InchMillimeterConvertSpreadSheet.Tests
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
        public async Task 異常系_無効なストリームが与えられた場合ArgumentNullExceptionがスローされること()
        {
            // given
            using Stream? xlsStream = null;

            // when
            var reader = new DrillSizeDataReader();
            Task target() => reader.ReadAllAsync(xlsStream!);

            // then
            var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(target);
        }

        [TestMethod()]
        public async Task 異常系_空のストリームが与えられた場合FormatExceptionがスローされること()
        {
            // given
            using Stream xlsStream = new MemoryStream();

            // when
            var reader = new DrillSizeDataReader();
            Task target() => reader.ReadAllAsync(xlsStream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<FileFormatException>(target);
        }

        [DataTestMethod()]
        [DataRow("A2", "@1", "識別子")]
        [DataRow("B2", "-1", "Inches")]
        [DataRow("C2", "0", "ISO Metric drill size(㎜)")]
        public async Task 異常系_不正なドリルサイズデータが含まれるストリームが与えられた場合DrillSizeDataExceptionがスローされること(string address, string value, string item)
        {
            // given
            using var workbook = MakeTestBook();
            var sheet = workbook.Worksheets.First();
            var range = sheet.Range(address);
            range.SetValue(value);
            var rowNumber = range.RowCount() + 1;
            using Stream xlsStream = new MemoryStream();
            workbook.SaveAs(xlsStream);

            // when
            var reader = new DrillSizeDataReader();
            Task target() => reader.ReadAllAsync(xlsStream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<DrillSizeDataException>(target);
            var message = $"{item}の値が不正です 値: {value}, 行: {rowNumber}";
            Assert.AreEqual(message, ex.Message);
        }

        private static IEnumerable<DrillSizeData> TestDrillSizeDatas() => new List<DrillSizeData>
        {
            TestDrillSizeDataFactory.Create("#60", 0.0399m, 1.01m),
            TestDrillSizeDataFactory.Create("#59", 0.043m, 1.09m),
            TestDrillSizeDataFactory.Create("#58", 0.0461m, 1.17m),
            TestDrillSizeDataFactory.Create("#57", 0.0491m, 1.25m),
            TestDrillSizeDataFactory.Create("#56", 0.0522m, 1.33m),
            TestDrillSizeDataFactory.Create("#55", 0.0553m, 1.4m),
            TestDrillSizeDataFactory.Create("#54", 0.0584m, 1.48m),
            TestDrillSizeDataFactory.Create("1/16", 0.0625m, 1.59m),
            TestDrillSizeDataFactory.Create("5/64", 0.0781m, 1.98m),
            TestDrillSizeDataFactory.Create("3/32", 0.0938m, 2.38m),
            TestDrillSizeDataFactory.Create("7/64", 0.1094m, 2.78m),
            TestDrillSizeDataFactory.Create("1/8", 0.1250m, 3.18m),
            TestDrillSizeDataFactory.Create("9/64", 0.1406m, 3.57m),
            TestDrillSizeDataFactory.Create("#A", 0.234m, 5.94m),
            TestDrillSizeDataFactory.Create("#B", 0.238m, 6.05m),
            TestDrillSizeDataFactory.Create("#C", 0.242m, 6.15m),
            TestDrillSizeDataFactory.Create("#D", 0.246m, 6.25m),
            TestDrillSizeDataFactory.Create("#E", 0.25m, 6.35m),
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

            Dictionary<int, int> counts = new()
            {
                {0, 0 },
                {3, 0 },
                {6, 0 },
            };
            var testDatas = TestDrillSizeDatas();
            testDatas.ToList().ForEach(x =>
            {
                int offset;
                if (Regex.IsMatch(x.SizeIdentifier, @"#\d+"))
                    offset = 0;
                else if (Regex.IsMatch(x.SizeIdentifier, @"\d{1,2}/\d{1,2}"))
                    offset = 3;
                else
                    offset = 6;

                counts[offset]++;

                sht.Cell(counts[offset] + 1, 1 + offset).SetValue(x.SizeIdentifier);
                sht.Cell(counts[offset] + 1, 2 + offset).SetValue(x.Inch);
                sht.Cell(counts[offset] + 1, 3 + offset).SetValue(x.Millimeter);
            });
            return workbook;
        }
    }
}