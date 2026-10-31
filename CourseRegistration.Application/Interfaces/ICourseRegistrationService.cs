namespace CourseRegistration.Application.Interfaces;

using CourseRegistration.Application.DTOs;

public interface ICourseRegistrationService
{
    Task<CourseRegistrationResult> EnrollLearnerAsync(EnrollLearnerRequest request, CancellationToken cancellationToken = default);
    Task<CourseRegistrationResult> DropLearnerAsync(DropLearnerRequest request, CancellationToken cancellationToken = default);
    Task<CourseRegistrationResult> PromoteNextEligibleWaitlistAsync(int courseOfferingId, CancellationToken cancellationToken = default);
}
