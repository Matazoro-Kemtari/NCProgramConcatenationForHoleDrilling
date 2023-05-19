using Reactive.Bindings;

namespace Wada.NcProgramConcatenationForHoleDrilling.Models
{
    internal record class PreviewPageModel
    {
        internal ReactivePropertySlim<string> CombinedProgramSource { get; } = new();
    }
}
