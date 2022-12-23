using System.Text;
using System.Text.RegularExpressions;
using Wada.Extension;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.NCProgramAggregation
{
    public record class NCProgramCode
    {
        public NCProgramCode(string programName, IEnumerable<NCBlock?> ncBlocks)
        {
            ID = Ulid.NewUlid();
            ProgramName = programName;
            NCBlocks = ncBlocks;
        }

        public override string ToString()
        {
            var ncBlocksString = string.Join("\n", NCBlocks.Select(x => x?.ToString()));
            return $"%\n{ncBlocksString}\n%\n";
        }

        /// <summary>
        /// 作業指示を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public OperationType FetchOperationType()
        {
            // 作業指示を探す
            IEnumerable<OperationType> hasOperationType = NCBlocks
                .Where(x => x != null)
                .Select(block => block!.NCWords
                .Where(w => w.GetType() == typeof(NCComment))
                .Select(w =>
                {
                    OperationType responce;
                    if (Regex.IsMatch(w.ToString()!, @"(?<=-)M\d+"))
                        responce = OperationType.TapProcessing;
                    else if (Regex.IsMatch(w.ToString()!, @"(?<=-)D\d{1,2}(\.?\d{1,2})?H\d+"))
                        responce = OperationType.Reaming;
                    else if (Regex.IsMatch(w.ToString()!, @"(?<=-)M\d+"))
                        responce = OperationType.Drilling;
                    else
                        responce = OperationType.Undetected;

                    return responce;
                }))
                .SelectMany(x => x);

            if (!hasOperationType.Any(x => x != OperationType.Undetected))
            {
                // 有効な指示がない場合
                string msg = "作業指示が見つかりません\n" +
                    "サブプログラムを確認して、作業指示を1件追加してください";
                throw new NCProgramConcatenationServiceException(msg);
            }

            if (hasOperationType.Count(x => x != OperationType.Undetected) > 1)
            {
                // 有効な指示が複数ある場合
                string msg = $"作業指示が{hasOperationType.Count(x => x != OperationType.Undetected)}件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
                throw new NCProgramConcatenationServiceException(msg);
            }

            return hasOperationType.First(x => x != OperationType.Undetected);
        }

        public Ulid ID { get; }

        /// <summary>
        /// プログラム番号
        /// </summary>
        public string ProgramName { get; init; }

        public IEnumerable<NCBlock?> NCBlocks { get; init; }
    }

    public enum OperationType
    {
        [EnumDisplayName("タップ")]
        TapProcessing,
        [EnumDisplayName("リーマ")]
        Reaming,
        [EnumDisplayName("ドリル")]
        Drilling,

        [EnumDisplayName("不明")]
        Undetected = int.MaxValue,
    }

    /// <summary>
    /// ブロック
    /// </summary>
    /// <param name="NCWords">ワード</param>
    public record class NCBlock(IEnumerable<INCWord> NCWords)
    {
        public override string ToString()
        {
            StringBuilder buf = new();
            NCWords.ToList().ForEach(x => buf.Append(x.ToString()));
            return buf.ToString();
        }
    }

    public class TestNCProgramCodeFactory
    {
        public static NCProgramCode Create(
            string programName = "O0001",
            IEnumerable<NCBlock?>? ncBlocks = default)
        {
            ncBlocks ??= new List<NCBlock>();
            return new(programName, ncBlocks);
        }
    }

    public class TestNCBlockFactory
    {
        public static NCBlock Create(IEnumerable<INCWord>? ncWords = default)
        {
            ncWords ??= new List<INCWord>();
            return new(ncWords);
        }
    }
}
