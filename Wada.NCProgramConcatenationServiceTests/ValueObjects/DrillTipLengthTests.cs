﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wada.NCProgramConcatenationService.ValueObjects.Tests
{
    [TestClass()]
    public class DrillTipLengthTests
    {
        [DataTestMethod()]
        [DataRow(2, 2)]
        [DataRow(2.5, 2)]
        [DataRow(3, 2.5)]
        [DataRow(3.5, 2.5)]
        [DataRow(4, 2.5)]
        [DataRow(4.5, 3)]
        [DataRow(5, 3)]
        public void 正常系_先端長さが計算できること(decimal diameter, decimal expected)
        {
            // given
            // when
            DrillTipLength drillTipLength = new(diameter);

            // then
            Assert.AreEqual(expected, drillTipLength.Value);
        }
    }
}