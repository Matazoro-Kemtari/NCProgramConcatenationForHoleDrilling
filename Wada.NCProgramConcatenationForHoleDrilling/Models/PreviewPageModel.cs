using Reactive.Bindings;

namespace Wada.NcProgramConcatenationForHoleDrilling.Models
{
    internal record class PreviewPageModel
    {
        public ReactivePropertySlim<string> CombinedProgramSource { get; } = new();
    }
}
