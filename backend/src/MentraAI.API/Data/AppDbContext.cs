using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.Chat.Models;
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
    public DbSet<UserStageProgress> UserStageProgress { get; set; }

    // == Quizzes ============
    public DbSet<QuizAttempt> QuizAttempts { get; set; }

    // == Chat ===============
    public DbSet<Conversation> Conversations { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // == UserProfile ====
        b.Entity<UserProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.CurrentSkillsJson).HasColumnName("CurrentSkillsJson").HasColumnType("text");
            e.Property(x => x.FutureSkillsJson).HasColumnName("FutureSkillsJson").HasColumnType("text");
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

        // == OnboardingQuestion ===
        b.Entity<OnboardingQuestion>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.QuestionKey).IsUnique();
            e.Property(x => x.OptionsJson).HasColumnType("text");
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
            e.Property(x => x.Confidence).HasColumnType("numeric(5,4)");
            e.Property(x => x.PrimaryRoleName).HasMaxLength(500);
            e.Property(x => x.TopRolesJson).HasColumnType("text");
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
            e.Property(x => x.RoadmapDataJson).HasColumnType("text");
            e.HasOne(x => x.UserTrack)
                .WithMany()
                .HasForeignKey(x => x.UserTrackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // == UserStageProgress ===
        b.Entity<UserStageProgress>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RoadmapId, x.StageIndex }).IsUnique();
            e.HasIndex(x => x.RoadmapId);
            e.Property(x => x.ResourcesDataJson).HasColumnType("text");
            e.HasOne(x => x.Roadmap)
                .WithMany()
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
            e.Property(x => x.Score).HasColumnType("numeric(5,2)");
            e.Property(x => x.PassingScore).HasColumnType("numeric(18,2)");
            e.Property(x => x.QuestionsDataJson).HasColumnType("text");
            e.Property(x => x.UserAnswersDataJson).HasColumnType("text");
            e.HasOne(x => x.StageProgress)
                .WithMany()
                .HasForeignKey(x => x.StageProgressId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // == Conversation ===
        b.Entity<Conversation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.UserId, x.CreatedAt });
            e.Property(x => x.ConversationTitle).HasMaxLength(500);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // == Seed: CareerTracks ==

        // Use a unified UTC timestamp variable for seeding data safely in PostgreSQL
        var utcSeedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // == Seed: CareerTracks ==
        b.Entity<CareerTrack>().HasData(
            new CareerTrack { Id = 1, Name = "Frontend Engineering", Slug = "frontend-engineering", Description = "User interfaces, client-side logic, React, modern web development, and accessibility.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 2, Name = "Backend Engineering", Slug = "backend-engineering", Description = "Server-side logic, robust RESTful/gRPC APIs, microservices architectures, and advanced databases.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 3, Name = "Full-Stack Development", Slug = "full-stack-development", Description = "End-to-end applications engineering handling both scalable client-side and server-side components.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 4, Name = "Mobile Development (iOS / Android / Cross-platform)", Slug = "mobile-development", Description = "Native or cross-platform mobile apps using modern setups like Swift, Kotlin, Flutter, or React Native.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 5, Name = "DevOps / Site Reliability Engineering (SRE)", Slug = "devops-sre", Description = "Continuous integration, high availability systems, secure cloud delivery, and automation pipelines.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 6, Name = "Cloud Architecture / Cloud Engineering", Slug = "cloud-engineering", Description = "Designing, managing, and scaling distributed enterprise systems natively across multi-cloud structures.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 7, Name = "Data Engineering", Slug = "data-engineering", Description = "Constructing robust batch/streaming pipelines, data lakes, warehouses, and data fabric architectures.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 8, Name = "Data Science / Analytics", Slug = "data-science-analytics", Description = "Statistical computing, advanced metrics forecasting, predictive analytics, and deep business intelligence.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 9, Name = "Machine Learning Engineering", Slug = "machine-learning-engineering", Description = "Building, operationalizing, and fine-tuning advanced predictive models and convolutional architectures.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 10, Name = "MLOps / AI Infrastructure", Slug = "mlops-ai-infrastructure", Description = "Scaling neural network inference pipelines, managing feature stores, and automated model governance.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 11, Name = "Cybersecurity Engineering", Slug = "cybersecurity-engineering", Description = "Threat vector emulation, security operations management, application auditing, and defensive infrastructure.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 12, Name = "Embedded Systems / IoT", Slug = "embedded-systems-iot", Description = "Bare-metal or RTOS hardware systems programming, firmware optimization, and microcontrollers architectures.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 13, Name = "Game Development", Slug = "game-development", Description = "Interactive graphics engineering, computer physics simulations, and engine patterns using Unreal or Unity.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 14, Name = "Blockchain / Web3 Development", Slug = "blockchain-web3", Description = "Smart contracts protocol development, cryptographical consensus algorithms, and decentralized operations.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 15, Name = "Platform Engineering", Slug = "platform-engineering", Description = "Building automated Internal Developer Platforms (IDP) and scaling standard tool chains for product teams.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 16, Name = "QA / Test Automation Engineering", Slug = "qa-test-automation", Description = "Architecting end-to-end integration test suites, behavior testing, and continuous quality guards.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 17, Name = "Systems Programming", Slug = "systems-programming", Description = "Low-level resource managers, memory allocators, compilers infrastructure development using Rust, C, or C++.", IsActive = true, CreatedAt = utcSeedTime },
            new CareerTrack { Id = 18, Name = "AI / LLM Application Development", Slug = "ai-llm-application-development", Description = "Architecting agentic setups, prompt frameworks pipelines, vector stores indexing, and cognitive workflow flows.", IsActive = true, CreatedAt = utcSeedTime }
        );

        // == Seed: OnboardingQuestions ==
        b.Entity<OnboardingQuestion>().HasData(
            new OnboardingQuestion { Id = 1, QuestionKey = "Age", QuestionText = "What is your age range?", QuestionType = "Choice", OptionsJson = "[\"18-24 years old\", \"25-34 years old\", \"35-44 years old\", \"45-54 years old\", \"55-64 years old\", \"65 years or older\", \"Prefer not to say\"]", DisplayOrder = 1, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 2, QuestionKey = "EdLevel", QuestionText = "What is your highest education level achieved?", QuestionType = "Choice", OptionsJson = "[\"Primary/elementary school\", \"Secondary school\", \"Some college without degree\", \"Associate degree\", \"Bachelor's degree\", \"Master's degree\", \"Professional degree\", \"Other\"]", DisplayOrder = 2, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 3, QuestionKey = "YearsCode", QuestionText = "How many years of coding experience do you have?", QuestionType = "Number", OptionsJson = "[]", DisplayOrder = 3, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 4, QuestionKey = "WorkExp", QuestionText = "How many years of professional work experience do you have?", QuestionType = "Number", OptionsJson = "[]", DisplayOrder = 4, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 5, QuestionKey = "Employment", QuestionText = "What is your current employment status?", QuestionType = "Choice", OptionsJson = "[\"Employed\", \"Independent contractor, freelancer\", \"Student\", \"Not employed\", \"I prefer not to say\"]", DisplayOrder = 5, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 6, QuestionKey = "RemoteWork", QuestionText = "What is your work environment preference?", QuestionType = "Choice", OptionsJson = "[\"Remote\", \"Hybrid\", \"In-person\"]", DisplayOrder = 6, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 7, QuestionKey = "Industry", QuestionText = "What is your current or most recent industry?", QuestionType = "Choice", OptionsJson = "[\"Software Development\", \"Fintech\", \"Healthcare\", \"Retail\", \"Manufacturing\", \"Government\", \"Education\", \"Other\"]", DisplayOrder = 7, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 8, QuestionKey = "OrgSize", QuestionText = "What is the size of your organisation?", QuestionType = "Choice", OptionsJson = "[\"Just me\", \"Less than 20 employees\", \"20 to 99 employees\", \"100 to 499 employees\", \"500 to 999 employees\", \"1,000+ employees\"]", DisplayOrder = 8, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 9, QuestionKey = "AISelect", QuestionText = "How often do you use AI tools?", QuestionType = "Choice", OptionsJson = "[\"Yes, I use AI tools daily\", \"Yes, I use AI tools weekly\", \"No, and I don't plan to\"]", DisplayOrder = 9, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 10, QuestionKey = "current_skills", QuestionText = "What technologies and skills do you currently know?", QuestionType = "MultiSelect", OptionsJson = "[\"c#\", \"sql\", \"javascript\", \"python\", \"java\", \"html/css\", \"asp.net core\", \"docker\", \"react\", \"angular\", \"typescript\", \"node.js\", \"postgresql\", \"redis\", \"azure\", \"aws\", \"kubernetes\", \"git\"]", DisplayOrder = 10, IsActive = true, CreatedAt = utcSeedTime },
            new OnboardingQuestion { Id = 11, QuestionKey = "future_skills", QuestionText = "What technologies and skills do you want to learn?", QuestionType = "MultiSelect", OptionsJson = "[\"asp.net core\", \"docker\", \"kubernetes\", \"react\", \"angular\", \"next.js\", \"machine learning\", \"azure\", \"aws\", \"rust\", \"go\", \"swift\", \"kotlin\", \"flutter\", \"blockchain\"]", DisplayOrder = 11, IsActive = true, CreatedAt = utcSeedTime }
        );
    }
}