namespace CourseRegistration.Application.Interfaces;

using CourseRegistration.Domain.Entities;

public interface ILearnerRepository
{
    Task<Learner?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
