using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.Quizzes.Controllers;
using MentraAI.API.Modules.Quizzes.DTOs.Requests;
using MentraAI.API.Modules.Quizzes.DTOs.Responses;
using MentraAI.API.Modules.Quizzes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MentraAI.Tests.Modules.Quizzes.Controllers;

public class QuizControllerTests
{
    private readonly Mock<IQuizService> _quizServiceMock;
    private readonly Mock<IValidator<GenerateQuizRequest>> _generateValidatorMock;
    private readonly Mock<IValidator<SubmitQuizRequest>> _submitValidatorMock;
    private readonly QuizController _sut;
    private readonly string _testUserId = "user-123";

    public QuizControllerTests()
    {
        _quizServiceMock = new Mock<IQuizService>();
        _generateValidatorMock = new Mock<IValidator<GenerateQuizRequest>>();
        _submitValidatorMock = new Mock<IValidator<SubmitQuizRequest>>();

        _sut = new QuizController(
            _quizServiceMock.Object,
            _generateValidatorMock.Object,
            _submitValidatorMock.Object);

        // Setup User Principal for GetUserId()
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GenerateQuiz_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new GenerateQuizRequest { StageProgressId = Guid.NewGuid() };
        var expectedResponse = new QuizResponse { QuizId = Guid.NewGuid() };

        _generateValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _quizServiceMock
            .Setup(s => s.GenerateQuizAsync(request.StageProgressId, _testUserId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.GenerateQuiz(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<QuizResponse>>(objectResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(expectedResponse.QuizId, apiResponse.Data!.QuizId);
    }

    [Fact]
    public async Task GenerateQuiz_ValidationFails_Returns400BadRequest()
    {
        // Arrange
        var request = new GenerateQuizRequest();
        var validationResult = new ValidationResult(new[] { new ValidationFailure("StageProgressId", "Required") });

        _generateValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _sut.GenerateQuiz(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task SubmitQuiz_ValidRequest_Returns200Ok()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var request = new SubmitQuizRequest { Answers = new List<QuizAnswerItem> { new() { QuestionId = "q1", Answer = "A" } } };
        var expectedResponse = new QuizSubmitResponse { Score = 100, IsPassed = true };

        _submitValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _quizServiceMock
            .Setup(s => s.SubmitQuizAsync(quizId, _testUserId, request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.SubmitQuiz(quizId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<QuizSubmitResponse>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(100, apiResponse.Data!.Score);
    }
}
