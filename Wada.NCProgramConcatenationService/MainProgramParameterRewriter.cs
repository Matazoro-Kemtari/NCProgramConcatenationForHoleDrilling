using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService
{
    public interface IMainProgramParameterRewriter
    {
        /// <summary>
        /// メインプログラムのパラメータを書き換える
        /// </summary>
        /// <param name="rewritableCode"></param>
        /// <param name="mainProgramType"></param>
        /// <returns></returns>
        NCProgramCode RewriteProgramParameter(NCProgramCode rewritableCode, MainProgramType mainProgramType, MaterialType materialType);
    }

    public class MainProgramParameterRewriter : IMainProgramParameterRewriter
    {
        public NCProgramCode RewriteProgramParameter(
            NCProgramCode rewritableCode, 
            MainProgramType mainProgram,
            MaterialType material)
        {
            string spinValue;
            switch (material)
            {
                case MaterialType.Aluminum:
                    spinValue = "2000";
                    break;
                case MaterialType.Iron:
                    spinValue = "1500";
                    break;
                default:
                    throw new AggregateException(nameof(material));
            }
            var rewritedNCBlocks = rewritableCode.NCBlocks
                .Select(x =>
                {
                    return x == null
                        ? null
                        : new NCBlock(
                        x.NCWords.Select(y =>
                        {
                            if (y.GetType() != typeof(NCWord))
                                return y;

                            NCWord ncWord = (NCWord)y;
                            return ncWord with
                            {
                                ValueData = ncWord.ValueData.Value.Contains('*') ?
                                (NumericalValue)ncWord.ValueData with { Value = spinValue } : ncWord.ValueData
                            };
                        }),
                        x.HasBlockSkip); 
                });

            return rewritableCode with
            {
                NCBlocks = rewritedNCBlocks
            };
        }
    }

    public enum MainProgramType
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
    }

    public enum MaterialType
    {
        Undefined,
        Aluminum,
        Iron,
    }

}
