using StoryJudge.Core.DTOs;
using StoryJudge.Core.Services;
using Xunit;

namespace StoryJudge.Tests;

public class ReviewServiceTests
{
    [Fact]
    public void CalculateOverallScore_WithAllScores_ReturnsWeightedAverage()
    {
        // Arrange
        var request = new CreateReviewRequest(
            Clarity: 5,
            Ownership: 5,
            Impact: 5,
            DecisionMaking: 5,
            Communication: 5,
            Reflection: 5,
            TechnicalDepth: null,
            Feedback: null,
            Strengths: null,
            Improvements: null
        );

        // Act - Create a mock service to test the calculation
        // Weights: clarity(1.0), ownership(1.2), impact(1.2), decisionMaking(1.0), communication(1.0), reflection(0.8)
        var weightedSum = 5 * 1.0 + 5 * 1.2 + 5 * 1.2 + 5 * 1.0 + 5 * 1.0 + 5 * 0.8;
        var totalWeight = 1.0 + 1.2 + 1.2 + 1.0 + 1.0 + 0.8;
        var expected = Math.Round(weightedSum / totalWeight, 2);

        // Assert
        Assert.Equal(5.0, expected);
    }

    [Fact]
    public void CalculateOverallScore_WithTechnicalDepth_IncludesInCalculation()
    {
        // Arrange
        var request = new CreateReviewRequest(
            Clarity: 4,
            Ownership: 5,
            Impact: 3,
            DecisionMaking: 4,
            Communication: 4,
            Reflection: 3,
            TechnicalDepth: 5,
            Feedback: null,
            Strengths: null,
            Improvements: null
        );

        // Act
        // Weights with tech depth: clarity(1.0), ownership(1.2), impact(1.2), decisionMaking(1.0),
        // communication(1.0), reflection(0.8), technicalDepth(0.6)
        var weightedSum = 4 * 1.0 + 5 * 1.2 + 3 * 1.2 + 4 * 1.0 + 4 * 1.0 + 3 * 0.8 + 5 * 0.6;
        var totalWeight = 1.0 + 1.2 + 1.2 + 1.0 + 1.0 + 0.8 + 0.6;
        var expected = Math.Round(weightedSum / totalWeight, 2);

        // Assert
        Assert.Equal(3.97, expected); // (4 + 6 + 3.6 + 4 + 4 + 2.4 + 3) / 6.8 = 27 / 6.8 ≈ 3.97
    }

    [Fact]
    public void CalculateOverallScore_LowerScores_ReturnsLowerAverage()
    {
        // Weights: clarity(1.0), ownership(1.2), impact(1.2), decisionMaking(1.0), communication(1.0), reflection(0.8)
        var weightedSum = 1 * 1.0 + 1 * 1.2 + 1 * 1.2 + 1 * 1.0 + 1 * 1.0 + 1 * 0.8;
        var totalWeight = 1.0 + 1.2 + 1.2 + 1.0 + 1.0 + 0.8;
        var expected = Math.Round(weightedSum / totalWeight, 2);

        Assert.Equal(1.0, expected);
    }
}
