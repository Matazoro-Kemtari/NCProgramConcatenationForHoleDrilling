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
        public async Task<NCProgramCode> ReadAllAsync(StreamReader reader, string programName)
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

                /*
                 * コメントとワードを抽出する
                 * \(.+\) : コメントと一致する
                 * (?<!\([^)]*)[A-Za-z] : アドレスと一致する
                 * (?<=#)\d{1,4}= : 変数番号と一致する
                 * -?\d{1,4}(\.\d*)? : 数値と一致する
                 */
                var matchedWords = Regex.Matches(line, @"(\(.+\)|((?<!\([^)]*)[A-Za-z]|(?<=#)\d+=)-?\d+(\.\d*)?)");
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
                    var matchData = Regex.Match(matchWord.Value, @"-?\d+(\.\d*)?");
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
                ncBlocks.Add(new NCBlock(ncWords));
            }

            return new(programName, ncBlocks);
        }
    }
}