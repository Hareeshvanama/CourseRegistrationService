namespace CourseRegistration.Domain.Entities;

public class CourseOffering
{
    public int Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresPrerequisite { get; set; } = true;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<WaitlistEntry> WaitlistEntries { get; set; } = new List<WaitlistEntry>();
}
