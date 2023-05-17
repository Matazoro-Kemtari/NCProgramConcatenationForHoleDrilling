using Reactive.Bindings;

namespace Wada.NcProgramConcatenationForHoleDrilling.Models
{
    internal record class NotationContentConfirmationDialogModel
    {
        public ReactivePropertySlim<string?> OperationTypeString { get; } = new();
        public ReactivePropertySlim<string?> SubProgramSource { get; } = new();
    }
}