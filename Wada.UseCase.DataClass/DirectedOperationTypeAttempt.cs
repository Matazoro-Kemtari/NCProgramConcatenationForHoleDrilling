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

    public enum MaterialTypeAttempt
    {
        Undefined,
        Aluminum,
        Iron,
    }

    public enum ReamerTypeAttempt
    {
        Undefined,
        Crystal,
        Skill
    }
}
