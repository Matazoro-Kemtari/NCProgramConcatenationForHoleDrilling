using Reactive.Bindings;

namespace Wada.NCProgramConcatenationForHoleDrilling.Models
{
    internal record class NotationContentConfirmation
    {
        public ReactivePropertySlim<string> OperationTypeString { get; } = new();
        public ReactivePropertySlim<string> SubProgramSource { get; } = new();
    }
}