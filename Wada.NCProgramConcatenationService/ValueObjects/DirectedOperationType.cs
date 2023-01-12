using Wada.Extension;

namespace Wada.NCProgramConcatenationService.ValueObjects
{
    public enum DirectedOperationType
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
}
