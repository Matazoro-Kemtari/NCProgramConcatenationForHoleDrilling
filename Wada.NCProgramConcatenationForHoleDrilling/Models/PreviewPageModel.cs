using Reactive.Bindings;

namespace Wada.NCProgramConcatenationForHoleDrilling.Models
{
    internal record class PreviewPageModel
    {
        public ReactivePropertySlim<string> CombinedProgramSource { get; } = new();
    }
}
