using Reactive.Bindings;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationForHoleDrilling.ViewModels;
using Wada.UseCase.DataClass;

namespace Wada.NCProgramConcatenationForHoleDrilling.Models
{
    internal record class ConcatenationPageModel
    {
        [Logging]
        internal void Clear()
        {
            NCProgramFile.Value = string.Empty;
            FetchedOperationType.Value = DirectedOperationTypeAttempt.Undetected;
            MachineTool.Value = MachineToolType.Undefined;
            Material.Value = MaterialType.Undefined;
            Reamer.Value = ReamerType.Undefined;
            Thickness.Value = string.Empty;
        }

        public ReactivePropertySlim<string> NCProgramFile { get; } = new();

        public ReactivePropertySlim<DirectedOperationTypeAttempt> FetchedOperationType { get; } = new(DirectedOperationTypeAttempt.Undetected);

        public ReactivePropertySlim<MachineToolType> MachineTool { get; } = new();

        public ReactivePropertySlim<MaterialType> Material { get; } = new();

        public ReactivePropertySlim<ReamerType> Reamer { get; } = new();

        public ReactivePropertySlim<string> Thickness { get; } = new(string.Empty);

        public ReactivePropertySlim<string> SubProgramNumber { get; } = new(string.Empty);
        
        public ReactivePropertySlim<decimal> TargetToolDiameter { get; } = new();
    }
}
