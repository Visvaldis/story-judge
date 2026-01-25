using System.Text.RegularExpressions;
using Xunit;

namespace StoryJudge.Tests;

public class CoverageDetectionTests
{
    private static bool HasMetrics(string text)
    {
        return Regex.IsMatch(text, @"\d+%|\d+x|\$\d+|\d+ (percent|times|users|customers|revenue)");
    }

    private static bool UsesILanguage(string text)
    {
        return Regex.IsMatch(text, @"\b(I|my|me|mine)\b", RegexOptions.IgnoreCase);
    }

    private static bool MentionsConstraints(string text)
    {
        return Regex.IsMatch(text,
            @"\b(deadline|budget|limited|constraint|restriction|challenge|obstacle|despite|although)\b",
            RegexOptions.IgnoreCase);
    }

    [Theory]
    [InlineData("Improved performance by 50%", true)]
    [InlineData("Revenue increased by 2x", true)]
    [InlineData("Saved $10000 in costs", true)]
    [InlineData("Helped 500 users", true)]
    [InlineData("Good performance improvement", false)]
    [InlineData("Made things better", false)]
    public void HasMetrics_DetectsNumericPatterns(string text, bool expected)
    {
        Assert.Equal(expected, HasMetrics(text));
    }

    [Theory]
    [InlineData("I developed a solution", true)]
    [InlineData("My approach was different", true)]
    [InlineData("The team developed it", false)]
    [InlineData("They did the work", false)]
    public void UsesILanguage_DetectsFirstPersonPronouns(string text, bool expected)
    {
        Assert.Equal(expected, UsesILanguage(text));
    }

    [Theory]
    [InlineData("We had a tight deadline", true)]
    [InlineData("Budget was limited", true)]
    [InlineData("Despite the challenges, we succeeded", true)]
    [InlineData("The project was successful", false)]
    [InlineData("Everything went smoothly", false)]
    public void MentionsConstraints_DetectsConstraintWords(string text, bool expected)
    {
        Assert.Equal(expected, MentionsConstraints(text));
    }
}
