using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace MentraAI.Tests.Modules.AIGateway.Services;

public class AIGatewayServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<AIGatewayService>> _loggerMock;
    private readonly AIGatewayService _sut;
    private readonly string _testUserId = "user-123";

    public AIGatewayServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://test-ai-service.com")
        };
        _loggerMock = new Mock<ILogger<AIGatewayService>>();
        _sut = new AIGatewayService(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTrackRecommendationsAsync_Success_ReturnsRecommendations()
    {
        // Arrange
        var mockResponse = new
        {
            signal = "201_Created",
            recommendations = new
            {
                status = "success",
                data = new
                {
                    recommendations = new
                    {
                        user_summary = "High potential backend engineer.",
                        primary_recommendation = "Backend Developer",
                        profile_completeness = 85,
                        missing_info_suggestions = new List<string> { "Add more years of work experience." },
                        recommended_tracks = new List<object>
                        {
                            new
                            {
                                track_name = "Backend Developer",
                                fit_score = 95,
                                reasoning = "Strong programming base.",
                                skill_overlap = new List<string> { "C#", "SQL" },
                                skills_to_learn = new List<string> { "Kubernetes" },
                                estimated_transition_weeks = 8
                            }
                        }
                    }
                }
            }
        };

        var responseJson = JsonSerializer.Serialize(mockResponse);

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.PathAndQuery == "/api/v1/tracks/recommend"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            });

        var profile = new TrackRecommendProfile
        {
            Age = "25",
            CurrentSkills = new List<string> { "C#", "SQL" }
        };

        // Act
        var result = await _sut.GetTrackRecommendationsAsync(_testUserId, profile);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Backend Developer", result.PrimaryRecommendation);
        Assert.Equal("High potential backend engineer.", result.UserSummary);
        Assert.Equal(85, result.ProfileCompleteness);
        Assert.Single(result.MissingInfoSuggestions);
        Assert.Single(result.RecommendedTracks);
        Assert.Equal("Backend Developer", result.RecommendedTracks[0].TrackName);
        Assert.Equal(95, result.RecommendedTracks[0].FitScore);
    }

    [Fact]
    public async Task GetTrackRecommendationsAsync_HttpError_ThrowsAIServiceException()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("AI Graph Generation failed.")
            });

        var profile = new TrackRecommendProfile { Age = "30" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AIServiceException>(
            () => _sut.GetTrackRecommendationsAsync(_testUserId, profile));

        Assert.Contains("Track recommender returned 500", ex.Message);
    }

    [Fact]
    public async Task GetTrackRecommendationsAsync_Timeout_ThrowsOperationCanceledException()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new OperationCanceledException("The operation was canceled."));

        var profile = new TrackRecommendProfile { Age = "30" };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.GetTrackRecommendationsAsync(_testUserId, profile));
    }
}
