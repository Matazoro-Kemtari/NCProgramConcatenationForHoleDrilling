using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wada.NCProgramConcatenationService.ValueObjects.Tests
{
    [TestClass()]
    public class TestNumericalValueTests
    {
        [DataTestMethod]
        [DataRow("0")]
        [DataRow("1")]
        [DataRow("-1")]
        [DataRow("0.")]
        [DataRow("1.")]
        [DataRow("-1.")]
        [DataRow("0.0")]
        [DataRow("1.0")]
        [DataRow("-1.0")]
        public void 正常系_オブジェクト生成できること(string value)
        {
            // given
            // when
            IValueData valueData = new NumericalValue(value);

            // then
            Assert.AreEqual(value, valueData.Value);
            decimal expected;
            if (value.Contains('.'))
                expected = decimal.Parse(string.Concat(value, "0"));
            else
                expected = decimal.Parse(value);
            Assert.AreEqual(expected, valueData.Number);
            Assert.IsFalse(valueData.Indefinite);
        }

        [TestMethod]
        public void 正常系_不定オブジェクト生成できること()
        {
            // given
            // when
            string value = "*";
            IValueData valueData = new NumericalValue(value);

            // then
            Assert.AreEqual(value, valueData.Value);
            Assert.AreEqual(0m, valueData.Number);
            Assert.IsTrue(valueData.Indefinite);
        }

        [DataTestMethod]
        [DataRow("*.")]
        [DataRow("*.*")]
        [DataRow(".*")]
        [DataRow("0.*")]
        [DataRow("*.0")]
        [DataRow("0*")]
        [DataRow("*0")]
        public void 異常系_アスタリスク以外の文字の場合例外を返すこと(string value)
        {
            // given
            // when
            void target()
            {
                _ = new NumericalValue(value);
            }

            // then
            var ex = Assert.ThrowsException<ArgumentException>(target);
            string msg = "アスタリスク以外の文字が含まれている";
            Assert.IsTrue(ex.Message.Contains(msg));
        }

        [DataTestMethod]
        [DataRow("%")]
        [DataRow("\\")]
        public void 異常系_アスタリスク以外の記号が含まれている場合例外を返すこと(string value)
        {
            // given
            // when
            void target()
            {
                _ = new NumericalValue(value);
            }

            // then
            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(target);
        }
    }
}