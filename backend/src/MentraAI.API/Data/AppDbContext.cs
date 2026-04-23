using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.Onboarding.Models;
using MentraAI.API.Modules.Quizzes.Models;
using MentraAI.API.Modules.Roadmaps.Models;
using MentraAI.API.Modules.StageProgress.Models;
using MentraAI.API.Modules.Users.Models;

namespace MentraAI.API.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // == Auth ===============
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // == Users ==============
    public DbSet<UserProfile> UserProfiles { get; set; }

    // == Onboarding =========
    public DbSet<OnboardingQuestion> OnboardingQuestions { get; set; }
    public DbSet<OnboardingAnswer> OnboardingAnswers { get; set; }

    // == Career Tracks ======
    public DbSet<CareerTrack> CareerTracks { get; set; }
    public DbSet<MLPrediction> MLPredictions { get; set; }
    public DbSet<UserTrack> UserTracks { get; set; }

    // == Roadmaps ===========
    public DbSet<Roadmap> Roadmaps { get; set; }

    // == Stage Progress =====
    public DbSet<UserStageProgress> UserStageProgresses { get; set; }

    // == Quizzes ============
    public DbSet<QuizAttempt> QuizAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // == UserProfile ====
        b.Entity<UserProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.CurrentSkillsJson)
                .HasColumnType("nvarchar(max)");
            e.Property(x => x.InterestsJson)
                .HasColumnType("nvarchar(max)");
        });

        // == RefreshToken ===
        b.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // == OnboardingQuestion ==========================================
        b.Entity<OnboardingQuestion>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.QuestionKey).IsUnique();
            e.Property(x => x.OptionsJson).HasColumnType("nvarchar(max)");
        });

        // == OnboardingAnswer =====
        b.Entity<OnboardingAnswer>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.QuestionId }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // == CareerTrack ====
        b.Entity<CareerTrack>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        // == MLPrediction ===
        b.Entity<MLPrediction>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId);
            e.Property(x => x.Confidence).HasColumnType("decimal(5,4)");
            e.Property(x => x.TopRolesJson).HasColumnType("nvarchar(max)");
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // == UserTrack ======
        b.Entity<UserTrack>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.CareerTrackId);
            e.HasIndex(x => new { x.UserId, x.IsActive });
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CareerTrack)
                .WithMany(c => c.UserTracks)
                .HasForeignKey(x => x.CareerTrackId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // == Roadmap ========
        b.Entity<Roadmap>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserTrackId, x.VersionNumber }).IsUnique();
            e.HasIndex(x => new { x.UserTrackId, x.IsActive });
            e.Property(x => x.RoadmapDataJson).HasColumnType("nvarchar(max)");
            e.HasOne(x => x.UserTrack)
                .WithMany()
                .HasForeignKey(x => x.UserTrackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // == UserStageProgress ===========================================
        b.Entity<UserStageProgress>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RoadmapId, x.StageIndex }).IsUnique();
            e.HasIndex(x => x.RoadmapId);
            e.Property(x => x.ResourcesDataJson).HasColumnType("nvarchar(max)");
            e.HasOne(x => x.Roadmap)
                .WithMany(r => r.Stages)
                .HasForeignKey(x => x.RoadmapId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // == QuizAttempt ====
        b.Entity<QuizAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.StageProgressId, x.AttemptNumber }).IsUnique();
            e.HasIndex(x => new { x.StageProgressId, x.IsSubmitted });
            e.HasIndex(x => new { x.UserId, x.GeneratedAt });
            e.Property(x => x.Score).HasColumnType("decimal(5,2)");
            e.Property(x => x.QuestionsDataJson).HasColumnType("nvarchar(max)");
            e.Property(x => x.UserAnswersDataJson).HasColumnType("nvarchar(max)");
            e.HasOne(x => x.StageProgress)
                .WithMany(s => s.QuizAttempts)
                .HasForeignKey(x => x.StageProgressId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // == Seed: CareerTracks ===============
        b.Entity<CareerTrack>().HasData(
            new CareerTrack { Id = 1, Name = "Data Science", Slug = "data-science", Description = "Machine learning, statistics, and data analysis.", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CareerTrack { Id = 2, Name = "Backend Developer", Slug = "backend-developer", Description = "APIs, databases, server-side logic.", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CareerTrack { Id = 3, Name = "Frontend Developer", Slug = "frontend-developer", Description = "React, CSS, UI/UX, and web interfaces.", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CareerTrack { Id = 4, Name = "DevOps Engineer", Slug = "devops-engineer", Description = "CI/CD, cloud infrastructure, and automation.", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new CareerTrack { Id = 5, Name = "Mobile Developer", Slug = "mobile-developer", Description = "iOS and Android development with Flutter or native.", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // == Seed: OnboardingQuestions ============
        b.Entity<OnboardingQuestion>().HasData(
            new OnboardingQuestion { Id = 1, QuestionKey = "background", QuestionText = "Describe your academic or professional background.", QuestionType = "TEXT", OptionsJson = null, DisplayOrder = 1, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new OnboardingQuestion { Id = 2, QuestionKey = "current_skills", QuestionText = "Which programming skills do you already have?", QuestionType = "MULTISELECT", OptionsJson = "[\"Python\",\"JavaScript\",\"SQL\",\"C#\",\"Java\",\"None\"]", DisplayOrder = 2, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new OnboardingQuestion { Id = 3, QuestionKey = "weekly_hours", QuestionText = "How many hours per week can you dedicate to learning?", QuestionType = "NUMBER", OptionsJson = null, DisplayOrder = 3, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new OnboardingQuestion { Id = 4, QuestionKey = "interests", QuestionText = "What areas of technology interest you most?", QuestionType = "MULTISELECT", OptionsJson = "[\"AI/ML\",\"Web Development\",\"Mobile\",\"Cloud\",\"Cybersecurity\",\"Data\"]", DisplayOrder = 4, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new OnboardingQuestion { Id = 5, QuestionKey = "career_goals", QuestionText = "What is your main career goal?", QuestionType = "TEXT", OptionsJson = null, DisplayOrder = 5, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}