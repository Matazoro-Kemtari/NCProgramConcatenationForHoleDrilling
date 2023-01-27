using System.Text;
using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.UseCase.DataClass
{
    /// <summary>
    /// NCプログラム
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="MainProgramClassification"></param>
    /// <param name="ProgramName"></param>
    /// <param name="NCBlocks"></param>
    public record class NCProgramCodeAttempt(
        string ID,
        MainProgramTypeAttempt MainProgramClassification,
        string ProgramName,
        IEnumerable<NCBlockAttempt?> NCBlocks)
    {
        public override string ToString()
        {
            var ncBlocksString = string.Join("\n", NCBlocks.Select(x => x?.ToString()));
            return $"%\n{ncBlocksString}\n%\n";
        }

        public static NCProgramCodeAttempt Parse(NCProgramCode ncProgramCode) => new(
            ncProgramCode.ID.ToString(),
            (MainProgramTypeAttempt)ncProgramCode.MainProgramClassification,
            ncProgramCode.ProgramName,
            ncProgramCode.NCBlocks.Select(x => x == null ? null : NCBlockAttempt.Parse(x)));

        public NCProgramCode Convert() => NCProgramCode.ReConstruct(ID, (NCProgramType)MainProgramClassification, ProgramName, NCBlocks.Select(x => x?.Convert()));
    }

    public class TestNCProgramCodeAttemptFactory
    {
        public static NCProgramCodeAttempt Create(
            string id = "01GQK2ATZNJTVTGC6A0SD00JB6",
            MainProgramTypeAttempt mainProgramClassification = MainProgramTypeAttempt.CenterDrilling,
            string orogramName = "O1234",
            IEnumerable<NCBlockAttempt?>? ncBlocks = null)
        {
            ncBlocks ??= new List<NCBlockAttempt?>
            {
                TestNCBlockAttemptFactory.Create(
                    ncWords:new List<INCWordAttempt>
                    {
                        TestNCCommentAttemptFactory.Create(),
                    }),
                TestNCBlockAttemptFactory.Create(),
                null,
            };

            return new NCProgramCodeAttempt(
                id,
                mainProgramClassification,
                orogramName,
                ncBlocks);
        }
    }

    /// <summary>
    /// サブプログラム用NCプログラム
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="MainProgramClassification"></param>
    /// <param name="ProgramName"></param>
    /// <param name="NCBlocks"></param>
    /// <param name="DirectedOperationClassification"></param>
    /// <param name="DirectedOperationToolDiameter"></param>
    public record class SubNCProgramCodeAttemp(
        string ID,
        MainProgramTypeAttempt MainProgramClassification,
        string ProgramName,
        IEnumerable<NCBlockAttempt?> NCBlocks,
        DirectedOperationTypeAttempt DirectedOperationClassification,
        decimal DirectedOperationToolDiameter)
        : NCProgramCodeAttempt(ID, MainProgramClassification, ProgramName, NCBlocks)
    {
        public override string ToString()
        {
            var ncBlocksString = string.Join("\n", NCBlocks.Select(x => x?.ToString()));
            return $"%\n{ncBlocksString}\n%\n";
        }

        public static new SubNCProgramCodeAttemp Parse(NCProgramCode ncProgramCode)
        {
            var ncBlocks = ncProgramCode.NCBlocks.Select(x => x == null ? null : NCBlockAttempt.Parse(x));
            return new(
            ncProgramCode.ID.ToString(),
            (MainProgramTypeAttempt)ncProgramCode.MainProgramClassification,
            ncProgramCode.ProgramName,
            ncBlocks,
            FetchDirectedOperationType(ncBlocks),
            FetchDirectedOperationToolDiameter(ncBlocks));
        }

        /// <summary>
        /// 作業指示を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UseCase_DataClassException"></exception>
        [Logging]
        private static DirectedOperationTypeAttempt FetchDirectedOperationType(IEnumerable<NCBlockAttempt?> ncBlocks)
        {
            // 作業指示を探す
            IEnumerable<DirectedOperationTypeAttempt> hasOperationType = ncBlocks
                .Where(x => x != null)
                .Select(block => block!.NCWords
                .Where(w => w.GetType() == typeof(NCCommentAttempt))
                .Select(w =>
                {
                    DirectedOperationTypeAttempt responce;
                    if (Regex.IsMatch(w.ToString()!, @"(?<=-)M\d+"))
                        responce = DirectedOperationTypeAttempt.Tapping;
                    else if (Regex.IsMatch(w.ToString()!, @"(?<=-)D\d+(\.?\d+)?[HG]\d+"))
                        responce = DirectedOperationTypeAttempt.Reaming;
                    else if (Regex.IsMatch(w.ToString()!, @"(?<=-)D\d+(\.?\d+)?DR"))
                        responce = DirectedOperationTypeAttempt.Drilling;
                    else
                        responce = DirectedOperationTypeAttempt.Undetected;

                    return responce;
                }))
                .SelectMany(x => x);

            if (hasOperationType.All(x => x == DirectedOperationTypeAttempt.Undetected))
                // 有効な指示が1件もない場合
                return DirectedOperationTypeAttempt.Undetected;

            if (hasOperationType.Count(x => x != DirectedOperationTypeAttempt.Undetected) > 1)
            {
                // 有効な指示が複数ある場合
                string msg = $"作業指示が{hasOperationType.Count(x => x != DirectedOperationTypeAttempt.Undetected)}件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
                throw new UseCase_DataClassException(msg);
            }

            return hasOperationType.First(x => x != DirectedOperationTypeAttempt.Undetected);
        }

        /// <summary>
        /// ツール径を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UseCase_DataClassException"></exception>
        [Logging]
        public static decimal FetchDirectedOperationToolDiameter(IEnumerable<NCBlockAttempt?> ncBlocks)
        {
            // 作業指示を探す
            IEnumerable<decimal> hasOperationType = ncBlocks
                .Where(x => x != null)
                .Select(block => block!.NCWords
                .Where(w => w.GetType() == typeof(NCCommentAttempt))
                .Select(w =>
                {
                    var tapMatch = Regex.Match(w.ToString()!, @"(?<=-M)\d+(\.\d+)?");
                    var reamerMatch = Regex.Match(w.ToString()!, @"(?<=-D)\d+(\.\d+)?(?=[HG]\d+)");
                    var drillMatch = Regex.Match(w.ToString()!, @"(?<=-D)\d+(\.\d+)?(?=DR)");

                    decimal diameter;
                    if (tapMatch.Success)
                        diameter = decimal.Parse(tapMatch.Value);
                    else if (reamerMatch.Success)
                        diameter = decimal.Parse(reamerMatch.Value);
                    else if (drillMatch.Success)
                        diameter = decimal.Parse(drillMatch.Value);
                    else
                        diameter = decimal.MinValue;

                    return diameter;
                }))
                .SelectMany(x => x);

            if (hasOperationType.All(x => x == decimal.MinValue))
                // 有効な指示が1件もない場合
                return 0m;

            if (hasOperationType.Count(x => x != decimal.MinValue) > 1)
            {
                // 有効な指示が複数ある場合
                string msg = $"作業指示が{hasOperationType.Count(x => x != decimal.MinValue)}件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
                throw new UseCase_DataClassException(msg);
            }

            return hasOperationType.First(x => x != decimal.MinValue);
        }
    }

    public class TestSubNCProgramCodeAttemptFactory
    {
        public static SubNCProgramCodeAttemp Create(
            string id = "01GQK2ATZNJTVTGC6A0SD00JB6",
            MainProgramTypeAttempt mainProgramClassification = MainProgramTypeAttempt.CenterDrilling,
            string programName = "O1234",
            IEnumerable<NCBlockAttempt?>? ncBlocks = null,
            DirectedOperationTypeAttempt directedOperationClassification = DirectedOperationTypeAttempt.Reaming,
            decimal directedOperationToolDiameter = 13.3m)
        {
            var ncProgram = TestNCProgramCodeAttemptFactory.Create(
                id, mainProgramClassification, programName, ncBlocks);

            return new SubNCProgramCodeAttemp(
                ncProgram.ID,
                ncProgram.MainProgramClassification,
                ncProgram.ProgramName,
                ncProgram.NCBlocks,
                directedOperationClassification,
                directedOperationToolDiameter);
        }
    }

    /// <summary>
    /// メインプログラム種別
    /// </summary>
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
        public override string ToString()
        {
            StringBuilder buf = new();
            if (HasBlockSkip != OptionalBlockSkipAttempt.None)
            {
                if (HasBlockSkip == OptionalBlockSkipAttempt.BDT1)
                    buf.Append('/');
                else
                    buf.Append("/" + (int)HasBlockSkip);
            }

            NCWords.ToList().ForEach(x => buf.Append(x.ToString()));
            return buf.ToString();
        }

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
        public override string ToString() => $"({Comment})";

        public static NCCommentAttempt Parse(NCComment ncComment) => new(ncComment.Comment);

        public INCWord Convert() => new NCComment(Comment);
    }

    public class TestNCCommentAttemptFactory
    {
        public static NCCommentAttempt Create(string comment = "SAMPLE")
            => new(comment);
    }

    /// <summary>
    /// ワード
    /// </summary>
    /// <param name="Address"></param>
    /// <param name="ValueData"></param>
    public record class NCWordAttempt(AddressAttempt Address, IValueDataAttempt ValueData) : INCWordAttempt
    {
        public override string ToString() => Address.ToString() + ValueData.ToString();

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
        public override string ToString() => Value.ToString();

        public static AddressAttempt Parse(Address address) => new(address.Value);

        internal Address Convert() => new(Value);
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
        public override string ToString() => Value;

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
        public override string ToString() => Value;

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
        public override string ToString() => $"#{VariableAddress}={ValueData}";

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

        internal VariableAddress Convert() => new(Value);
    }

    public class TestVariableAddressAttemptFactory
    {
        public static VariableAddressAttempt Create(uint value = 1) => new(value);
    }
}