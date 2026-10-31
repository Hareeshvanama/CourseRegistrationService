namespace CourseRegistration.Infrastructure.Data;

using CourseRegistration.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class CourseRegistrationDbContext : DbContext
{
    public CourseRegistrationDbContext(DbContextOptions<CourseRegistrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Learner> Learners => Set<Learner>();
    public DbSet<CourseOffering> CourseOfferings => Set<CourseOffering>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseOffering>()
            .ToTable(table => table.HasCheckConstraint("CK_CourseOffering_Capacity_NonNegative", "Capacity >= 0"));

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.LearnerId, e.CourseOfferingId })
            .IsUnique();

        modelBuilder.Entity<WaitlistEntry>()
            .HasIndex(w => new { w.LearnerId, w.CourseOfferingId })
            .IsUnique()
            .HasFilter("\"IsActive\" = 1");

        modelBuilder.Entity<WaitlistEntry>()
            .HasIndex(w => new { w.CourseOfferingId, w.CreatedAtUtc, w.Id });

        modelBuilder.Entity<Learner>()
            .HasMany(l => l.Enrollments)
            .WithOne(e => e.Learner)
            .HasForeignKey(e => e.LearnerId);

        modelBuilder.Entity<CourseOffering>()
            .HasMany(c => c.Enrollments)
            .WithOne(e => e.CourseOffering)
            .HasForeignKey(e => e.CourseOfferingId);

        modelBuilder.Entity<Learner>()
            .HasMany(l => l.WaitlistEntries)
            .WithOne(w => w.Learner)
            .HasForeignKey(w => w.LearnerId);

        modelBuilder.Entity<CourseOffering>()
            .HasMany(c => c.WaitlistEntries)
            .WithOne(w => w.CourseOffering)
            .HasForeignKey(w => w.CourseOfferingId);

        base.OnModelCreating(modelBuilder);
    }
}
