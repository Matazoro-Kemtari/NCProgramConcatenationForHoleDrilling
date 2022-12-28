using Reactive.Bindings;
using Wada.NCProgramConcatenationForHoleDrilling.ViewModels;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.NCProgramConcatenationForHoleDrilling.Models
{
    internal record class ConcatenationPageModel
    {
        internal void Clear()
        {
            NCProgramFile.Value = string.Empty;
            FetchedOperationType.Value = DirectedOperationType.Undetected;
            MachineTool.Value = MachineToolType.Undefined;
            Material.Value = MaterialType.Undefined;
            Reamer.Value = ReamerType.Undefined;
            Thickness.Value = string.Empty;
        }

        public ReactivePropertySlim<string> NCProgramFile { get; } = new();

        public ReactivePropertySlim<DirectedOperationType> FetchedOperationType { get; } = new(DirectedOperationType.Undetected);

        public ReactivePropertySlim<MachineToolType> MachineTool { get; } = new();

        public ReactivePropertySlim<MaterialType> Material { get; } = new();

        public ReactivePropertySlim<ReamerType> Reamer { get; } = new();

        public ReactivePropertySlim<string> Thickness { get; } = new(string.Empty);
    }
}
