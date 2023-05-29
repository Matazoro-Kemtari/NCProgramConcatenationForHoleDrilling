using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation
{
    public interface IMainProgramParameter
    {
        /// <summary>
        /// ツール径キー
        /// </summary>
        string DiameterKey { get; }

        /// <summary>
        /// ツール径
        /// </summary>
        decimal DirectedOperationToolDiameter { get; }

        /// <summary>
        /// C/D深さ
        /// </summary>
        decimal CenterDrillDepth { get; }

        /// <summary>
        /// 面取り深さ
        /// </summary>
        decimal? ChamferingDepth { get; }


        /// <summary>
        /// ドリル先端の長さ
        /// </summary>
        decimal DrillTipLength { get; }
    }

    /// <summary>
    /// 下穴工程のあるパラメーター
    /// </summary>
    public interface IPilotHoleDrilledParameter : IMainProgramParameter
    {
        /// <summary>
        /// 下穴径
        /// </summary>
        decimal PilotHoleDiameter { get; }

        /// <summary>
        /// 下穴のドリル先端の長さ
        /// </summary>
        DrillTipLength PilotHoleDrillTipLength { get; }
    }

    /// <summary>
    /// リーマパラメータ
    /// </summary>
    /// <param name="DiameterKey">リーマー径</param>
    /// <param name="PilotHoleDiameter">下穴1径</param>
    /// <param name="SecondaryPilotHoleDiameter">下穴2径</param>
    /// <param name="CenterDrillDepth">C/D深さ</param>
    /// <param name="ChamferingDepth">面取り深さ</param>
    public record class ReamingProgramParameter(
        string DiameterKey,
        decimal PilotHoleDiameter,
        decimal SecondaryPilotHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth) : IPilotHoleDrilledParameter
    {
        [Logging]
        private static decimal Validate(string value) => decimal.Parse(value);

        public decimal DirectedOperationToolDiameter => Validate(DiameterKey);

        public decimal DrillTipLength => 5m;

        /// <summary>
        /// 下穴1のドリル先端の長さ
        /// </summary>
        public DrillTipLength PilotHoleDrillTipLength => new(PilotHoleDiameter);

        /// <summary>
        /// 下穴2のドリル先端の長さ
        /// </summary>
        public DrillTipLength SecondaryPilotHoleDrillTipLength => new(SecondaryPilotHoleDiameter);
    }

    public class TestReamingProgramParameterFactory
    {
        public static ReamingProgramParameter Create(
            string DiameterKey = "13.3",
            decimal PilotHoleDiameter = 9.1m,
            decimal SecondaryPilotHoleDiameter = 11.1m,
            decimal CenterDrillDepth = -3.1m,
            decimal? ChamferingDepth = -1.7m) => new(DiameterKey, PilotHoleDiameter, SecondaryPilotHoleDiameter, CenterDrillDepth, ChamferingDepth);
    }

    /// <summary>
    /// タップパラメータ
    /// </summary>
    /// <param name="DiameterKey">タップ径</param>
    /// <param name="PilotHoleDiameter">下穴径</param>
    /// <param name="CenterDrillDepth">C/D深さ</param>
    /// <param name="ChamferingDepth">面取り深さ</param>
    /// <param name="SpinForAluminum">回転(アルミ)</param>
    /// <param name="FeedForAluminum">送り(アルミ)</param>
    /// <param name="SpinForIron">回転(SS400)</param>
    /// <param name="FeedForIron">送り(SS400)</param>
    public record class TappingProgramParameter(
        string DiameterKey,
        decimal PilotHoleDiameter,
        decimal CenterDrillDepth,
        decimal? ChamferingDepth,
        int SpinForAluminum,
        int FeedForAluminum,
        int SpinForIron,
        int FeedForIron) : IPilotHoleDrilledParameter
    {
        [Logging]
        private static decimal Validate(string value)
        {
            var matchedDiameter = Regex.Match(value, @"(?<=M)\d+(\.\d)?");
            if (!matchedDiameter.Success)
                throw new DomainException(
                    "タップ径の値が読み取れません\n" +
                    $"書式を確認してください タップ径: {value}");

            return decimal.Parse(matchedDiameter.Value);
        }

        public decimal DirectedOperationToolDiameter => Validate(DiameterKey);

        public decimal DrillTipLength => 5m;

        /// <summary>
        /// 下穴のドリル先端の長さ
        /// </summary>
        public DrillTipLength PilotHoleDrillTipLength => new(PilotHoleDiameter);
    }

    public class TestTappingProgramParameterFactory
    {
        public static TappingProgramParameter Create(
            string DiameterKey = "M13.3",
            decimal PilotHoleDiameter = 11.1m,
            decimal CenterDrillDepth = -3.1m,
            decimal? ChamferingDepth = -1.7m,
            int SpinForAluminum = 700,
            int FeedForAluminum = 300,
            int SpinForIron = 700,
            int FeedForIron = 300) => new(DiameterKey, PilotHoleDiameter, CenterDrillDepth, ChamferingDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);
    }
    /// <summary>
    /// ドリルパラメータ
    /// </summary>
    /// <param name="DiameterKey">ドリル径</param>
    /// <param name="CenterDrillDepth">C/D深さ</param>
    /// <param name="CutDepth">切込量</param>
    /// <param name="SpinForAluminum">回転(アルミ)</param>
    /// <param name="FeedForAluminum">送り(アルミ)</param>
    /// <param name="SpinForIron">回転(SS400)</param>
    /// <param name="FeedForIron">送り(SS400)</param>
    public record class DrillingProgramParameter(
        string DiameterKey,
        decimal CenterDrillDepth,
        decimal CutDepth,
        int SpinForAluminum,
        int FeedForAluminum,
        int SpinForIron,
        int FeedForIron) : IMainProgramParameter
    {
        [Logging]
        private static decimal Validate(string value) => decimal.Parse(value);

        [Logging]
        private static decimal CalcChamferingDepth(decimal diameter) => -(diameter / 2m + 0.2m);

        public decimal DirectedOperationToolDiameter => Validate(DiameterKey);

        public decimal? ChamferingDepth => CalcChamferingDepth(DirectedOperationToolDiameter);

        public decimal DrillTipLength => new DrillTipLength(DirectedOperationToolDiameter).Value;
    }

    public class TestDrillingProgramParameterFactory
    {
        public static DrillingProgramParameter Create(
        string DiameterKey="13.3",
        decimal CenterDrillDepth = -1.5m,
        decimal CutDepth = 3.5m,
        int SpinForAluminum = 740,
        int FeedForAluminum = 100,
        int SpinForIron = 490,
        int FeedForIron = 70) => new(DiameterKey, CenterDrillDepth, CutDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);
    }
}
