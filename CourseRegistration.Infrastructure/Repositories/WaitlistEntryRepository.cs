namespace CourseRegistration.Infrastructure.Repositories;

using CourseRegistration.Application.Interfaces;
using CourseRegistration.Domain.Entities;
using CourseRegistration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class WaitlistEntryRepository : IWaitlistEntryRepository
{
    private readonly CourseRegistrationDbContext _dbContext;

    public WaitlistEntryRepository(CourseRegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WaitlistEntry?> GetActiveWaitlistEntryAsync(int learnerId, int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WaitlistEntries
            .FirstOrDefaultAsync(w => w.LearnerId == learnerId && w.CourseOfferingId == courseOfferingId && w.IsActive, cancellationToken);
    }

    public async Task<List<WaitlistEntry>> GetActiveQueueAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WaitlistEntries
            .Where(w => w.CourseOfferingId == courseOfferingId && w.IsActive)
            .OrderBy(w => w.CreatedAtUtc)
            .ThenBy(w => w.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WaitlistEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.WaitlistEntries.AddAsync(entry, cancellationToken);
    }

    public async Task UpdateAsync(WaitlistEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.WaitlistEntries.Update(entry);
        await Task.CompletedTask;
    }
}
