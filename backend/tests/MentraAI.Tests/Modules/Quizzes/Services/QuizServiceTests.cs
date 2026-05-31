using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.Quizzes.DTOs.Requests;
using MentraAI.API.Modules.Quizzes.Models;
using MentraAI.API.Modules.Quizzes.Repositories;
using MentraAI.API.Modules.Quizzes.Services;
using MentraAI.API.Modules.Roadmaps.Models;
using MentraAI.API.Modules.Roadmaps.Services;
using MentraAI.API.Modules.StageProgress.Models;
using MentraAI.API.Modules.StageProgress.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace MentraAI.Tests.Modules.Quizzes.Services;

public class QuizServiceTests
{
    private readonly Mock<IQuizRepository> _quizRepoMock;
    private readonly Mock<IStageProgressRepository> _stageRepoMock;
    private readonly Mock<ICareerTrackRepository> _trackRepoMock;
    private readonly Mock<IAIGatewayService> _aiGatewayMock;
    private readonly Mock<IQuizScoringService> _scoringMock;
    private readonly Mock<IRoadmapService> _roadmapServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<QuizService>> _loggerMock;
    private readonly QuizService _sut;
    private readonly string _testUserId = "user-123";

    // Minimal roadmap JSON with topics so ExtractStageTopics can parse them
    private const string ValidRoadmapJson = """
        {
            "roadmap": {
                "data": {
                    "difficulty_level": "beginner",
                    "curriculum": {
                        "stages": [
                            {
                                "id": "stage_0",
                                "name": "Intro",
                                "topics": ["HTML", "CSS"],
                                "estimated_weeks": 2
                            }
                        ]
                    }
                }
            }
        }
        """;

    public QuizServiceTests()
    {
        _quizRepoMock = new Mock<IQuizRepository>();
        _stageRepoMock = new Mock<IStageProgressRepository>();
        _trackRepoMock = new Mock<ICareerTrackRepository>();
        _aiGatewayMock = new Mock<IAIGatewayService>();
        _scoringMock = new Mock<IQuizScoringService>();
        _roadmapServiceMock = new Mock<IRoadmapService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<QuizService>>();

        _sut = new QuizService(
            _quizRepoMock.Object,
            _stageRepoMock.Object,
            _trackRepoMock.Object,
            _aiGatewayMock.Object,
            _scoringMock.Object,
            _roadmapServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateQuizAsync_StageNotActive_ThrowsAppException()
    {
        var stageId = Guid.NewGuid();
        var userTrackId = 1;

        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress
            {
                Id = stageId,
                Status = "LOCKED",
                Roadmap = new Roadmap
                {
                    UserTrackId = userTrackId,
                    RoadmapDataJson = ValidRoadmapJson
                }
            });

        _trackRepoMock.Setup(r => r.GetActiveTrackByUserIdAsync(_testUserId))
            .ReturnsAsync(new UserTrack { Id = userTrackId });

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _sut.GenerateQuizAsync(stageId, _testUserId));
        Assert.Equal("STAGE_NOT_ACTIVE", ex.ErrorCode);
    }

    [Fact]
    public async Task GenerateQuizAsync_PendingQuizExists_ThrowsAppException()
    {
        var stageId = Guid.NewGuid();
        var userTrackId = 1;

        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress
            {
                Id = stageId,
                Status = "ACTIVE",
                Roadmap = new Roadmap
                {
                    UserTrackId = userTrackId,
                    RoadmapDataJson = ValidRoadmapJson
                }
            });

        _trackRepoMock.Setup(r => r.GetActiveTrackByUserIdAsync(_testUserId))
            .ReturnsAsync(new UserTrack { Id = userTrackId });

