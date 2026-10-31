namespace CourseRegistration.Application.Interfaces;

using CourseRegistration.Domain.Entities;

public interface IEnrollmentRepository
{
    Task<int> GetEnrollmentCountAsync(int courseOfferingId, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetActiveEnrollmentAsync(int learnerId, int courseOfferingId, CancellationToken cancellationToken = default);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task RemoveAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
}
