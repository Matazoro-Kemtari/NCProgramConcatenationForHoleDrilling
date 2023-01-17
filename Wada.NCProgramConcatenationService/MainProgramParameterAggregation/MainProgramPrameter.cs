using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.MainProgramParameterAggregation
{
    public interface IMainProgramPrameter
    {
        /// <summary>
        /// ツール径キー
        /// </summary>
        string DiameterKey { get; }

        /// <summary>
        /// ツール径
        /// </summary>
        double TargetToolDiameter { get; }

        /// <summary>
        /// C/D深さ
        /// </summary>
        double CenterDrillDepth { get; }

        /// <summary>
        /// 面取り深さ
        /// </summary>
        double? ChamferingDepth { get; }


        /// <summary>
        /// ドリル先端の長さ
        /// </summary>
        double DrillTipLength { get; }
    }

    /// <summary>
    /// リーマパラメータ
    /// </summary>
    /// <param name="DiameterKey">リーマ径</param>
    /// <param name="PreparedHoleDiameter">下穴1</param>
    /// <param name="SecondPreparedHoleDiameter">下穴2</param>
    /// <param name="CenterDrillDepth">C/D深さ</param>
    /// <param name="ChamferingDepth">面取り深さ</param>
    public record class ReamingProgramPrameter(
        string DiameterKey,
        double PreparedHoleDiameter,
        double SecondPreparedHoleDiameter,
        double CenterDrillDepth,
        double? ChamferingDepth) : IMainProgramPrameter
    {
        [Logging]
        private static double Validate(string value) => double.Parse(value);

        public double TargetToolDiameter => Validate(DiameterKey);

        public double DrillTipLength => 5d;

        /// <summary>
        /// 下穴1のドリル先端の長さ
        /// </summary>
        public DrillTipLength FastPreparedHoleDrillTipLength => new(PreparedHoleDiameter);

        /// <summary>
        /// 下穴2のドリル先端の長さ
        /// </summary>
        public DrillTipLength SecondPreparedHoleDrillTipLength => new(SecondPreparedHoleDiameter);
    }

    /// <summary>
    /// タップパラメータ
    /// </summary>
    /// <param name="DiameterKey">タップ径</param>
    /// <param name="PreparedHoleDiameter">下穴</param>
    /// <param name="CenterDrillDepth">C/D深さ</param>
    /// <param name="ChamferingDepth">面取り深さ</param>
    /// <param name="SpinForAluminum">回転(アルミ)</param>
    /// <param name="FeedForAluminum">送り(アルミ)</param>
    /// <param name="SpinForIron">回転(SS400)</param>
    /// <param name="FeedForIron">送り(SS400)</param>
    public record class TappingProgramPrameter(
        string DiameterKey,
        double PreparedHoleDiameter,
        double CenterDrillDepth,
        double? ChamferingDepth,
        double SpinForAluminum,
        double FeedForAluminum,
        double SpinForIron,
        double FeedForIron) : IMainProgramPrameter
    {
        [Logging]
        private static double Validate(string value)
        {
            var matchedDiameter = Regex.Match(value, @"(?<=M)\d+");
            if (!matchedDiameter.Success)
                throw new NCProgramConcatenationServiceException(
                    "タップ径の値が読み取れません\n" +
                    $"書式を確認してください タップ径: {value}");

            return double.Parse(matchedDiameter.Value);
        }

        public double TargetToolDiameter => Validate(DiameterKey);

        public double DrillTipLength => 5d;

        /// <summary>
        /// 下穴のドリル先端の長さ
        /// </summary>
        public DrillTipLength PreparedHoleDrillTipLength => new(PreparedHoleDiameter);
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
    public record class DrillingProgramPrameter(
        string DiameterKey,
        double CenterDrillDepth,
        double CutDepth,
        double SpinForAluminum,
        double FeedForAluminum,
        double SpinForIron,
        double FeedForIron) : IMainProgramPrameter
    {
        [Logging]
        private static double Validate(string value) => double.Parse(value);

        [Logging]
        private static double CalcChamferingDepth(double diameter) => -(diameter / 2d + 0.2d);

        public double TargetToolDiameter => Validate(DiameterKey);

        public double? ChamferingDepth => CalcChamferingDepth(TargetToolDiameter);

        public double DrillTipLength => new DrillTipLength(TargetToolDiameter).Value;
    }
}