        _quizRepoMock.Setup(r => r.GetPendingByStageAsync(stageId))
            .ReturnsAsync(new QuizAttempt { Id = Guid.NewGuid() });

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _sut.GenerateQuizAsync(stageId, _testUserId));
        Assert.Equal("QUIZ_PENDING_EXISTS", ex.ErrorCode);
    }

    [Fact]
    public async Task GenerateQuizAsync_ValidRequest_PassesTopicsToAIAndPersistsMetadata()
    {
        var stageId = Guid.NewGuid();
        var userTrackId = 1;
        var careerTrackSlug = "frontend-developer";

        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress
            {
                Id = stageId,
                Status = "ACTIVE",
                StageIndex = 0,
                AiStageId = "stage_0",
                StageName = "Intro",
                Roadmap = new Roadmap
                {
                    UserTrackId = userTrackId,
                    RoadmapDataJson = ValidRoadmapJson
                }
            });

        _trackRepoMock.Setup(r => r.GetActiveTrackByUserIdAsync(_testUserId))
            .ReturnsAsync(new UserTrack
            {
                Id = userTrackId,
                CareerTrack = new CareerTrack { Slug = careerTrackSlug }
            });

        _quizRepoMock.Setup(r => r.GetPendingByStageAsync(stageId))
            .ReturnsAsync((QuizAttempt?)null);

        _quizRepoMock.Setup(r => r.GetNextAttemptNumberAsync(stageId))
            .ReturnsAsync(1);

        // topics param must be present — topics ["HTML","CSS"] extracted from JSON
        _aiGatewayMock.Setup(ai => ai.GenerateQuizAsync(
                _testUserId, careerTrackSlug, "stage_0", "Intro",
                It.IsAny<string>(),
                It.Is<List<string>>(t => t.Contains("HTML") && t.Contains("CSS")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizGenerationResult
            {
                QuestionsDataJson = "[{}]",
                TotalQuestions = 1,
                PassingScore = 70,
                TimeLimitMinutes = 20,
                Questions = new List<QuizQuestionDisplay>
                {
                    new() { Id = "q1", Text = "Q?", Choices = new() }
                }
            });

        QuizAttempt? capturedAttempt = null;
        _quizRepoMock.Setup(r => r.CreateAsync(It.IsAny<QuizAttempt>()))
            .Callback<QuizAttempt>(a => capturedAttempt = a)
            .ReturnsAsync((QuizAttempt a) => a);

        _mapperMock.Setup(m => m.Map<MentraAI.API.Modules.Quizzes.DTOs.Responses.QuizResponse>(
                It.IsAny<QuizAttempt>()))
            .Returns(new MentraAI.API.Modules.Quizzes.DTOs.Responses.QuizResponse());

        await _sut.GenerateQuizAsync(stageId, _testUserId);

        // Verify topics were passed to AI
        _aiGatewayMock.Verify(ai => ai.GenerateQuizAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<List<string>>(t => t.Contains("HTML")),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify PassingScore and TimeLimitMinutes were persisted correctly (int, not decimal)
        Assert.NotNull(capturedAttempt);
        Assert.Equal(70, capturedAttempt!.PassingScore);
        Assert.Equal(20, capturedAttempt!.TimeLimitMinutes);
    }

    [Fact]
    public async Task SubmitQuizAsync_AlreadySubmitted_ThrowsAppException()
    {
        var quizId = Guid.NewGuid();
        _quizRepoMock.Setup(r => r.GetByIdAsync(quizId))
            .ReturnsAsync(new QuizAttempt
            {
                Id = quizId,
                UserId = _testUserId,
                IsSubmitted = true
            });

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _sut.SubmitQuizAsync(quizId, _testUserId, new SubmitQuizRequest()));
        Assert.Equal("QUIZ_ALREADY_SUBMITTED", ex.ErrorCode);
    }

    [Fact]
    public async Task SubmitQuizAsync_Passed_CompletesStageAndUnlocksNext()
    {
        var quizId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var request = new SubmitQuizRequest { Answers = new List<QuizAnswerItem>() };

        _quizRepoMock.Setup(r => r.GetByIdAsync(quizId))
            .ReturnsAsync(new QuizAttempt
            {
                Id = quizId,
                UserId = _testUserId,
                IsSubmitted = false,
                StageProgressId = stageId
            });

        _scoringMock.Setup(s => s.Score(It.IsAny<string>(), request.Answers, It.IsAny<decimal>()))
            .Returns(new QuizScoreResult(4, 4, 100, true));

        _quizRepoMock.Setup(r => r.SubmitAsync(quizId, It.IsAny<string>(), 4, 100, true))
            .ReturnsAsync(new QuizAttempt { SubmittedAt = DateTime.UtcNow });

        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress
            {
                Id = stageId,
                RoadmapId = 1,
                StageIndex = 0
            });

        _stageRepoMock.Setup(r => r.UnlockNextStageAsync(1, 0))
            .ReturnsAsync(new UserStageProgress
            {
                Id = Guid.NewGuid(),
                StageName = "Stage 2",
                StageIndex = 1
            });

        var result = await _sut.SubmitQuizAsync(quizId, _testUserId, request);

        Assert.True(result.IsPassed);
        Assert.NotNull(result.NextStage);
        _stageRepoMock.Verify(r => r.CompleteStageAsync(stageId), Times.Once);
        _stageRepoMock.Verify(r => r.UnlockNextStageAsync(1, 0), Times.Once);
        
        _aiGatewayMock.Verify(
            ai => ai.GetAdaptedRoadmapAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<FailedQuestion>>(), It.IsAny<decimal>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitQuizAsync_Failed_TriggersAdaptation()
    {
        var quizId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var request = new SubmitQuizRequest { Answers = new List<QuizAnswerItem>() };

        _quizRepoMock.Setup(r => r.GetByIdAsync(quizId))
            .ReturnsAsync(new QuizAttempt
            {
                Id = quizId,
                UserId = _testUserId,
                IsSubmitted = false,
                StageProgressId = stageId,
                QuestionsDataJson = "[]"
            });

        _scoringMock.Setup(s => s.Score(It.IsAny<string>(), request.Answers, It.IsAny<decimal>()))
            .Returns(new QuizScoreResult(0, 4, 0, false));

        _quizRepoMock.Setup(r => r.SubmitAsync(quizId, It.IsAny<string>(), 0, 0, false))
            .ReturnsAsync(new QuizAttempt { SubmittedAt = DateTime.UtcNow });

        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress
            {
                Id = stageId,
                StageName = "Intro",
                AiStageId = "stage_0",
                Roadmap = new Roadmap
                {
                    RoadmapDataJson = ValidRoadmapJson
                }
            });

        _trackRepoMock.Setup(r => r.GetActiveTrackByUserIdAsync(_testUserId))
            .ReturnsAsync(new UserTrack
            {
                CareerTrack = new CareerTrack { Slug = "frontend" }
            });

        _aiGatewayMock.Setup(ai => ai.GetAdaptedRoadmapAsync(
                _testUserId, "frontend", "stage_0", "Intro",
                It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<FailedQuestion>>(), 0,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdaptationResult { RemediationResourcesJson = "{}" });

        var result = await _sut.SubmitQuizAsync(quizId, _testUserId, request);

        Assert.False(result.IsPassed);
        Assert.True(result.RoadmapAdapted);
        _stageRepoMock.Verify(r => r.CompleteStageAsync(It.IsAny<Guid>()), Times.Never);
        _stageRepoMock.Verify(r => r.PatchResourcesAsync(stageId, It.IsAny<string>()), Times.Once);
    }
}

