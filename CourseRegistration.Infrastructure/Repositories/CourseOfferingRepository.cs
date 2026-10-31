namespace CourseRegistration.Infrastructure.Repositories;

using CourseRegistration.Application.Interfaces;
using CourseRegistration.Domain.Entities;
using CourseRegistration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class CourseOfferingRepository : ICourseOfferingRepository
{
    private readonly CourseRegistrationDbContext _dbContext;

    public CourseOfferingRepository(CourseRegistrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourseOffering?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CourseOfferings
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(CourseOffering offering, CancellationToken cancellationToken = default)
    {
        _dbContext.CourseOfferings.Update(offering);
        await Task.CompletedTask;
    }
}
