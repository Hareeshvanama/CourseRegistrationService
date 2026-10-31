namespace CourseRegistration.Api.Controllers;

using CourseRegistration.Application.DTOs;
using CourseRegistration.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CourseRegistrationController : ControllerBase
{
    private readonly ICourseRegistrationService _service;

    public CourseRegistrationController(ICourseRegistrationService service)
    {
        _service = service;
    }

    [HttpGet("{courseOfferingId:int}/enrollments")]
    public async Task<IActionResult> GetRegisteredLearners(int courseOfferingId, CancellationToken cancellationToken)
    {
        var learners = await _service.GetRegisteredLearnersAsync(courseOfferingId, cancellationToken);
        return Ok(learners);
    }

    [HttpPost("enroll")]
    public async Task<IActionResult> Enroll([FromBody] EnrollLearnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.EnrollLearnerAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("drop")]
    public async Task<IActionResult> Drop([FromBody] DropLearnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.DropLearnerAsync(request, cancellationToken);
        return Ok(result);
    }
}
