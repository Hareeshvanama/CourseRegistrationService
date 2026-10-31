namespace CourseRegistration.Domain.Entities;

public class Learner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasRequiredPrerequisite { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<WaitlistEntry> WaitlistEntries { get; set; } = new List<WaitlistEntry>();
}
