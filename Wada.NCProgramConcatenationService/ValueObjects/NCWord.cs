using System.Text.RegularExpressions;

namespace Wada.NCProgramConcatenationService.ValueObjects
{
    public interface INCWord { }

    /// <summary>
    /// コメント
    /// </summary>
    /// <param name="Comment"></param>
    public record class NCComment(string Comment) : INCWord
    {
        public override string ToString() => $"({Comment})";
    }

    /// <summary>
    /// ワード
    /// </summary>
    /// <param name="Address"></param>
    /// <param name="ValueData"></param>
    public record class NCWord(Address Address, IValueData ValueData) : INCWord
    {
        public override string ToString() => Address.ToString() + ValueData.ToString();
    }

    /// <summary>
    /// アドレス
    /// </summary>
    public record class Address
    {
        public Address(char value)
        {
            if (!Regex.IsMatch(value.ToString(), @"^[a-zA-Z]$"))
                throw new NCProgramConcatenationServiceException(nameof(value));

            Value = value;
        }
        public override string ToString() => Value.ToString();

        public char Value { get; init; }
    }

    public interface IValueData
    {
        decimal Number();
    }

    /// <summary>
    /// 数値(座標以外)
    /// </summary>
    /// <param name="Value"></param>
    public record class NumericalValue : IValueData
    {
        public NumericalValue(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string buf;
            if (value.Contains('.'))
                buf = string.Concat(value, "0");
            else
                buf = value;

            if (!decimal.TryParse(buf, out _))
                throw new ArgumentOutOfRangeException(nameof(value));

            Value = value;
        }

        public decimal Number()
        {
            if (Value.Contains('.'))
            {
                return decimal.Parse(string.Concat(Value, "0"));
            }
            return decimal.Parse(Value);
        }

        public override string ToString() => Value;

        public string Value { get; init; }
    }

    /// <summary>
    /// 座標数値
    /// </summary>
    public record class CoordinateValue : IValueData
    {
        public CoordinateValue(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string buf;
            if (value.Contains('.'))
                buf = string.Concat(value, "0");
            else
                buf = value;

            if (!decimal.TryParse(buf, out _))
                throw new ArgumentOutOfRangeException(nameof(value));

            Value = value;
        }

        public decimal Number()
        {
            if (Value.Contains('.'))
                return decimal.Parse(string.Concat(Value, "0"));

            // 小数点がないと0.001の単位で解釈する
            decimal buf = decimal.Parse(Value);
            return buf / 1000m;
        }

        public override string ToString() => Value;

        public string Value { get; init; }
    }

    /// <summary>
    /// 変数
    /// </summary>
    /// <param name="VariableAddress"></param>
    /// <param name="ValueData"></param>
    public record class NCVariable(VariableAddress VariableAddress, IValueData ValueData) : INCWord
    {
        public override string ToString() => $"#{VariableAddress}={ValueData}";
    }

    /// <summary>
    /// 変数のアドレス
    /// </summary>
    /// <param name="Value"></param>
    public record class VariableAddress(uint Value)
    {
        public override string ToString() => Value.ToString();
    }
}
