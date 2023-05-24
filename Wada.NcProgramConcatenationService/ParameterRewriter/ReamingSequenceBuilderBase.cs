﻿using System.Data;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ParameterRewriter.Process;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

internal enum ReamerType
{
    CrystalReamerParameter,
    SkillReamerParameter,
}

public abstract class ReamingSequenceBuilderBase : IMainProgramSequenceBuilder
{
    private readonly ParameterType _parameterType;
    private readonly ReamerType _reamerType;

    private protected ReamingSequenceBuilderBase(ParameterType parameterType, ReamerType reamerType)
    {
        _parameterType = parameterType;
        _reamerType = reamerType;
    }

    /// <summary>
    /// 2回下穴のパラメータを書き換える
    /// </summary>
    /// <param name="rewritableCode"></param>
    /// <param name="material"></param>
    /// <param name="thickness"></param>
    /// <param name="drillingMethod">穴加工方法</param>
    /// <param name="blindPilotHoleDepth">止まり穴下穴深さ</param>
    /// <param name="drillingParameters"></param>
    /// <param name="reamingParameter"></param>
    /// <param name="subProgramNumber">サブプログラムNo</param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    [Logging]
    private static List<NcProgramCode> RewriteCncProgramForDrilling(
        NcProgramCode rewritableCode,
        MaterialType material,
        decimal thickness,
        DrillingMethod drillingMethod,
        decimal blindPilotHoleDepth,
        IEnumerable<DrillingProgramParameter> drillingParameters,
        ReamingProgramParameter reamingParameter,
        string subProgramNumber)
    {
        List<NcProgramCode> ncPrograms = new();
        // 下穴 1回目
        var fastDrillingParameter = drillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= reamingParameter.PreparedHoleDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {reamingParameter.PreparedHoleDiameter}");
        var fastDrillingDepth = drillingMethod switch
        {
            DrillingMethod.ThroughHole=> thickness + fastDrillingParameter.DrillTipLength,
            DrillingMethod.BlindHole => blindPilotHoleDepth,
            _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
        };
        ncPrograms.Add(DrillingProgramRewriter.Rewrite(
            rewritableCode,
            material,
            fastDrillingDepth,
            fastDrillingParameter,
            subProgramNumber,
            reamingParameter.PreparedHoleDiameter));

        // 下穴 2回目
        var secondDrillingParameter = drillingParameters
            .Where(x => x.DirectedOperationToolDiameter <= reamingParameter.SecondPreparedHoleDiameter)
            .MaxBy(x => x.DirectedOperationToolDiameter)
            ?? throw new DomainException(
                $"穴径に該当するリストがありません 穴径: {reamingParameter.SecondPreparedHoleDiameter}");
        var secondDrillingDepth = thickness + secondDrillingParameter.DrillTipLength;
        ncPrograms.Add(DrillingProgramRewriter.Rewrite(
            rewritableCode,
            material,
            secondDrillingDepth,
            secondDrillingParameter,
            subProgramNumber,
            reamingParameter.SecondPreparedHoleDiameter));

        return ncPrograms;
    }

    [Logging]
    public virtual IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolRecord rewriteByToolRecord)
    {
        if (rewriteByToolRecord.Material == MaterialType.Undefined)
            throw new ArgumentException("素材が未定義です");

        // _parameterTypeリーマのパラメータを受け取る
        IEnumerable<ReamingProgramParameter> reamingParameters;
        if (_parameterType == ParameterType.CrystalReamerParameter)
            reamingParameters = rewriteByToolRecord.CrystalReamerParameters;
        else
            reamingParameters = rewriteByToolRecord.SkillReamerParameters;

        // ドリルのパラメータを受け取る
        var drillingParameters = rewriteByToolRecord.DrillingParameters;

        ReamingProgramParameter reamingParameter;
        try
        {
            reamingParameter = reamingParameters.First(x => x.DirectedOperationToolDiameter == rewriteByToolRecord.DirectedOperationToolDiameter);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainException(
                $"リーマー径 {rewriteByToolRecord.DirectedOperationToolDiameter}のリストがありません", ex);
        }

        // リーマー

        // メインプログラムを工程ごとに取り出す
        List<NcProgramCode> rewrittenNcPrograms = new();
        foreach (var rewritableCode in rewriteByToolRecord.RewritableCodes)
        {
            switch (rewritableCode.MainProgramClassification)
            {
                case NcProgramType.CenterDrilling:
                    rewrittenNcPrograms.Add(CenterDrillingProgramRewriter.Rewrite(
                        rewritableCode,
                        rewriteByToolRecord.Material,
                        reamingParameter,
                        rewriteByToolRecord.SubProgramNumber));
                    break;
                case NcProgramType.Drilling:
                    rewrittenNcPrograms.AddRange(RewriteCncProgramForDrilling(
                        rewritableCode,
                        rewriteByToolRecord.Material,
                        rewriteByToolRecord.Thickness,
                        rewriteByToolRecord.DrillingMethod,
                        rewriteByToolRecord.BlindPilotHoleDepth,
                        drillingParameters,
                        reamingParameter,
                        rewriteByToolRecord.SubProgramNumber));
                    break;
                case NcProgramType.Chamfering:
                    if (reamingParameter.ChamferingDepth != null)
                        rewrittenNcPrograms.Add(ChamferingProgramRewriter.Rewrite(
                            rewritableCode,
                            rewriteByToolRecord.Material,
                            reamingParameter,
                            rewriteByToolRecord.SubProgramNumber));
                    break;
                case NcProgramType.Reaming:
                    var reamingDepth = rewriteByToolRecord.DrillingMethod switch
                    {
                        DrillingMethod.ThroughHole=> rewriteByToolRecord.Thickness + 5m,
                        DrillingMethod.BlindHole => rewriteByToolRecord.BlindHoleDepth,
                        _ => throw new NotImplementedException("DrillingMethodの値が想定外の値です"),
                    };
                    rewrittenNcPrograms.Add(ReamingProgramRewriter.Rewrite(
                        rewritableCode,
                        rewriteByToolRecord.Material,
                        _reamerType,
                        reamingDepth,
                        reamingParameter,
                        rewriteByToolRecord.SubProgramNumber));
                    break;
                default:
                    // 何もしない
                    break;
            }
        }
        return rewrittenNcPrograms;
    }
}
