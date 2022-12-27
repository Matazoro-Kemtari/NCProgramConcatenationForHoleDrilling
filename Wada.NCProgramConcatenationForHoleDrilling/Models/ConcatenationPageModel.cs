using Reactive.Bindings;

namespace Wada.NCProgramConcatenationForHoleDrilling.Models
{
    internal record class ConcatenationPageModel
    {
        internal void Clear()
        {
            NCProgramFile.Value = null;
        }

        public ReactivePropertySlim<string?> NCProgramFile { get; } = new();
    }
}
