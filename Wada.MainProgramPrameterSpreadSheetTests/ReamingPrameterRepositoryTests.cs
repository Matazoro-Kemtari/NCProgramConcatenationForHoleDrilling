using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet.Tests
{
    [TestClass()]
    public class ReamingPrameterRepositoryTests
    {
        [TestMethod()]
        public void 正常系_リーマパラメータエクセルが読み込めること()
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            using Stream xlsStream = new MemoryStream();
            workbook.SaveAs(xlsStream);

            // when
            IMainProgramPrameterRepository reamingPrameterRepository = new ReamingPrameterRepository();
            IEnumerable<IMainProgramPrameter> reamingProgramPrameters = reamingPrameterRepository.ReadAll(xlsStream);

            // then
            Assert.AreEqual(1, reamingProgramPrameters.Count());
        }

        [TestMethod()]
        public void 正常系_面取り深さの値が無でもパラメータエクセルが読み込めること()
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 5).SetValue("無");
            using Stream xlsStream = new MemoryStream();
            workbook.SaveAs(xlsStream);

            // when
            IMainProgramPrameterRepository reamingPrameterRepository = new ReamingPrameterRepository();
            IEnumerable<IMainProgramPrameter> reamingProgramPrameters = reamingPrameterRepository.ReadAll(xlsStream);

            // then
            Assert.AreEqual(1, reamingProgramPrameters.Count());
            Assert.IsNull(reamingProgramPrameters.Select(x => x.ChamferingDepth).First());
        }

        [DataTestMethod()]
        [DataRow("a")]
        [DataRow("A")]
        [DataRow("!")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("漢字")]
        public void 異常系_リーマ径に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 1).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterRepository reamingPrameterRepository = new ReamingPrameterRepository();
            void target() =>
                 reamingPrameterRepository.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"リーマ径が取得できません" +
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
        public void 異常系_DR1に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 2).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterRepository reamingPrameterRepository = new ReamingPrameterRepository();
            void target() =>
                 reamingPrameterRepository.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
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
        public void 異常系_DR2に数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 3).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterRepository reamingPrameterRepository = new ReamingPrameterRepository();
            void target() =>
                 reamingPrameterRepository.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"DR2(φ)が取得できません" +
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
        public void 異常系_CDに数値以外が入っているとき例外を返すこと(string? value)
        {
            // given
            using XLWorkbook workbook = MakeTestBook();
            workbook.Worksheets.First().Cell(2, 4).SetValue(value);
            using Stream stream = new MemoryStream();
            workbook.SaveAs(stream);

            // when
            IMainProgramPrameterRepository reamingPrameterRepository = new ReamingPrameterRepository();
            void target() =>
                 reamingPrameterRepository.ReadAll(stream);

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string expected = $"C/D深さが取得できません" +
                $" シート: Sheet1," +
                $" セル: D2";
            Assert.AreEqual(expected, ex.Message);
        }

        private static XLWorkbook MakeTestBook()
        {
            XLWorkbook workbook = new();
            var sht = workbook.AddWorksheet();
            sht.Cell(1, 1).SetValue("クリスタルリーマ径");
            sht.Cell(1, 2).SetValue("DR1(φ)");
            sht.Cell(1, 3).SetValue("DR2(φ)");
            sht.Cell(1, 4).SetValue("C/D深さ");
            sht.Cell(1, 5).SetValue("面取深さ");

            sht.Cell(2, 1).SetValue(4.76);
            sht.Cell(2, 2).SetValue(4.3);
            sht.Cell(2, 3).SetValue(6.15);
            sht.Cell(2, 4).SetValue(-0.5);
            sht.Cell(2, 5).SetValue(-3.1);

            return workbook;
        }
    }
}