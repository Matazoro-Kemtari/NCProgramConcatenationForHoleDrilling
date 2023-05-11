using Wada.MainProgramPrameterSpreadSheet;
using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

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
            TestDrillSizeDataFactory.Create("#60", 0.0399d, 1.01d),
            TestDrillSizeDataFactory.Create("#59", 0.043d, 1.09d),
            TestDrillSizeDataFactory.Create("#58", 0.0461d, 1.17d),
            TestDrillSizeDataFactory.Create("#57", 0.0491d, 1.25d),
            TestDrillSizeDataFactory.Create("#56", 0.0522d, 1.33d),
            TestDrillSizeDataFactory.Create("#55", 0.0553d, 1.4d),
            TestDrillSizeDataFactory.Create("#54", 0.0584d, 1.48d),
            TestDrillSizeDataFactory.Create("1/16", 0.0625d, 1.59d),
            TestDrillSizeDataFactory.Create("5/64", 0.0781d, 1.98d),
            TestDrillSizeDataFactory.Create("3/32", 0.0938d, 2.38d),
            TestDrillSizeDataFactory.Create("7/64", 0.1094d, 2.78d),
            TestDrillSizeDataFactory.Create("1/8", 0.1250d, 3.18d),
            TestDrillSizeDataFactory.Create("9/64", 0.1406d, 3.57d),
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