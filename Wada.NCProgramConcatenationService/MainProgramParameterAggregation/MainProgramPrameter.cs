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
        /// 下穴のドリル先端の長さ
        /// </summary>
        DrillTipLength PreparedHoleDrillTipLength { get; }
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

        public double TargetToolDiameter { get; } = Validate(DiameterKey);

        public DrillTipLength PreparedHoleDrillTipLength { get; } = new(PreparedHoleDiameter);
        public DrillTipLength SecondPreparedHoleDrillTipLength { get; } = new(SecondPreparedHoleDiameter);
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

        public double TargetToolDiameter { get; } = Validate(DiameterKey);

        public DrillTipLength PreparedHoleDrillTipLength { get; } = new(PreparedHoleDiameter);
    }

    public record class DrillingProgramPrameter(
        string DiameterKey,
        double CenterDrillDepth,
        double CutDepthForAluminum,
        double SpinForAluminum,
        double FeedForAluminum,
        double CutDepthForIron,
        double SpinForIron,
        double FeedForIron) : IMainProgramPrameter
    {
        public double TargetToolDiameter => throw new NotImplementedException();

        public double? ChamferingDepth => throw new NotImplementedException();

        public DrillTipLength PreparedHoleDrillTipLength => throw new NotImplementedException();
    }
}
