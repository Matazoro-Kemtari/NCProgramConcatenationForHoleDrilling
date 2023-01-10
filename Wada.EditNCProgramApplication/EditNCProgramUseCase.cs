using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.EditNCProgramApplication
{
    public interface IEditNCProgramUseCase
    {
        Task<NCProgramCode> ExecuteAsync(EditNCProgramPram editNCProgramPram);
    }

    public class EditNCProgramUseCase : IEditNCProgramUseCase
    {
        [Logging]
        public async Task<NCProgramCode> ExecuteAsync(EditNCProgramPram editNCProgramPram)
        {
            throw new NotImplementedException();
        }
    }

    public record class EditNCProgramPram(NCProgramCode NCProgramCode, MachineToolType MachineTool, MaterialType Material, ReamerType Reamer, double Thickness);
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