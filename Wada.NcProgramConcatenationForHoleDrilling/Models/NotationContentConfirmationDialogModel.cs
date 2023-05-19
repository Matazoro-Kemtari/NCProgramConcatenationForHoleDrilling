using Reactive.Bindings;

namespace Wada.NcProgramConcatenationForHoleDrilling.Models
{
    internal record class NotationContentConfirmationDialogModel
    {
        internal ReactivePropertySlim<string?> OperationTypeString { get; } = new();
        internal ReactivePropertySlim<string?> SubProgramSource { get; } = new();
    }
}