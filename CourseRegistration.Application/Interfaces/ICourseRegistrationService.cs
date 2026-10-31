namespace CourseRegistration.Application.Interfaces;

using CourseRegistration.Application.DTOs;

public interface ICourseRegistrationService
{
    Task<IReadOnlyList<RegisteredLearnerDto>> GetRegisteredLearnersAsync(int courseOfferingId, CancellationToken cancellationToken = default);
    Task<CourseRegistrationResult> EnrollLearnerAsync(EnrollLearnerRequest request, CancellationToken cancellationToken = default);
    Task<CourseRegistrationResult> DropLearnerAsync(DropLearnerRequest request, CancellationToken cancellationToken = default);
    Task<CourseRegistrationResult> PromoteNextEligibleWaitlistAsync(int courseOfferingId, CancellationToken cancellationToken = default);
}
