﻿using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramParameterSpreadSheet.Tests
{
    [TestClass()]
    public class TappingParameterReaderTests
    {
        [TestMethod()]
        public async Task 正常系_タップパラメータエクセルが読み込めること()
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            using Stream xlsStream = new MemoryStream();
            workbook.SaveAs(xlsStream);

            // when
            IMainProgramParameterReader tappingParameterReader = new TappingParameterReader();
            IEnumerable<IMainProgramParameter> tappingProgramParameters = await tappingParameterReader.ReadAllAsync(xlsStream);

            // then
            Assert.AreEqual(1, tappingProgramParameters.Count());
            Assert.AreEqual(10, tappingProgramParameters.Select(x => x.DirectedOperationToolDiameter).First());
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public async Task 異常系_DR1に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 2).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramParameterReader tappingParameterReader = new TappingParameterReader();
            Task target() =>
                 tappingParameterReader.ReadAllAsync(stream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MainProgramParameterException>(target);
            string expected = $"DR1(φ)が取得できません" +
                $" シート: Sheet1," +
                $" セル: B2";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public async Task 異常系_CDに数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 3).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramParameterReader tappingParameterReader = new TappingParameterReader();
            Task target() =>
                 tappingParameterReader.ReadAllAsync(stream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MainProgramParameterException>(target);
            string expected = $"C/D深さが取得できません" +
                $" シート: Sheet1," +
                $" セル: C2";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public async Task 異常系_面取りに数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 4).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramParameterReader tappingParameterReader = new TappingParameterReader();
            Task target() =>
                 tappingParameterReader.ReadAllAsync(stream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MainProgramParameterException>(target);
            string expected = $"面取深さが取得できません" +
                $" シート: Sheet1," +
                $" セル: D2";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public async Task 異常系_回転ALに数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 5).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramParameterReader tappingParameterReader = new TappingParameterReader();
            Task target() =>
                 tappingParameterReader.ReadAllAsync(stream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MainProgramParameterException>(target);
            string expected = $"回転(AL)が取得できません" +
                $" シート: Sheet1," +
                $" セル: E2";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public async Task 異常系_送りALに数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 6).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramParameterReader tappingParameterReader = new TappingParameterReader();
            Task target() =>
                 tappingParameterReader.ReadAllAsync(stream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MainProgramParameterException>(target);
            string expected = $"送り(AL)が取得できません" +
                $" シート: Sheet1," +
                $" セル: F2";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public async Task 異常系_回転SS400に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 7).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramParameterReader tappingParameterReader = new TappingParameterReader();
            Task target() =>
                 tappingParameterReader.ReadAllAsync(stream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MainProgramParameterException>(target);
            string expected = $"回転(SS400)が取得できません" +
                $" シート: Sheet1," +
                $" セル: G2";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public async Task 異常系_送りSS400に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 8).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramParameterReader tappingParameterReader = new TappingParameterReader();
            Task target() =>
                 tappingParameterReader.ReadAllAsync(stream);

            // then
            var ex = await Assert.ThrowsExceptionAsync<MainProgramParameterException>(target);
            string expected = $"送り(SS400)が取得できません" +
                $" シート: Sheet1," +
                $" セル: H2";
            Assert.AreEqual(expected, ex.Message);
        }

        private static XLWorkbook MakeTestBook()
        {
            XLWorkbook workbook = new();
            var sht = workbook.AddWorksheet();
            sht.Cell(1, 1).SetValue("タップ径");
            sht.Cell(1, 2).SetValue("DR1(φ)");
            sht.Cell(1, 3).SetValue("C/D深さ");
            sht.Cell(1, 4).SetValue("面取深さ");
            sht.Cell(1, 5).SetValue("回転(AL)");
            sht.Cell(1, 6).SetValue("送り(AL)");
            sht.Cell(1, 7).SetValue("回転(SS400)");
            sht.Cell(1, 8).SetValue("送り(SS400");

            sht.Cell(2, 1).SetValue("M10*P1.5");
            sht.Cell(2, 2).SetValue(8.6);
            sht.Cell(2, 3).SetValue(-1.5);
            sht.Cell(2, 4).SetValue(-5.3);
            sht.Cell(2, 5).SetValue(200);
            sht.Cell(2, 6).SetValue(300);
            sht.Cell(2, 7).SetValue(140);
            sht.Cell(2, 8).SetValue(210);
            return workbook;
        }
    }
}