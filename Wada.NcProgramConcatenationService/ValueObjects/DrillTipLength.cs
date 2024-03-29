﻿using Wada.AOP.Logging;

namespace Wada.NcProgramConcatenationService.ValueObjects
{
    public record class DrillTipLength
    {
        [Logging]
        public DrillTipLength(decimal diameter)
        {
            // 通し穴見込量(mm)
            const decimal throughHoleEstimatedQuantity = 1.5m;

            decimal _diameter = Convert.ToDecimal(diameter);
            decimal tangent = Convert.ToDecimal(Math.Tan(65));
            var degree = Math.Round(Math.Abs(_diameter / 2m / tangent), 1, MidpointRounding.AwayFromZero);
            degree -= degree % 0.5m;
            degree += throughHoleEstimatedQuantity;

            Value = (decimal)degree;
        }

        public decimal Value { get; init; }
    }
}
