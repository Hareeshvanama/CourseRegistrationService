namespace CourseRegistration.Infrastructure.Repositories;

using CourseRegistration.Application.Interfaces;
using CourseRegistration.Domain.Entities;
using CourseRegistration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class LearnerRepository : ILearnerRepository
{
    private readonly CourseRegistrationDbContext _dbContext;

    public LearnerRepository(CourseRegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Learner?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Learners
            .Include(l => l.Enrollments)
            .Include(l => l.WaitlistEntries)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }
}
