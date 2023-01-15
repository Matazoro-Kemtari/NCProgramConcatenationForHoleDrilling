using Wada.AOP.Logging;

namespace Wada.NCProgramConcatenationService.ValueObjects
{
    public record class DrillTipLength
    {
        [Logging]
        public DrillTipLength(double diameter)
        {
            // 通し穴見込量(mm)
            const decimal throughHoleEstimatedQuantity = 1.5m;

            decimal _diameter = Convert.ToDecimal(diameter);
            decimal tangent = Convert.ToDecimal(Math.Tan(65));
            var degree = Math.Round(Math.Abs(_diameter / 2m / tangent), 1, MidpointRounding.AwayFromZero);
            degree -= degree % 0.5m;
            degree += throughHoleEstimatedQuantity;

            Value = (double)degree;
        }

        public double Value { get; init; }
    }
}
