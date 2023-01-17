using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.EditNCProgramApplication
{
    public interface IEditNCProgramUseCase
    {
        Task<IEnumerable<NCProgramCode>> ExecuteAsync(EditNCProgramPram editNCProgramPram);
    }

    public class EditNCProgramUseCase : IEditNCProgramUseCase
    {
        private readonly IMainProgramParameterRewriter _mainParameterRewriter;

        public EditNCProgramUseCase(IMainProgramParameterRewriter mainParameterRewriter)
        {
            _mainParameterRewriter = mainParameterRewriter;
        }

        [Logging]
        public async Task<IEnumerable<NCProgramCode>> ExecuteAsync(EditNCProgramPram editNCProgramPram)
        {
            var taskRewriters = editNCProgramPram.MainProgramCodes.Select(x => Task.Run(() =>
            {
                string key= x.Key;
                NCProgramCode ncProgramCode = x.Value;
                throw new NotImplementedException();
                //return _mainParameterRewriter.RewriteProgramParameter();
            }));
            // TODO: domain service完成後に直し
            throw new NotImplementedException();
            //return await Task.WhenAll(taskRewriters);
        }
    }

    public record class EditNCProgramPram(
        Dictionary<string, NCProgramCode> MainProgramCodes,
        MachineToolType MachineTool,
        MaterialType Material,
        ReamerType Reamer,
        decimal Thickness);

    public class TestEditNCProgramPramFactory
    {
        public static EditNCProgramPram Create(
            Dictionary<string, NCProgramCode>? mainProgramCodes = default,
            MachineToolType machineTool = MachineToolType.RB250F,
            MaterialType material = MaterialType.Aluminum,
            ReamerType reamer = ReamerType.Crystal,
            decimal thickness = 15)
        {
            mainProgramCodes ??= new Dictionary<string, NCProgramCode>()
            {
                {
                    "CD",
                    TestNCProgramCodeFactory.Create(
                        programName: "O1000",
                        new List<NCBlock>
                        {
                            TestNCBlockFactory.Create(
                                new List<INCWord>
                                {
                                    TestNCWordFactory.Create(
                                        TestAddressFactory.Create(),
                                        TestNumericalValueFactory.Create()),
                                }),
                        })
                },
                {
                    "DR",
                    TestNCProgramCodeFactory.Create(
                        programName: "O2000",
                        new List<NCBlock>
                        {
                            TestNCBlockFactory.Create(
                                new List<INCWord>
                                {
                                    TestNCWordFactory.Create(
                                        TestAddressFactory.Create(),
                                        TestNumericalValueFactory.Create()),
                                }),
                        })
                },
                {
                    "MENTORI",
                    TestNCProgramCodeFactory.Create(
                        programName: "O3000",
                        new List<NCBlock>
                        {
                            TestNCBlockFactory.Create(
                                new List<INCWord>
                                {
                                    TestNCWordFactory.Create(
                                        TestAddressFactory.Create(),
                                        TestNumericalValueFactory.Create()),
                                }),
                        })
                },
            };

            return new(mainProgramCodes,
                       machineTool,
                       material,
                       reamer,
                       thickness);
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