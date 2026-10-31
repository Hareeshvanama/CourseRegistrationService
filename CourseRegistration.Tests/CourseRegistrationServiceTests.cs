namespace CourseRegistration.Tests;

using CourseRegistration.Application.DTOs;
using CourseRegistration.Application.Services;
using CourseRegistration.Domain.Entities;
using CourseRegistration.Infrastructure.Data;
using CourseRegistration.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class CourseRegistrationServiceTests
{
    [Fact]
    public async Task GetRegisteredLearners_ReturnsEnrolledLearnersInEnrollmentOrder()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var firstEnrollment = EnrollmentFor(1);
        firstEnrollment.EnrolledAtUtc = DateTime.UtcNow.AddMinutes(-2);
        var secondEnrollment = EnrollmentFor(2);
        secondEnrollment.EnrolledAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.SeedAsync(
            [EligibleLearner(1), EligibleLearner(2), EligibleLearner(3)],
            Offering(capacity: 2),
            [secondEnrollment, firstEnrollment],
            [WaitlistFor(3, DateTime.UtcNow)]);

        var learners = await fixture.Service.GetRegisteredLearnersAsync(courseOfferingId: 1);

        Assert.Collection(
            learners,
            learner =>
            {
                Assert.Equal(1, learner.LearnerId);
                Assert.Equal("Learner 1", learner.LearnerName);
            },
            learner =>
            {
                Assert.Equal(2, learner.LearnerId);
                Assert.Equal("Learner 2", learner.LearnerName);
            });
    }

    [Fact]
    public async Task Enroll_WhenSeatAvailable_EnrollsLearner()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync([EligibleLearner(1)], Offering(capacity: 1));

        var result = await fixture.Service.EnrollLearnerAsync(Request(learnerId: 1));

        Assert.True(result.Success);
        Assert.True(result.Enrolled);
        Assert.Equal(1, await fixture.Db.Enrollments.CountAsync());
    }

    [Fact]
    public async Task Enroll_WhenCourseIsFull_AddsLearnerToWaitlist()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync([EligibleLearner(1), EligibleLearner(2)], Offering(capacity: 1), [EnrollmentFor(1)]);

        var result = await fixture.Service.EnrollLearnerAsync(Request(learnerId: 2));

        Assert.True(result.Success);
        Assert.True(result.Waitlisted);
        Assert.Equal(1, await fixture.Db.WaitlistEntries.CountAsync(w => w.IsActive));
    }

    [Fact]
    public async Task Enroll_WhenLearnerAlreadyEnrolled_DoesNotAddDuplicate()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync([EligibleLearner(1)], Offering(capacity: 2), [EnrollmentFor(1)]);

        var result = await fixture.Service.EnrollLearnerAsync(Request(learnerId: 1));

        Assert.False(result.Success);
        Assert.Equal(1, await fixture.Db.Enrollments.CountAsync());
    }

    [Fact]
    public async Task Enroll_WhenAlreadyWaitlisted_DoesNotAddDuplicate()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync(
            [EligibleLearner(1), EligibleLearner(2)],
            Offering(capacity: 1),
            [EnrollmentFor(1)],
            [WaitlistFor(2, DateTime.UtcNow)]);

        var result = await fixture.Service.EnrollLearnerAsync(Request(learnerId: 2));

        Assert.False(result.Success);
        Assert.Equal(1, await fixture.Db.WaitlistEntries.CountAsync(w => w.IsActive));
    }

    [Fact]
    public async Task Enroll_WhenPrerequisiteMissing_RejectsLearner()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync([new Learner { Id = 1, Name = "No prerequisite" }], Offering(capacity: 1));

        var result = await fixture.Service.EnrollLearnerAsync(Request(learnerId: 1));

        Assert.False(result.Success);
        Assert.Empty(await fixture.Db.Enrollments.ToListAsync());
        Assert.Empty(await fixture.Db.WaitlistEntries.ToListAsync());
    }

    [Fact]
    public async Task Drop_WhenEnrolled_PromotesEarliestEligibleWaitlistedLearner()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync(
            [EligibleLearner(1), EligibleLearner(2), EligibleLearner(3)],
            Offering(capacity: 1),
            [EnrollmentFor(1)],
            [WaitlistFor(2, DateTime.UtcNow.AddMinutes(-2)), WaitlistFor(3, DateTime.UtcNow.AddMinutes(-1))]);

        var result = await fixture.Service.DropLearnerAsync(DropRequest(learnerId: 1));

        Assert.True(result.Success);
        Assert.True(result.Dropped);
        Assert.Equal(new[] { 2 }, await fixture.Db.Enrollments.Select(e => e.LearnerId).ToArrayAsync());
        Assert.False((await fixture.Db.WaitlistEntries.SingleAsync(w => w.LearnerId == 2)).IsActive);
        Assert.True((await fixture.Db.WaitlistEntries.SingleAsync(w => w.LearnerId == 3)).IsActive);
    }

    [Fact]
    public async Task Drop_WhenQueueFrontIsIneligible_SkipsItAndLeavesItQueued()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync(
            [EligibleLearner(1), new Learner { Id = 2, Name = "Not eligible" }, EligibleLearner(3)],
            Offering(capacity: 1),
            [EnrollmentFor(1)],
            [WaitlistFor(2, DateTime.UtcNow.AddMinutes(-2)), WaitlistFor(3, DateTime.UtcNow.AddMinutes(-1))]);

        var result = await fixture.Service.DropLearnerAsync(DropRequest(learnerId: 1));

        Assert.True(result.Success);
        Assert.Equal(3, (await fixture.Db.Enrollments.SingleAsync()).LearnerId);
        Assert.True((await fixture.Db.WaitlistEntries.SingleAsync(w => w.LearnerId == 2)).IsActive);
        Assert.False((await fixture.Db.WaitlistEntries.SingleAsync(w => w.LearnerId == 3)).IsActive);
    }

    [Fact]
    public async Task Drop_WhenLearnerIsNotEnrolled_ReturnsFailureWithoutChangingState()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync([EligibleLearner(1)], Offering(capacity: 1));

        var result = await fixture.Service.DropLearnerAsync(DropRequest(learnerId: 1));

        Assert.False(result.Success);
        Assert.Empty(await fixture.Db.Enrollments.ToListAsync());
    }

    [Fact]
    public async Task Drop_WhenRepeated_ReturnsFailureAndDoesNotPromoteTwice()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync(
            [EligibleLearner(1), EligibleLearner(2)],
            Offering(capacity: 1),
            [EnrollmentFor(1)],
            [WaitlistFor(2, DateTime.UtcNow)]);

        var firstDrop = await fixture.Service.DropLearnerAsync(DropRequest(learnerId: 1));
        var repeatedDrop = await fixture.Service.DropLearnerAsync(DropRequest(learnerId: 1));

        Assert.True(firstDrop.Success);
        Assert.False(repeatedDrop.Success);
        Assert.Equal(1, await fixture.Db.Enrollments.CountAsync());
        Assert.Equal(2, (await fixture.Db.Enrollments.SingleAsync()).LearnerId);
    }

    [Fact]
    public async Task Promote_WhenNoEligibleCandidate_LeavesWaitlistUnchanged()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        await fixture.SeedAsync(
            [new Learner { Id = 1, Name = "Not eligible" }],
            Offering(capacity: 1),
            waitlist: [WaitlistFor(1, DateTime.UtcNow)]);

        var result = await fixture.Service.PromoteNextEligibleWaitlistAsync(1);

        Assert.False(result.Success);
        Assert.True((await fixture.Db.WaitlistEntries.SingleAsync()).IsActive);
        Assert.Empty(await fixture.Db.Enrollments.ToListAsync());
    }

    [Fact]
    public async Task Enroll_WhenOfferingDoesNotRequirePrerequisite_AllowsLearner()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var offering = Offering(capacity: 1);
        offering.RequiresPrerequisite = false;
        await fixture.SeedAsync([new Learner { Id = 1, Name = "No prerequisite" }], offering);

        var result = await fixture.Service.EnrollLearnerAsync(Request(learnerId: 1));

        Assert.True(result.Enrolled);
    }

    [Fact]
    public async Task ConcurrentEnrollments_ForSingleSeat_EnrollOneAndWaitlistTheOther()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"course-registration-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={databasePath};Default Timeout=10;Pooling=False";
        var options = new DbContextOptionsBuilder<CourseRegistrationDbContext>()
            .UseSqlite(connectionString)
            .Options;

        try
        {
            await using (var setupDb = new CourseRegistrationDbContext(options))
            {
                await setupDb.Database.EnsureCreatedAsync();
                setupDb.Learners.AddRange(EligibleLearner(1), EligibleLearner(2));
                setupDb.CourseOfferings.Add(Offering(capacity: 1));
                await setupDb.SaveChangesAsync();
            }

            await using (var firstDb = new CourseRegistrationDbContext(options))
            await using (var secondDb = new CourseRegistrationDbContext(options))
            {
                var firstService = CreateService(firstDb);
                var secondService = CreateService(secondDb);

                var results = await Task.WhenAll(
                    firstService.EnrollLearnerAsync(Request(learnerId: 1)),
                    secondService.EnrollLearnerAsync(Request(learnerId: 2)));

                Assert.Single(results, result => result.Enrolled);
                Assert.Single(results, result => result.Waitlisted);
                Assert.Equal(1, await firstDb.Enrollments.CountAsync());
                Assert.Equal(1, await firstDb.WaitlistEntries.CountAsync(w => w.IsActive));
            }
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    private static Learner EligibleLearner(int id) => new()
    {
        Id = id,
        Name = $"Learner {id}",
        HasRequiredPrerequisite = true
    };

    private static CourseOffering Offering(int capacity) => new()
    {
        Id = 1,
        CourseCode = "CS101",
        Capacity = capacity
    };

    private static Enrollment EnrollmentFor(int learnerId) => new()
    {
        LearnerId = learnerId,
        CourseOfferingId = 1
    };

    private static WaitlistEntry WaitlistFor(int learnerId, DateTime createdAt) => new()
    {
        LearnerId = learnerId,
        CourseOfferingId = 1,
        CreatedAtUtc = createdAt
    };

    private static EnrollLearnerRequest Request(int learnerId) => new()
    {
        LearnerId = learnerId,
        CourseOfferingId = 1
    };

    private static DropLearnerRequest DropRequest(int learnerId) => new()
    {
        LearnerId = learnerId,
        CourseOfferingId = 1
    };

    private sealed class DatabaseFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private DatabaseFixture(SqliteConnection connection, CourseRegistrationDbContext db)
        {
            _connection = connection;
            Db = db;
            Service = CreateService(db);
        }

        public CourseRegistrationDbContext Db { get; }
        public CourseRegistrationService Service { get; }

        public static async Task<DatabaseFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<CourseRegistrationDbContext>()
                .UseSqlite(connection)
                .Options;
            var db = new CourseRegistrationDbContext(options);
            await db.Database.EnsureCreatedAsync();
            return new DatabaseFixture(connection, db);
        }

        public async Task SeedAsync(
            Learner[] learners,
            CourseOffering offering,
            Enrollment[]? enrollments = null,
            WaitlistEntry[]? waitlist = null)
        {
            Db.Learners.AddRange(learners);
            Db.CourseOfferings.Add(offering);

            foreach (var enrollment in enrollments ?? [])
            {
                enrollment.Learner = learners.Single(l => l.Id == enrollment.LearnerId);
                enrollment.CourseOffering = offering;
                Db.Enrollments.Add(enrollment);
            }

            foreach (var entry in waitlist ?? [])
            {
                entry.Learner = learners.Single(l => l.Id == entry.LearnerId);
                entry.CourseOffering = offering;
                Db.WaitlistEntries.Add(entry);
            }

            await Db.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private static CourseRegistrationService CreateService(CourseRegistrationDbContext db)
    {
        return new CourseRegistrationService(
            new CourseOfferingRepository(db),
            new EnrollmentRepository(db),
            new WaitlistEntryRepository(db),
            new LearnerRepository(db),
            new RegistrationUnitOfWork(db));
    }
}
