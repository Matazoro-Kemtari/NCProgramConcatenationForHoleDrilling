using Reactive.Bindings;
using Wada.NCProgramConcatenationForHoleDrilling.ViewModels;

namespace Wada.NCProgramConcatenationForHoleDrilling.Models
{
    internal record class ConcatenationPageModel
    {
        internal void Clear()
        {
            NCProgramFile.Value = string.Empty;
            MachineTool.Value = MachineToolType.Undefined;
            Material.Value = MaterialType.Undefined;
            Thickness.Value = string.Empty;
        }

        public ReactivePropertySlim<string> NCProgramFile { get; } = new();

        public ReactivePropertySlim<MachineToolType> MachineTool { get; } = new();

        public ReactivePropertySlim<MaterialType> Material { get; } = new();

        public ReactivePropertySlim<string> Thickness { get; } = new(string.Empty);
    }
}
