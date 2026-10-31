using CourseRegistration.Application.Interfaces;
using CourseRegistration.Application.Services;
using CourseRegistration.Infrastructure.Data;
using CourseRegistration.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<CourseRegistrationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CourseRegistration") ?? "Data Source=course-registration.db"));

builder.Services.AddScoped<ICourseRegistrationService, CourseRegistrationService>();
builder.Services.AddScoped<ICourseOfferingRepository, CourseOfferingRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IWaitlistEntryRepository, WaitlistEntryRepository>();
builder.Services.AddScoped<ILearnerRepository, LearnerRepository>();
builder.Services.AddScoped<IRegistrationUnitOfWork, RegistrationUnitOfWork>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CourseRegistrationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", async (CourseRegistrationDbContext dbContext, CancellationToken cancellationToken) =>
{
    var databaseConnected = await dbContext.Database.CanConnectAsync(cancellationToken);
    return databaseConnected
        ? Results.Ok(new { status = "Healthy", database = "Connected" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
