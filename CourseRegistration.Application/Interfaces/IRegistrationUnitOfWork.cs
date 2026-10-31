namespace CourseRegistration.Application.Interfaces;

public interface IRegistrationUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
}