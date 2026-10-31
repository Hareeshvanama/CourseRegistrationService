namespace CourseRegistration.Application.Interfaces;

using CourseRegistration.Domain.Entities;

public interface ICourseOfferingRepository
{
    Task<CourseOffering?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(CourseOffering offering, CancellationToken cancellationToken = default);
}
