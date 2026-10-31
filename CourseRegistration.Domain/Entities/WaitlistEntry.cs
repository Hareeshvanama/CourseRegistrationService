namespace CourseRegistration.Domain.Entities;

public class WaitlistEntry
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public int CourseOfferingId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Learner Learner { get; set; } = null!;
    public CourseOffering CourseOffering { get; set; } = null!;
}
