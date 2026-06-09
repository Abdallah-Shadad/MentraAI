using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.DTOs.Responses;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.CareerTracks.Services;
using MentraAI.API.Modules.Users.DTOs.Responses;
using MentraAI.API.Modules.Users.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MentraAI.Tests.Modules.CareerTracks.Services;

public class CareerTrackServiceTests
{
    private readonly Mock<ICareerTrackRepository> _repoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IAIGatewayService> _aiGatewayMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<CareerTrackService>> _loggerMock;
    private readonly CareerTrackService _sut;
    private readonly string _testUserId = "user-123";

    public CareerTrackServiceTests()
    {
        _repoMock = new Mock<ICareerTrackRepository>();
        _mapperMock = new Mock<IMapper>();
        _aiGatewayMock = new Mock<IAIGatewayService>();
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<CareerTrackService>>();

        _sut = new CareerTrackService(
            _repoMock.Object,
            _mapperMock.Object,
            _aiGatewayMock.Object,
            _userServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetPredictionAsync_UserNotOnboarded_ThrowsAppException()
    {
        // Arrange
        _userServiceMock.Setup(u => u.GetIsOnboardedAsync(_testUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(
            () => _sut.GetPredictionAsync(_testUserId));

        Assert.Equal(ErrorCodes.NOT_ONBOARDED, ex.ErrorCode);
        Assert.Equal(422, ex.StatusCode);
    }

    [Fact]
    public async Task GetPredictionAsync_UserOnboardedNoPrediction_ReturnsDefaultPrediction()
    {
        // Arrange
        _userServiceMock.Setup(u => u.GetIsOnboardedAsync(_testUserId))
            .ReturnsAsync(true);

        _repoMock.Setup(r => r.GetLatestPredictionByUserIdAsync(_testUserId))
            .ReturnsAsync((MLPrediction?)null);

        // Act
        var result = await _sut.GetPredictionAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ready for Recommendation", result.PrimaryRole.Name);
        Assert.Equal(0, result.PrimaryRole.Confidence);
        Assert.Empty(result.TopRoles);
        Assert.True((DateTime.UtcNow - result.PredictedAt).TotalSeconds < 5);
    }

    [Fact]
    public async Task GetPredictionAsync_UserOnboardedWithPrediction_ReturnsMappedPrediction()
    {
        // Arrange
        _userServiceMock.Setup(u => u.GetIsOnboardedAsync(_testUserId))
            .ReturnsAsync(true);

        var prediction = new MLPrediction
        {
            UserId = _testUserId,
            PrimaryRoleName = "Backend Developer",
            Confidence = 0.95m,
            TopRolesJson = "[{\"name\":\"Backend Developer\",\"confidence\":95},{\"name\":\"Frontend Developer\",\"confidence\":60}]",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _repoMock.Setup(r => r.GetLatestPredictionByUserIdAsync(_testUserId))
            .ReturnsAsync(prediction);

        // Act
        var result = await _sut.GetPredictionAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Backend Developer", result.PrimaryRole.Name);
        Assert.Equal(0.95m, result.PrimaryRole.Confidence);
        Assert.Equal(2, result.TopRoles.Count);
        Assert.Equal("Backend Developer", result.TopRoles[0].Name);
        Assert.Equal(95, result.TopRoles[0].Confidence);
        Assert.Equal("Frontend Developer", result.TopRoles[1].Name);
        Assert.Equal(60, result.TopRoles[1].Confidence);
        Assert.Equal(prediction.CreatedAt, result.PredictedAt);
    }

    [Fact]
    public async Task GetRecommendationsAsync_UserNotOnboarded_ThrowsAppException()
    {
        // Arrange
        _userServiceMock.Setup(u => u.GetIsOnboardedAsync(_testUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(
            () => _sut.GetRecommendationsAsync(_testUserId));

        Assert.Equal(ErrorCodes.NOT_ONBOARDED, ex.ErrorCode);
        Assert.Equal(422, ex.StatusCode);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ValidRequest_MapsCareerTrackIdSuccessfully()
    {
        // Arrange
        _userServiceMock.Setup(u => u.GetIsOnboardedAsync(_testUserId))
            .ReturnsAsync(true);

        var userProfile = new UserProfileResponse
        {
            UserId = _testUserId,
            Age = "25",
            EdLevel = "Bachelor",
            CurrentSkills = new List<string> { "C#", "SQL" },
            FutureSkills = new List<string> { "Cloud" }
        };

        _userServiceMock.Setup(u => u.GetProfileAsync(_testUserId))
            .ReturnsAsync(userProfile);

        var aiResult = new TrackRecommendationResult
        {
            UserSummary = "High potential",
            PrimaryRecommendation = "Backend Engineering",
            ProfileCompleteness = 90,
            MissingInfoSuggestions = new List<string> { "Add certification" },
            RecommendedTracks = new List<TrackMatch>
            {
                new TrackMatch
                {
                    TrackName = "Backend Engineering",
                    FitScore = 95,
                    Reasoning = "Good backend logic",
                    SkillOverlap = new List<string> { "C#" },
                    SkillsToLearn = new List<string> { "Kubernetes" },
                    EstimatedTransitionWeeks = 8
                },
                new TrackMatch
                {
                    TrackName = "Frontend Engineering",
                    FitScore = 60,
                    Reasoning = "Needs JS",
                    SkillOverlap = new List<string>(),
                    SkillsToLearn = new List<string> { "React" },
                    EstimatedTransitionWeeks = 12
                }
            }
        };

        _aiGatewayMock.Setup(a => a.GetTrackRecommendationsAsync(_testUserId, It.IsAny<TrackRecommendProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResult);

        var activeTracks = new List<CareerTrack>
        {
            new CareerTrack { Id = 101, Name = "Backend Engineering" },
            new CareerTrack { Id = 102, Name = "frontend engineering" } // Case-insensitive matching check
        };

        _repoMock.Setup(r => r.GetAllActiveTracksAsync())
            .ReturnsAsync(activeTracks);

        // Act
        var result = await _sut.GetRecommendationsAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("High potential", result.UserSummary);
        Assert.Equal("Backend Engineering", result.PrimaryRecommendation);
        Assert.Equal(90, result.ProfileCompleteness);
        Assert.Equal("Add certification", result.MissingInfoSuggestions.Single());
        Assert.Equal(2, result.RecommendedTracks.Count);

        var backendTrack = result.RecommendedTracks.First(t => t.TrackName == "Backend Engineering");
        Assert.Equal(101, backendTrack.CareerTrackId);
        Assert.Equal(95, backendTrack.FitScore);
        Assert.Equal("Good backend logic", backendTrack.Reasoning);
        Assert.Equal("C#", backendTrack.SkillOverlap.Single());
        Assert.Equal("Kubernetes", backendTrack.SkillsToLearn.Single());
        Assert.Equal(8, backendTrack.EstimatedTransitionWeeks);

        var frontendTrack = result.RecommendedTracks.First(t => t.TrackName == "Frontend Engineering");
        Assert.Equal(102, frontendTrack.CareerTrackId); // Mapped using case-insensitive comparison
        Assert.Equal(60, frontendTrack.FitScore);

        _repoMock.Verify(r => r.SavePredictionAsync(_testUserId, It.Is<PredictionResult>(p =>
            p.PrimaryRoleName == "Backend Engineering" &&
            p.PrimaryConfidence == 0.95m
        )), Times.Once);
    }

    [Fact]
    public async Task GetRecommendationsAsync_TrackNameMismatch_LeavesIdNullAndLogsWarning()
    {
        // Arrange
        _userServiceMock.Setup(u => u.GetIsOnboardedAsync(_testUserId))
            .ReturnsAsync(true);

        var userProfile = new UserProfileResponse
        {
            UserId = _testUserId
        };

        _userServiceMock.Setup(u => u.GetProfileAsync(_testUserId))
            .ReturnsAsync(userProfile);

        var aiResult = new TrackRecommendationResult
        {
            RecommendedTracks = new List<TrackMatch>
            {
                new TrackMatch
                {
                    TrackName = "Unknown Engineering",
                    FitScore = 80
                }
            }
        };

        _aiGatewayMock.Setup(a => a.GetTrackRecommendationsAsync(_testUserId, It.IsAny<TrackRecommendProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResult);

        var activeTracks = new List<CareerTrack>
        {
            new CareerTrack { Id = 101, Name = "Backend Engineering" }
        };

        _repoMock.Setup(r => r.GetAllActiveTracksAsync())
            .ReturnsAsync(activeTracks);

        // Act
        var result = await _sut.GetRecommendationsAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.RecommendedTracks);
        var track = result.RecommendedTracks.Single();
        Assert.Equal("Unknown Engineering", track.TrackName);
        Assert.Null(track.CareerTrackId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("does not match")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
