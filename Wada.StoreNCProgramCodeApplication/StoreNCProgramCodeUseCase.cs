using Wada.UseCase.DataClass;

namespace Wada.StoreNCProgramCodeApplication
{
    public interface IStoreNCProgramCodeUseCase
    {
        Task ExecuteAsync(NCProgramCodeAttempt storableCode);
    }

    public class StoreNCProgramCodeUseCase : IStoreNCProgramCodeUseCase
    {
        public Task ExecuteAsync(NCProgramCodeAttempt storableCode)
        {
            throw new NotImplementedException();
        }
    }
}