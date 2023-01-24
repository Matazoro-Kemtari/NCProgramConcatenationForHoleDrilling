using System.Text.RegularExpressions;
using Wada.AOP.Logging;

namespace Wada.NCProgramConcatenationService.ValueObjects
{
    /// <summary>
    /// オプショナルブロックスキップ
    /// </summary>
    public enum OptionalBlockSkip
    {
        None,
        BDT1,
        BDT2,
        BDT3,
        BDT4,
        BDT5,
        BDT6,
        BDT7,
        BDT8,
        BDT9,
    }

    public interface INCWord { }

    /// <summary>
    /// コメント
    /// </summary>
    /// <param name="Comment"></param>
    public record class NCComment(string Comment) : INCWord
    {
        public override string ToString() => $"({Comment})";
    }

    public class TestNCCommentFactory
    {
        public static NCComment Create(string comment = "COMMENT") => new(comment);
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

    public class TestNCWordFactory
    {
        public static NCWord Create(Address? address = default, IValueData? valueData = default)
        {
            address ??= TestAddressFactory.Create();
            valueData ??= TestCoordinateValueFactory.Create();
            return new(address, valueData);
        }
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

    public class TestAddressFactory
    {
        public static Address Create(char value = 'G') => new(value);
    }

    public interface IValueData
    {
        decimal Number { get; }
        string Value { get; }
        bool Indefinite { get; }
    }

    /// <summary>
    /// 数値(座標以外)
    /// </summary>
    /// <param name="Value"></param>
    public record class NumericalValue(string Value) : IValueData
    {
        public override string ToString() => Value;

        [Logging]
        private static string Validate(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Contains('*') && Regex.IsMatch(value, @"[^*]"))
                // アスタリスクに混ざり物がある
                throw new ArgumentException("アスタリスク以外の文字が含まれている", nameof(value));

            if (!value.Contains('*'))
            {
                string buf;
                if (value.Contains('.'))
                    buf = string.Concat(value, "0");
                else
                    buf = value;

                if (!decimal.TryParse(buf, out _))
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            return value;
        }

        [Logging]
        private static decimal ConvertNumber(string value)
        {
            // アスタリスクの場合の処理
            if (value.Contains('*'))
                return 0m;

            if (value.Contains('.'))
            {
                return decimal.Parse(string.Concat(value, "0"));
            }
            return decimal.Parse(value);
        }

        public decimal Number => ConvertNumber(Value);

        public string Value { get; init; } = Validate(Value);

        public bool Indefinite => Value.Contains('*');
    }

    public class TestNumericalValueFactory
    {
        public static NumericalValue Create(string value = "8000") => new(value);
    }

    /// <summary>
    /// 座標数値
    /// </summary>
    /// /// <param name="Value"></param>
    public record class CoordinateValue(string Value) : IValueData
    {
        public override string ToString() => Value;

        [Logging]
        private static string Validate(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Contains('*') && Regex.IsMatch(value, @"[^*]"))
                // アスタリスクに混ざり物がある
                throw new ArgumentException("アスタリスク以外の文字が含まれている", nameof(value));

            if (!value.Contains('*'))
            {
                string buf;
                if (value.Contains('.'))
                    buf = string.Concat(value, "0");
                else
                    buf = value;

                if (!decimal.TryParse(buf, out _))
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            return value;
        }

        [Logging]
        private static decimal ConvertNumber(string value)
        {
            // アスタリスクの場合の処理
            if (value.Contains('*'))
                return 0m;

            if (value.Contains('.'))
                return decimal.Parse(string.Concat(value, "0"));

            // 小数点がないと0.001の単位で解釈する
            decimal buf = decimal.Parse(value);
            return buf / 1000m;
        }

        public decimal Number => ConvertNumber(Value);

        public string Value { get; init; } = Validate(Value);

        public bool Indefinite => Value.Contains('*');
    }

    public class TestCoordinateValueFactory
    {
        public static CoordinateValue Create(string value = "8.") => new(value);
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

    public class TestNCVariableFactory
    {
        public static NCVariable Create(
            VariableAddress? variableAddress = default,
            IValueData? valueData = default)
        {
            variableAddress ??= TestVariableAddressFactory.Create();
            valueData ??= TestCoordinateValueFactory.Create();
            return new(variableAddress, valueData);
        }
    }

    /// <summary>
    /// 変数のアドレス
    /// </summary>
    /// <param name="Value"></param>
    public record class VariableAddress(uint Value)
    {
        public override string ToString() => Value.ToString();
    }

    public class TestVariableAddressFactory
    {
        public static VariableAddress Create(uint value = 1) => new(value);
    }

    public enum NCProgramType
    {
        /// <summary>
        /// センタードリル
        /// </summary>
        CenterDrilling,
        /// <summary>
        /// ドリル
        /// </summary>
        Drilling,
        /// <summary>
        /// 面取り
        /// </summary>
        Chamfering,
        /// <summary>
        /// リーマ
        /// </summary>
        Reaming,
        /// <summary>
        /// タップ
        /// </summary>
        Tapping,

        /// <summary>
        /// サブプログラム
        /// </summary>
        SubProgram = int.MaxValue,
    }
}
