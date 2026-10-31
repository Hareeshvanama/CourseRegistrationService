namespace CourseRegistration.Application.Services;

using CourseRegistration.Application.DTOs;
using CourseRegistration.Application.Interfaces;
using CourseRegistration.Domain.Entities;

public class CourseRegistrationService : ICourseRegistrationService
{
    private readonly ICourseOfferingRepository _courseOfferingRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IWaitlistEntryRepository _waitlistEntryRepository;
    private readonly ILearnerRepository _learnerRepository;
    private readonly IRegistrationUnitOfWork _unitOfWork;

    public CourseRegistrationService(
        ICourseOfferingRepository courseOfferingRepository,
        IEnrollmentRepository enrollmentRepository,
        IWaitlistEntryRepository waitlistEntryRepository,
        ILearnerRepository learnerRepository,
        IRegistrationUnitOfWork unitOfWork)
    {
        _courseOfferingRepository = courseOfferingRepository;
        _enrollmentRepository = enrollmentRepository;
        _waitlistEntryRepository = waitlistEntryRepository;
        _learnerRepository = learnerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CourseRegistrationResult> EnrollLearnerAsync(EnrollLearnerRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var learner = await _learnerRepository.GetByIdAsync(request.LearnerId, token);
            if (learner is null)
            {
                return Failure("Learner not found.");
            }

            var offering = await _courseOfferingRepository.GetByIdAsync(request.CourseOfferingId, token);
            if (offering is null || !offering.IsActive)
            {
                return Failure("Course offering not found or inactive.");
            }

            var duplicateEnrollment = await _enrollmentRepository.GetActiveEnrollmentAsync(request.LearnerId, request.CourseOfferingId, token);
            if (duplicateEnrollment is not null)
            {
                return Failure("Learner is already enrolled in this offering.");
            }

            var duplicateWaitlist = await _waitlistEntryRepository.GetActiveWaitlistEntryAsync(request.LearnerId, request.CourseOfferingId, token);
            if (duplicateWaitlist is not null)
            {
                return Failure("Learner is already waitlisted for this offering.");
            }

            if (!IsEligible(learner, offering))
            {
                return Failure("Learner does not meet the prerequisite requirement.");
            }

            if (offering.Capacity < 0)
            {
                return Failure("Course capacity cannot be negative.");
            }

            var activeEnrollments = await _enrollmentRepository.GetEnrollmentCountAsync(offering.Id, token);
            if (activeEnrollments < offering.Capacity)
            {
                await _enrollmentRepository.AddAsync(CreateEnrollment(learner, offering), token);

                return new CourseRegistrationResult
                {
                    Success = true,
                    Enrolled = true,
                    Message = "Learner enrolled successfully."
                };
            }

            var waitlistEntry = new WaitlistEntry
            {
                LearnerId = learner.Id,
                CourseOfferingId = offering.Id,
                Learner = learner,
                CourseOffering = offering
            };

            await _waitlistEntryRepository.AddAsync(waitlistEntry, token);

            return new CourseRegistrationResult
            {
                Success = true,
                Waitlisted = true,
                Message = "Course is full. Learner was added to the waitlist."
            };
        }, cancellationToken);
    }

    public async Task<CourseRegistrationResult> DropLearnerAsync(DropLearnerRequest request, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var enrollment = await _enrollmentRepository.GetActiveEnrollmentAsync(request.LearnerId, request.CourseOfferingId, token);
            if (enrollment is null)
            {
                return Failure("Learner is not currently enrolled in this offering.");
            }

            await _enrollmentRepository.RemoveAsync(enrollment, token);
            await _unitOfWork.SaveChangesAsync(token);
            var promotionResult = await PromoteNextEligibleWaitlistCoreAsync(request.CourseOfferingId, token);

            return new CourseRegistrationResult
            {
                Success = true,
                Dropped = true,
                Enrolled = promotionResult.Enrolled,
                Message = promotionResult.Enrolled ? "Learner dropped and the next eligible learner was promoted." : "Learner dropped successfully."
            };
        }, cancellationToken);
    }

    public async Task<CourseRegistrationResult> PromoteNextEligibleWaitlistAsync(int courseOfferingId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(
            token => PromoteNextEligibleWaitlistCoreAsync(courseOfferingId, token), cancellationToken);
    }

    private async Task<CourseRegistrationResult> PromoteNextEligibleWaitlistCoreAsync(int courseOfferingId, CancellationToken cancellationToken)
    {
        var offering = await _courseOfferingRepository.GetByIdAsync(courseOfferingId, cancellationToken);
        if (offering is null || !offering.IsActive)
        {
            return Failure("Course offering not found or inactive.");
        }

        var activeEnrollmentCount = await _enrollmentRepository.GetEnrollmentCountAsync(offering.Id, cancellationToken);
        if (activeEnrollmentCount >= offering.Capacity)
        {
            return Failure("No seat available for promotion.");
        }

        var queue = await _waitlistEntryRepository.GetActiveQueueAsync(courseOfferingId, cancellationToken);
        foreach (var entry in queue)
        {
            var learner = entry.Learner ?? await _learnerRepository.GetByIdAsync(entry.LearnerId, cancellationToken);
            if (learner is null || !IsEligible(learner, offering))
            {
                continue;
            }

            await _enrollmentRepository.AddAsync(CreateEnrollment(learner, offering), cancellationToken);
            entry.IsActive = false;
            await _waitlistEntryRepository.UpdateAsync(entry, cancellationToken);

            return new CourseRegistrationResult
            {
                Success = true,
                Enrolled = true,
                Message = "Next eligible waitlist learner promoted to enrollment."
            };
        }

        return Failure(queue.Count == 0 ? "No waitlist entries to promote." : "No eligible learner found in waitlist.");
    }

    private static bool IsEligible(Learner learner, CourseOffering offering)
    {
        return !offering.RequiresPrerequisite || learner.HasRequiredPrerequisite;
    }

    private static Enrollment CreateEnrollment(Learner learner, CourseOffering offering)
    {
        return new Enrollment
        {
            LearnerId = learner.Id,
            CourseOfferingId = offering.Id,
            Learner = learner,
            CourseOffering = offering
        };
    }

    private static CourseRegistrationResult Failure(string message)
    {
        return new CourseRegistrationResult { Success = false, Message = message };
    }
}
