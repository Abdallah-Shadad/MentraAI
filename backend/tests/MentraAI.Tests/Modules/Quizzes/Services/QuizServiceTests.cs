using MentraAI.API.Common.Exceptions;
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
    private readonly Mock<ILogger<QuizService>> _loggerMock;
    private readonly QuizService _sut;

    private readonly string _testUserId = "user-123";

    public QuizServiceTests()
    {
        _quizRepoMock = new Mock<IQuizRepository>();
        _stageRepoMock = new Mock<IStageProgressRepository>();
        _trackRepoMock = new Mock<ICareerTrackRepository>();
        _aiGatewayMock = new Mock<IAIGatewayService>();
        _scoringMock = new Mock<IQuizScoringService>();
        _roadmapServiceMock = new Mock<IRoadmapService>();
        _loggerMock = new Mock<ILogger<QuizService>>();

        _sut = new QuizService(
            _quizRepoMock.Object,
            _stageRepoMock.Object,
            _trackRepoMock.Object,
            _aiGatewayMock.Object,
            _scoringMock.Object,
            _roadmapServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateQuizAsync_StageNotActive_ThrowsAppException()
    {
        // Arrange
        var stageId = Guid.NewGuid();
        var userTrackId = 1;
        
        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress 
            { 
                Id = stageId, 
                Status = "LOCKED",
                Roadmap = new Roadmap { UserTrackId = userTrackId }
            });

        _trackRepoMock.Setup(r => r.GetActiveTrackByUserIdAsync(_testUserId))
            .ReturnsAsync(new UserTrack { Id = userTrackId });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.GenerateQuizAsync(stageId, _testUserId));
        Assert.Equal("STAGE_NOT_ACTIVE", ex.ErrorCode);
    }

    [Fact]
    public async Task GenerateQuizAsync_PendingQuizExists_ThrowsAppException()
    {
        // Arrange
        var stageId = Guid.NewGuid();
        var userTrackId = 1;
        
        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress 
            { 
                Id = stageId, 
                Status = "ACTIVE",
                Roadmap = new Roadmap { UserTrackId = userTrackId }
            });

        _trackRepoMock.Setup(r => r.GetActiveTrackByUserIdAsync(_testUserId))
            .ReturnsAsync(new UserTrack { Id = userTrackId });

        _quizRepoMock.Setup(r => r.GetPendingByStageAsync(stageId))
            .ReturnsAsync(new QuizAttempt { Id = Guid.NewGuid() }); // Pending exists

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.GenerateQuizAsync(stageId, _testUserId));
        Assert.Equal("QUIZ_PENDING_EXISTS", ex.ErrorCode);
    }

    [Fact]
    public async Task GenerateQuizAsync_ValidRequest_CallsAIAndCreatesQuiz()
    {
        // Arrange
        var stageId = Guid.NewGuid();
        var userTrackId = 1;
        var careerTrackSlug = "backend";
        
        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress 
            { 
                Id = stageId, 
                Status = "ACTIVE",
                AiStageId = "ai-123",
                StageName = "Stage 1",
                Roadmap = new Roadmap { UserTrackId = userTrackId }
            });

        _trackRepoMock.Setup(r => r.GetActiveTrackByUserIdAsync(_testUserId))
            .ReturnsAsync(new UserTrack 
            { 
                Id = userTrackId,
                CareerTrack = new CareerTrack { Slug = careerTrackSlug }
            });

        _quizRepoMock.Setup(r => r.GetPendingByStageAsync(stageId))
            .ReturnsAsync((QuizAttempt?)null);

        _aiGatewayMock.Setup(ai => ai.GenerateQuizAsync(_testUserId, careerTrackSlug, "ai-123", "Stage 1", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizGenerationResult 
            {
                QuestionsDataJson = "[{}]",
                TotalQuestions = 1,
                Questions = new List<QuizQuestionDisplay> { new() { Id = "q1" } }
            });

        _quizRepoMock.Setup(r => r.CreateAsync(It.IsAny<QuizAttempt>()))
            .ReturnsAsync(new QuizAttempt { Id = Guid.NewGuid() });

        // Act
        var result = await _sut.GenerateQuizAsync(stageId, _testUserId);

        // Assert
        Assert.NotNull(result);
        _aiGatewayMock.Verify(ai => ai.GenerateQuizAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _quizRepoMock.Verify(r => r.CreateAsync(It.IsAny<QuizAttempt>()), Times.Once);
    }

    [Fact]
    public async Task SubmitQuizAsync_AlreadySubmitted_ThrowsAppException()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        _quizRepoMock.Setup(r => r.GetByIdAsync(quizId))
            .ReturnsAsync(new QuizAttempt { Id = quizId, UserId = _testUserId, IsSubmitted = true });

        var request = new SubmitQuizRequest();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _sut.SubmitQuizAsync(quizId, _testUserId, request));
        Assert.Equal("QUIZ_ALREADY_SUBMITTED", ex.ErrorCode);
    }

    [Fact]
    public async Task SubmitQuizAsync_Passed_CompletesStageAndUnlocksNext()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var request = new SubmitQuizRequest { Answers = new List<QuizAnswerItem>() };

        _quizRepoMock.Setup(r => r.GetByIdAsync(quizId))
            .ReturnsAsync(new QuizAttempt { Id = quizId, UserId = _testUserId, IsSubmitted = false, StageProgressId = stageId });

        _scoringMock.Setup(s => s.Score(It.IsAny<string>(), request.Answers))
            .Returns(new QuizScoreResult(1, 1, 100, true)); // Passed

        _quizRepoMock.Setup(r => r.SubmitAsync(quizId, It.IsAny<string>(), 1, 100, true))
            .ReturnsAsync(new QuizAttempt { SubmittedAt = DateTime.UtcNow });

        _stageRepoMock.Setup(r => r.GetByIdAsync(stageId))
            .ReturnsAsync(new UserStageProgress { Id = stageId, RoadmapId = 1, StageIndex = 0 });

        _stageRepoMock.Setup(r => r.UnlockNextStageAsync(1, 0))
            .ReturnsAsync(new UserStageProgress { Id = Guid.NewGuid(), StageName = "Stage 2", StageIndex = 1 });

        // Act
        var result = await _sut.SubmitQuizAsync(quizId, _testUserId, request);

        // Assert
        Assert.True(result.IsPassed);
        Assert.NotNull(result.NextStage);
        _stageRepoMock.Verify(r => r.CompleteStageAsync(stageId), Times.Once);
        _stageRepoMock.Verify(r => r.UnlockNextStageAsync(1, 0), Times.Once);
        _roadmapServiceMock.Verify(r => r.AdaptRoadmapAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SubmitQuizAsync_Failed_TriggersAdaptation()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var request = new SubmitQuizRequest { Answers = new List<QuizAnswerItem>() };

        _quizRepoMock.Setup(r => r.GetByIdAsync(quizId))
            .ReturnsAsync(new QuizAttempt { Id = quizId, UserId = _testUserId, IsSubmitted = false, StageProgressId = stageId });

        _scoringMock.Setup(s => s.Score(It.IsAny<string>(), request.Answers))
            .Returns(new QuizScoreResult(0, 1, 0, false)); // Failed

        _quizRepoMock.Setup(r => r.SubmitAsync(quizId, It.IsAny<string>(), 0, 0, false))
            .ReturnsAsync(new QuizAttempt { SubmittedAt = DateTime.UtcNow });

        _roadmapServiceMock.Setup(r => r.AdaptRoadmapAsync(stageId, It.IsAny<string>(), It.IsAny<string>(), 0, _testUserId))
            .ReturnsAsync(new Roadmap());

        // Act
        var result = await _sut.SubmitQuizAsync(quizId, _testUserId, request);

        // Assert
        Assert.False(result.IsPassed);
        Assert.True(result.RoadmapAdapted);
        _stageRepoMock.Verify(r => r.CompleteStageAsync(It.IsAny<Guid>()), Times.Never);
        _roadmapServiceMock.Verify(r => r.AdaptRoadmapAsync(stageId, It.IsAny<string>(), It.IsAny<string>(), 0, _testUserId), Times.Once);
    }
}
