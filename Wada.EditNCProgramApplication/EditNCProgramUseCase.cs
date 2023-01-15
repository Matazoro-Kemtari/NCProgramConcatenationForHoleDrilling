using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.EditNCProgramApplication
{
    public interface IEditNCProgramUseCase
    {
        Task<IEnumerable<NCProgramCode>> ExecuteAsync(EditNCProgramPram editNCProgramPram);
    }

    public record class EditNCProgramPram(
        Dictionary<string, NCProgramCode> MainProgramCodes,
        MachineToolType MachineTool,
        MaterialType Material,
        ReamerType Reamer,
        double Thickness);

    public class EditNCProgramUseCase : IEditNCProgramUseCase
    {
        [Logging]
        public async Task<IEnumerable<NCProgramCode>> ExecuteAsync(EditNCProgramPram editNCProgramPram)
        {
            throw new NotImplementedException();
        }
    }

    public enum MachineToolType
    {
        Undefined,
        RB250F,
        RB260,
        Triaxial,
    }
    public enum MaterialType
    {
        Undefined,
        Aluminum,
        Iron,
    }
    public enum ReamerType
    {
        Undefined,
        Crystal,
        Skill
    }
}