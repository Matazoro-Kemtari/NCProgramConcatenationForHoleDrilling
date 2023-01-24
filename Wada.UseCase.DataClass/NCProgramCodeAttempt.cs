using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.UseCase.DataClass
{
    public enum MachineToolTypeAttempt
    {
        //Undefined, 使わない
        RB250F = 1,
        RB260,
        Triaxial,
    }

    public record class NCProgramCodeAttempt(
        string ID,
        MainProgramTypeAttempt MainProgramClassification,
        string ProgramName,
        IEnumerable<NCBlockAttempt?> NCBlocks)
    {
        public static NCProgramCodeAttempt Parse(NCProgramCode ncProgramCode) => new(
            ncProgramCode.ID.ToString(),
            (MainProgramTypeAttempt)ncProgramCode.MainProgramClassification,
            ncProgramCode.ProgramName,
            ncProgramCode.NCBlocks.Select(x => x == null ? null : NCBlockAttempt.Parse(x)));

        public NCProgramCode Convert() => NCProgramCode.ReConstruct(ID, (NCProgramType)MainProgramClassification, ProgramName, NCBlocks.Select(x => x?.Convert()));
    }

    public enum MainProgramTypeAttempt
    {
        CenterDrilling,
        Drilling,
        Chamfering,
        Reaming,
        Tapping,
    }

    /// <summary>
    /// オプショナルブロックスキップ
    /// </summary>
    public enum OptionalBlockSkipAttempt
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

    public record class NCBlockAttempt(IEnumerable<INCWordAttempt> NCWords, OptionalBlockSkipAttempt HasBlockSkip)
    {
        public NCBlock? Convert() => new(NCWords.Select(x => x.Convert()), (OptionalBlockSkip)HasBlockSkip);

        public static NCBlockAttempt Parse(NCBlock ncBlock)
        {
            IEnumerable<INCWordAttempt> ncWords = ncBlock.NCWords
                .Select(x =>
                {
                    INCWordAttempt ncWordAttempt;
                    if (x.GetType() == typeof(NCComment))
                        ncWordAttempt = NCCommentAttempt.Parse((NCComment)x);
                    else if (x.GetType() == typeof(NCWord))
                        ncWordAttempt = NCWordAttempt.Parse((NCWord)x);
                    else if (x.GetType() == typeof(NCVariable))
                        ncWordAttempt = NCVariableAttempt.Parse((NCVariable)x);
                    else
                        throw new NotImplementedException();

                    return ncWordAttempt;
                });
            return new(ncWords, (OptionalBlockSkipAttempt)ncBlock.HasBlockSkip);
        }
    }

    public class TestNCBlockAttemptFactory
    {
        public static NCBlockAttempt Create(IEnumerable<INCWordAttempt>? ncWords = default)
        {
            ncWords ??= new List<INCWordAttempt>
            {
                TestNCWordAttemptFactory.Create(
                    address: TestAddressAttemptFactory.Create('G'),
                    valueData: TestNumericalValueAttemptFactory.Create("98")),
                TestNCWordAttemptFactory.Create(
                    address: TestAddressAttemptFactory.Create('G'),
                    valueData: TestNumericalValueAttemptFactory.Create("82")),
                TestNCWordAttemptFactory.Create(
                    address: TestAddressAttemptFactory.Create('R'),
                    valueData: TestCoordinateValueAttemptFactory.Create("3")),
                TestNCWordAttemptFactory.Create(
                    address: TestAddressAttemptFactory.Create('Z'),
                    valueData: TestCoordinateValueAttemptFactory.Create("*")),
                TestNCWordAttemptFactory.Create(
                    address: TestAddressAttemptFactory.Create('P'),
                    valueData: TestCoordinateValueAttemptFactory.Create("*")),
                TestNCWordAttemptFactory.Create(
                    address: TestAddressAttemptFactory.Create('Q'),
                    valueData: TestCoordinateValueAttemptFactory.Create("*")),
                TestNCWordAttemptFactory.Create(
                    address: TestAddressAttemptFactory.Create('F'),
                    valueData: TestNumericalValueAttemptFactory.Create("*")),
                TestNCWordAttemptFactory.Create(
                    address: TestAddressAttemptFactory.Create('L'),
                    valueData: TestNumericalValueAttemptFactory.Create("0")),
            };
            return new(ncWords, OptionalBlockSkipAttempt.None);
        }
    }

    public interface INCWordAttempt
    {
        INCWord Convert();
    }

    /// <summary>
    /// コメント
    /// </summary>
    /// <param name="Comment"></param>
    public record class NCCommentAttempt(string Comment) : INCWordAttempt
    {
        public static NCCommentAttempt Parse(NCComment ncComment) => new(ncComment.Comment);

        public INCWord Convert() => new NCComment(Comment);
    }

    /// <summary>
    /// ワード
    /// </summary>
    /// <param name="Address"></param>
    /// <param name="ValueData"></param>
    public record class NCWordAttempt(AddressAttempt Address, IValueDataAttempt ValueData) : INCWordAttempt
    {
        public static NCWordAttempt Parse(NCWord ncWord)
        {
            IValueDataAttempt valueDataAttempt;
            if (ncWord.ValueData.GetType() == typeof(NumericalValue))
                valueDataAttempt = NumericalValueAttempt.Parse((NumericalValue)ncWord.ValueData);
            else if (ncWord.ValueData.GetType() == typeof(CoordinateValue))
                valueDataAttempt = CoordinateValueAttempt.Parse((CoordinateValue)ncWord.ValueData);
            else
                throw new NotImplementedException();

            return new(AddressAttempt.Parse(ncWord.Address), valueDataAttempt);
        }

        public INCWord Convert()
        => new NCWord(Address.Convert(), ValueData.Convert());
    }

    public class TestNCWordAttemptFactory
    {
        public static NCWordAttempt Create(AddressAttempt? address = default, IValueDataAttempt? valueData = default)
        {
            address ??= TestAddressAttemptFactory.Create();
            valueData ??= TestCoordinateValueAttemptFactory.Create();
            return new(address, valueData);
        }
    }

    /// <summary>
    /// アドレス
    /// </summary>
    public record class AddressAttempt(char Value)
    {
        public static AddressAttempt Parse(Address address) => new AddressAttempt(address.Value);

        internal Address Convert() => new Address(Value);
    }

    public class TestAddressAttemptFactory
    {
        public static AddressAttempt Create(char value = 'G') => new(value);
    }

    public interface IValueDataAttempt
    {
        decimal Number { get; }
        string Value { get; }
        bool Indefinite { get; }

        IValueData Convert();
    }

    /// <summary>
    /// 数値(座標以外)
    /// </summary>
    /// <param name="Value"></param>
    public record class NumericalValueAttempt(string Value) : IValueDataAttempt
    {
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

        public IValueData Convert() => new NumericalValue(Value);

        public static NumericalValueAttempt Parse(NumericalValue valueData) => new(valueData.Value);

        public decimal Number => ConvertNumber(Value);

        public bool Indefinite => Value.Contains('*');
    }

    public class TestNumericalValueAttemptFactory
    {
        public static NumericalValueAttempt Create(string value = "8000") => new(value);
    }

    /// <summary>
    /// 座標数値
    /// </summary>
    /// /// <param name="Value"></param>
    public record class CoordinateValueAttempt(string Value) : IValueDataAttempt
    {
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

        public IValueData Convert() => new CoordinateValue(Value);

        internal static CoordinateValueAttempt Parse(CoordinateValue valueData) => new(valueData.Value);

        public decimal Number => ConvertNumber(Value);

        public bool Indefinite => Value.Contains('*');
    }

    public class TestCoordinateValueAttemptFactory
    {
        public static CoordinateValueAttempt Create(string value = "8.") => new(value);
    }

    /// <summary>
    /// 変数
    /// </summary>
    /// <param name="VariableAddress"></param>
    /// <param name="ValueData"></param>
    public record class NCVariableAttempt(VariableAddressAttempt VariableAddress, IValueDataAttempt ValueData) : INCWordAttempt
    {
        public static NCVariableAttempt Parse(NCVariable ncVariable)
        {
            IValueDataAttempt valueDataAttempt;
            if (ncVariable.ValueData.GetType() == typeof(NumericalValue))
                valueDataAttempt = NumericalValueAttempt.Parse((NumericalValue)ncVariable.ValueData);
            else if (ncVariable.ValueData.GetType() == typeof(CoordinateValue))
                valueDataAttempt = CoordinateValueAttempt.Parse((CoordinateValue)ncVariable.ValueData);
            else
                throw new NotImplementedException();

            return new(VariableAddressAttempt.Parse(ncVariable.VariableAddress), valueDataAttempt);
        }

        public INCWord Convert() => new NCVariable(VariableAddress.Convert(), ValueData.Convert());
    }

    public class TestNCVariableAttemptFactory
    {
        public static NCVariableAttempt Create(
            VariableAddressAttempt? variableAddress = default,
            IValueDataAttempt? valueData = default)
        {
            variableAddress ??= TestVariableAddressAttemptFactory.Create();
            valueData ??= TestCoordinateValueAttemptFactory.Create();
            return new(variableAddress, valueData);
        }
    }

    /// <summary>
    /// 変数のアドレス
    /// </summary>
    /// <param name="Value"></param>
    public record class VariableAddressAttempt(uint Value)
    {
        internal static VariableAddressAttempt Parse(VariableAddress variableAddress) => new(variableAddress.Value);

        internal VariableAddress Convert() => new VariableAddress(Value);
    }

    public class TestVariableAddressAttemptFactory
    {
        public static VariableAddressAttempt Create(uint value = 1) => new(value);
    }
}