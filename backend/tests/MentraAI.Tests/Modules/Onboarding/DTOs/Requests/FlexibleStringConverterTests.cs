using System.Text.Json;
using MentraAI.API.Modules.Onboarding.DTOs.Requests;
using Xunit;

namespace MentraAI.Tests.Modules.Onboarding.DTOs.Requests;

public class FlexibleStringConverterTests
{
    [Theory]
    [InlineData("{\"QuestionId\": 10, \"AnswerText\": \"developer\"}", "developer")]
    [InlineData("{\"QuestionId\": 1, \"AnswerText\": 25}", "25")]
    [InlineData("{\"QuestionId\": 2, \"AnswerText\": true}", "true")]
    [InlineData("{\"QuestionId\": 2, \"AnswerText\": false}", "false")]
    [InlineData("{\"QuestionId\": 5, \"AnswerText\": null}", null)]
    [InlineData("{\"QuestionId\": 10, \"AnswerText\": [\"skill1\", \"skill2\"]}", "[\"skill1\", \"skill2\"]")]
    [InlineData("{\"QuestionId\": 10, \"AnswerText\": []}", "[]")]
    [InlineData("{\"QuestionId\": 10, \"AnswerText\": {\"key\":\"value\"}}", "{\"key\":\"value\"}")]
    public void Deserialize_VariousJsonTypes_BindsToStringCorrectly(string json, string? expectedAnswerText)
    {
        // Act
        var result = JsonSerializer.Deserialize<AnswerItem>(json);

        // Assert
        Assert.NotNull(result);
        if (expectedAnswerText == null)
        {
            Assert.Null(result.AnswerText);
        }
        else
        {
            Assert.Equal(expectedAnswerText, result.AnswerText);
        }
    }
}
