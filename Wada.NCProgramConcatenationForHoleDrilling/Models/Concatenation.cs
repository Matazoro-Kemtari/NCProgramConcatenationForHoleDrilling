using Reactive.Bindings;

namespace Wada.NCProgramConcatenationForHoleDrilling.Models
{
    public record class Concatenation
    {
        public ReactivePropertySlim<string> NCProgramFile { get; init; } = new();
    }
}
