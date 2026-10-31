namespace CourseRegistration.Application.Interfaces;

using CourseRegistration.Domain.Entities;

public interface IWaitlistEntryRepository
{
    Task<WaitlistEntry?> GetActiveWaitlistEntryAsync(int learnerId, int courseOfferingId, CancellationToken cancellationToken = default);
    Task<List<WaitlistEntry>> GetActiveQueueAsync(int courseOfferingId, CancellationToken cancellationToken = default);
    Task AddAsync(WaitlistEntry entry, CancellationToken cancellationToken = default);
    Task UpdateAsync(WaitlistEntry entry, CancellationToken cancellationToken = default);
}
