namespace CourseRegistration.Application.DTOs;

public class RegisteredLearnerDto
{
    public int EnrollmentId { get; set; }
    public int CourseOfferingId { get; set; }
    public int LearnerId { get; set; }
    public string LearnerName { get; set; } = string.Empty;
    public DateTime EnrolledAtUtc { get; set; }
}
