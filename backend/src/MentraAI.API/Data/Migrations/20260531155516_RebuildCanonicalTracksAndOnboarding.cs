using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MentraAI.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RebuildCanonicalTracksAndOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PrimaryRoleName",
                table: "MLPredictions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "User interfaces, client-side logic, React, modern web development, and accessibility.", "Frontend Engineering", "frontend-engineering" });

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Server-side logic, robust RESTful/gRPC APIs, microservices architectures, and advanced databases.", "Backend Engineering", "backend-engineering" });

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "End-to-end applications engineering handling both scalable client-side and server-side components.", "Full-Stack Development", "full-stack-development" });

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Native or cross-platform mobile apps using modern setups like Swift, Kotlin, Flutter, or React Native.", "Mobile Development (iOS / Android / Cross-platform)", "mobile-development" });

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Continuous integration, high availability systems, secure cloud delivery, and automation pipelines.", "DevOps / Site Reliability Engineering (SRE)", "devops-sre" });

            migrationBuilder.InsertData(
                table: "CareerTracks",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "Slug" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Designing, managing, and scaling distributed enterprise systems natively across multi-cloud structures.", true, "Cloud Architecture / Cloud Engineering", "cloud-engineering" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Constructing robust batch/streaming pipelines, data lakes, warehouses, and data fabric architectures.", true, "Data Engineering", "data-engineering" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Statistical computing, advanced metrics forecasting, predictive analytics, and deep business intelligence.", true, "Data Science / Analytics", "data-science-analytics" },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Building, operationalizing, and fine-tuning advanced predictive models and convolutional architectures.", true, "Machine Learning Engineering", "machine-learning-engineering" },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Scaling neural network inference pipelines, managing feature stores, and automated model governance.", true, "MLOps / AI Infrastructure", "mlops-ai-infrastructure" },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Threat vector emulation, security operations management, application auditing, and defensive infrastructure.", true, "Cybersecurity Engineering", "cybersecurity-engineering" },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bare-metal or RTOS hardware systems programming, firmware optimization, and microcontrollers architectures.", true, "Embedded Systems / IoT", "embedded-systems-iot" },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Interactive graphics engineering, computer physics simulations, and engine patterns using Unreal or Unity.", true, "Game Development", "game-development" },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Smart contracts protocol development, cryptographical consensus algorithms, and decentralized operations.", true, "Blockchain / Web3 Development", "blockchain-web3" },
                    { 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Building automated Internal Developer Platforms (IDP) and scaling standard tool chains for product teams.", true, "Platform Engineering", "platform-engineering" },
                    { 16, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Architecting end-to-end integration test suites, behavior testing, and continuous quality guards.", true, "QA / Test Automation Engineering", "qa-test-automation" },
                    { 17, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Low-level resource managers, memory allocators, compilers infrastructure development using Rust, C, or C++.", true, "Systems Programming", "systems-programming" },
                    { 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Architecting agentic setups, prompt frameworks pipelines, vector stores indexing, and cognitive workflow flows.", true, "AI / LLM Application Development", "ai-llm-application-development" }
                });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[\"18-24 years old\", \"25-34 years old\", \"35-44 years old\", \"45-54 years old\", \"55-64 years old\", \"65 years or older\", \"Prefer not to say\"]", "Age", "What is your age range?", "Choice" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[\"Primary/elementary school\", \"Secondary school\", \"Some college without degree\", \"Associate degree\", \"Bachelor''s degree\", \"Master''s degree\", \"Professional degree\", \"Other\"]", "EdLevel", "What is your highest education level achieved?", "Choice" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[]", "YearsCode", "How many years of coding experience do you have?", "Number" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[]", "WorkExp", "How many years of professional work experience do you have?", "Number" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[\"Employed\", \"Independent contractor, freelancer\", \"Student\", \"Not employed\", \"I prefer not to say\"]", "Employment", "What is your current employment status?", "Choice" });

            migrationBuilder.InsertData(
                table: "OnboardingQuestions",
                columns: new[] { "Id", "CreatedAt", "DisplayOrder", "IsActive", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, true, "[\"Remote\", \"Hybrid\", \"In-person\"]", "RemoteWork", "What is your work environment preference?", "Choice" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, true, "[\"Software Development\", \"Fintech\", \"Healthcare\", \"Retail\", \"Manufacturing\", \"Government\", \"Education\", \"Other\"]", "Industry", "What is your current or most recent industry?", "Choice" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, true, "[\"Just me\", \"Less than 20 employees\", \"20 to 99 employees\", \"100 to 499 employees\", \"500 to 999 employees\", \"1,000+ employees\"]", "OrgSize", "What is the size of your organisation?", "Choice" },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, true, "[\"Yes, I use AI tools daily\", \"Yes, I use AI tools weekly\", \"No, and I don''t plan to\"]", "AISelect", "How often do you use AI tools?", "Choice" },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, true, "[\"c#\", \"sql\", \"javascript\", \"python\", \"java\", \"html/css\"]", "current_skills", "What technologies and skills do you currently know?", "MultiSelect" },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, true, "[\"asp.net core\", \"docker\", \"kubernetes\", \"react\", \"angular\", \"next.js\"]", "future_skills", "What technologies and skills do you want to learn?", "MultiSelect" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryRoleName",
                table: "MLPredictions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Machine learning, statistics, and data analysis.", "Data Science", "data-science" });

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "APIs, databases, server-side logic.", "Backend Developer", "backend-developer" });

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "React, CSS, UI/UX, and web interfaces.", "Frontend Developer", "frontend-developer" });

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CI/CD, cloud infrastructure, and automation.", "DevOps Engineer", "devops-engineer" });

            migrationBuilder.UpdateData(
                table: "CareerTracks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Description", "Name", "Slug" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "iOS and Android development with Flutter or native.", "Mobile Developer", "mobile-developer" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "background", "Describe your academic or professional background.", "TEXT" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[\"Python\",\"JavaScript\",\"SQL\",\"C#\",\"Java\",\"None\"]", "current_skills", "Which programming skills do you already have?", "MULTISELECT" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "weekly_hours", "How many hours per week can you dedicate to learning?", "NUMBER" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[\"AI/ML\",\"Web Development\",\"Mobile\",\"Cloud\",\"Cybersecurity\",\"Data\"]", "interests", "What areas of technology interest you most?", "MULTISELECT" });

            migrationBuilder.UpdateData(
                table: "OnboardingQuestions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "OptionsJson", "QuestionKey", "QuestionText", "QuestionType" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "career_goals", "What is your main career goal?", "TEXT" });
        }
    }
}
