using Wada.Extension;

namespace Wada.UseCase.DataClass
{
    public enum DirectedOperationTypeAttempt
    {
        [EnumDisplayName("タップ")]
        Tapping,
        [EnumDisplayName("リーマ")]
        Reaming,
        [EnumDisplayName("ドリル")]
        Drilling,

        [EnumDisplayName("不明")]
        Undetected = int.MaxValue,
    }

    public enum MachineToolTypeAttempt
    {
        [EnumDisplayName("不明")]
        Undefined,

        [EnumDisplayName("RB250F")]
        RB250F = 1,

        [EnumDisplayName("RB260")]
        RB260,

        [EnumDisplayName("611V")]
        Triaxial,
    }

    public enum MaterialTypeAttempt
    {
        [EnumDisplayName("不明")]
        Undefined,

        [EnumDisplayName("AL")]
        Aluminum,

        [EnumDisplayName("SS400")]
        Iron,
    }

    public enum ReamerTypeAttempt
    {
        Undefined,
        Crystal,
        Skill
    }
}
