namespace CourseRegistration.Application.DTOs;

public class CourseRegistrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Enrolled { get; set; }
    public bool Waitlisted { get; set; }
    public bool Dropped { get; set; }
}
