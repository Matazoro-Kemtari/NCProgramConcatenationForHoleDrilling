using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet.Tests
{
    [TestClass()]
    public class DrillingParameterReaderTests
    {
        [TestMethod()]
        public void 正常系_ドリルパラメータエクセルが読み込めること()
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            using Stream xlsStream = new MemoryStream();
            workbook.SaveAs(xlsStream);

            // when
            IMainProgramPrameterReader drillingPrameterReader = new DrillingParameterReader();
            IEnumerable<IMainProgramPrameter> drillingProgramPrameters = drillingPrameterReader.ReadAll(xlsStream);

            // then
            Assert.AreEqual(1, drillingProgramPrameters.Count());
            Assert.AreEqual(10, drillingProgramPrameters.Select(x => x.DirectedOperationToolDiameter).First());
            Assert.AreEqual(-1.5m, drillingProgramPrameters.Select(x => x.CenterDrillDepth).First());
            Assert.AreEqual(-5.2m, drillingProgramPrameters.Select(x => x.ChamferingDepth).First());
            Assert.AreEqual(4.5m, drillingProgramPrameters.Select(x => x.DrillTipLength).First());
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public void 異常系_ドリル径に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 1).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterReader drillingPrameterReader = new DrillingParameterReader();
            void target() =>
                 drillingPrameterReader.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"DR(φ)が取得できません" +
                $" シート: Sheet1," +
                $" セル: A2";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public void 異常系_CD深さに数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 2).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterReader drillingPrameterReader = new DrillingParameterReader();
            void target() =>
                 drillingPrameterReader.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"C/D深さが取得できません" +
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
        public void 異常系_切込に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 5).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterReader drillingPrameterReader = new DrillingParameterReader();
            void target() =>
                 drillingPrameterReader.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"切込(Q)が取得できません" +
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
        public void 異常系_回転ALに数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 6).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterReader drillingPrameterReader = new DrillingParameterReader();
            void target() =>
                 drillingPrameterReader.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"回転(AL)が取得できません" +
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
        public void 異常系_送りALに数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 7).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterReader drillingPrameterReader = new DrillingParameterReader();
            void target() =>
                 drillingPrameterReader.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"送り(AL)が取得できません" +
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
        public void 異常系_回転SS400に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 8).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterReader drillingPrameterReader = new DrillingParameterReader();
            void target() =>
                 drillingPrameterReader.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"回転(SS400)が取得できません" +
                $" シート: Sheet1," +
                $" セル: H2";
            Assert.AreEqual(expected, ex.Message);
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public void 異常系_送りSS400に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 9).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterReader drillingPrameterReader = new DrillingParameterReader();
            void target() =>
                 drillingPrameterReader.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"送り(SS400)が取得できません" +
                $" シート: Sheet1," +
                $" セル: I2";
            Assert.AreEqual(expected, ex.Message);
        }

        private static XLWorkbook MakeTestBook()
        {
            XLWorkbook workbook = new();
            var sht = workbook.AddWorksheet();
            sht.Cell(1, 1).SetValue("DR(φ)");
            sht.Cell(1, 2).SetValue("C/D深さ");
            sht.Cell(1, 3).SetValue("面取深さ(工具径÷2+0.2)");
            sht.Cell(1, 4).SetValue("先端(PL)+見込み");
            sht.Cell(1, 5).SetValue("切込(Q)");
            sht.Cell(1, 6).SetValue("回転(AL)");
            sht.Cell(1, 7).SetValue("送り(AL)");
            sht.Cell(1, 8).SetValue("回転(SS400)");
            sht.Cell(1, 9).SetValue("送り(SS400)");

            sht.Cell(2, 1).SetValue(10);
            sht.Cell(2, 2).SetValue(-1.5);
            sht.Cell(2, 3).SetValue(-5.2);
            sht.Cell(2, 4).SetValue(4.5);
            sht.Cell(2, 5).SetValue(3);
            sht.Cell(2, 6).SetValue(960);
            sht.Cell(2, 7).SetValue(130);
            sht.Cell(2, 8).SetValue(640);
            sht.Cell(2, 9).SetValue(90);
            return workbook;
        }
    }
}