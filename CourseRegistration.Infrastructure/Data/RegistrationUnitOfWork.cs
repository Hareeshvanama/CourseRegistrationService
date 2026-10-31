namespace CourseRegistration.Infrastructure.Data;

using System.Data;
using CourseRegistration.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public class RegistrationUnitOfWork : IRegistrationUnitOfWork
{
    private readonly CourseRegistrationDbContext _dbContext;

    public RegistrationUnitOfWork(CourseRegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}