using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.CareerTracks.Services;
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
}
