using Reactive.Bindings;

namespace Wada.NCProgramConcatenationForHoleDrilling.Models
{
    internal record class Concatenation
    {
        internal void Clear()
        {
            NCProgramFile.Value = default;
        }

        public ReactivePropertySlim<string> NCProgramFile { get; init; } = new();
    }
}
