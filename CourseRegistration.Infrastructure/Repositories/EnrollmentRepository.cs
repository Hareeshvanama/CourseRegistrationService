namespace CourseRegistration.Infrastructure.Repositories;

using CourseRegistration.Application.Interfaces;
using CourseRegistration.Domain.Entities;
using CourseRegistration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly CourseRegistrationDbContext _dbContext;

    public EnrollmentRepository(CourseRegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> GetEnrollmentCountAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Enrollments.CountAsync(e => e.CourseOfferingId == courseOfferingId, cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByCourseOfferingAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Enrollments
            .AsNoTracking()
            .Include(e => e.Learner)
            .Where(e => e.CourseOfferingId == courseOfferingId)
            .OrderBy(e => e.EnrolledAtUtc)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Enrollment?> GetActiveEnrollmentAsync(int learnerId, int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Enrollments
            .FirstOrDefaultAsync(e => e.LearnerId == learnerId && e.CourseOfferingId == courseOfferingId, cancellationToken);
    }

    public async Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Enrollments.AddAsync(enrollment, cancellationToken);
    }

    public async Task RemoveAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        _dbContext.Enrollments.Remove(enrollment);
        await Task.CompletedTask;
    }
}
