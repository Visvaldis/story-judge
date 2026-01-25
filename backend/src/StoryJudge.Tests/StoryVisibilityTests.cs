using StoryJudge.Core.Enums;
using StoryJudge.Core.Models;
using Xunit;

namespace StoryJudge.Tests;

public class StoryVisibilityTests
{
    private static bool CanAccess(Story story, string? userId, string? shareToken)
    {
        // Author always has access
        if (!string.IsNullOrEmpty(userId) && story.AuthorId == userId)
        {
            return true;
        }

        return story.Visibility switch
        {
            Visibility.Public => story.Status == StoryStatus.Published,
            Visibility.Unlisted => !string.IsNullOrEmpty(shareToken) && story.ShareToken == shareToken,
            Visibility.Private => false,
            _ => false
        };
    }

    [Fact]
    public void CanAccess_AuthorAlwaysHasAccess_EvenForPrivateStory()
    {
        var story = new Story
        {
            AuthorId = "user123",
            Visibility = Visibility.Private,
            Status = StoryStatus.Draft
        };

        Assert.True(CanAccess(story, "user123", null));
    }

    [Fact]
    public void CanAccess_PublicPublishedStory_AnyoneCanAccess()
    {
        var story = new Story
        {
            AuthorId = "user123",
            Visibility = Visibility.Public,
            Status = StoryStatus.Published
        };

        Assert.True(CanAccess(story, null, null));
        Assert.True(CanAccess(story, "other_user", null));
    }

    [Fact]
    public void CanAccess_PublicDraftStory_OnlyAuthorCanAccess()
    {
        var story = new Story
        {
            AuthorId = "user123",
            Visibility = Visibility.Public,
            Status = StoryStatus.Draft
        };

        Assert.False(CanAccess(story, null, null));
        Assert.False(CanAccess(story, "other_user", null));
        Assert.True(CanAccess(story, "user123", null));
    }

    [Fact]
    public void CanAccess_UnlistedStory_RequiresShareToken()
    {
        var story = new Story
        {
            AuthorId = "user123",
            Visibility = Visibility.Unlisted,
            Status = StoryStatus.Published,
            ShareToken = "secret_token"
        };

        Assert.False(CanAccess(story, null, null));
        Assert.False(CanAccess(story, null, "wrong_token"));
        Assert.True(CanAccess(story, null, "secret_token"));
        Assert.True(CanAccess(story, "user123", null)); // Author still has access
    }

    [Fact]
    public void CanAccess_PrivateStory_OnlyAuthorCanAccess()
    {
        var story = new Story
        {
            AuthorId = "user123",
            Visibility = Visibility.Private,
            Status = StoryStatus.Draft
        };

        Assert.False(CanAccess(story, null, null));
        Assert.False(CanAccess(story, "other_user", null));
        Assert.True(CanAccess(story, "user123", null));
    }
}
