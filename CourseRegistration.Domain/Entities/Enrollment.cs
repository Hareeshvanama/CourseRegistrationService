namespace CourseRegistration.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public int CourseOfferingId { get; set; }
    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;

    public Learner Learner { get; set; } = null!;
    public CourseOffering CourseOffering { get; set; } = null!;
}
