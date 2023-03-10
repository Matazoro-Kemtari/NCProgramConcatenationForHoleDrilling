using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramFile
{
    public class NCProgramRepository : INCProgramRepository
    {
        [Logging]
        public async Task<NCProgramCode> ReadAllAsync(StreamReader reader, NCProgramType ncProgram, string programName)
        {
            List<NCBlock?> ncBlocks = new();

            while (!reader.EndOfStream)
            {
                // 1行読込
                string? line = await reader.ReadLineAsync();

                if (line == null)
                {
                    ncBlocks.Add(null);
                    continue;
                }
                else if (line.Trim() == "%")
                    continue;

                // オプショナルブロックスキップ判定
                OptionalBlockSkip hasBlockSkip = ExistsOptionalBlockSkip(line);

                /*
                 * コメントとワード(アドレス+数値)を抽出する
                 * 集積すると分かりにくいので注意
                 */
                var matchedWords = Regex.Matches(
                    line,
                    @"(\(.+\)" + // コメントと一致する
                    @"|(?<!\([^)]*)[A-Za-z](-?\d+(\.\d*)?|\*+)" + // ワード(アドレス+数値)と一致する
                    @"|(?<=#)\d+=-?\d+(\.\d*)?)"); // 変数と一致する
                if (matchedWords.Count == 0)
                {
                    ncBlocks.Add(null);
                    continue;
                }

                List<INCWord> ncWords = new();
                foreach (Match matchWord in matchedWords.Cast<Match>())
                {
                    // コメント
                    var matchComment = Regex.Match(matchWord.Value, @"(?<=\()[^\)]+");
                    // ワード
                    var matchAddress = Regex.Match(matchWord.Value, @"[A-Za-z]");
                    var matchData = Regex.Match(matchWord.Value, @"(-?\d+(\.\d*)?|\*+)");
                    // 変数
                    var matchVariable = Regex.Match(matchWord.Value, @"\d+(?==)");
                    var matchVarValue = Regex.Match(matchWord.Value, @"(?<==)-?\d+(\.\d*)?");

                    INCWord ncWord;
                    if (matchComment.Success)
                    {
                        ncWord = new NCComment(matchComment.Value);
                    }
                    else if (matchAddress.Success && matchData.Success)
                    {
                        ncWord = new NCWord(
                            new Address(matchAddress.Value.ToCharArray()[0]),
                            new NumericalValue(matchData.Value));
                    }
                    else if (matchVariable.Success && matchVarValue.Success)
                    {
                        ncWord = new NCVariable(
                            new VariableAddress(uint.Parse(matchVariable.Value)),
                            new CoordinateValue(matchVarValue.Value));
                    }
                    else
                    {
                        throw new InvalidOperationException($"想定外のNC命令がありました NC命令: {matchWord.Value}");
                    }
                    ncWords.Add(ncWord);
                }
                ncBlocks.Add(new NCBlock(ncWords, hasBlockSkip));
            }

            return new(ncProgram, programName, ncBlocks);
        }

        private static OptionalBlockSkip ExistsOptionalBlockSkip(string line)
        {
            OptionalBlockSkip hasBlockSkip = OptionalBlockSkip.None;
            if (Regex.IsMatch(line, @"^/[1-9]?(?!\d)"))
            {
                // スラッシュの後の数字1桁
                Match num = Regex.Match(line, @"(?<=/)\d");

                if (num.Success)
                {
                    hasBlockSkip = (OptionalBlockSkip)int.Parse(num.Value);
                }
                else
                    hasBlockSkip = OptionalBlockSkip.BDT1;
            }

            return hasBlockSkip;
        }

        public async Task WriteAllAsync(StreamWriter writer, string ncProgramCode)
        {
            await writer.WriteAsync(ncProgramCode);
            await writer.FlushAsync();
        }
    }
}