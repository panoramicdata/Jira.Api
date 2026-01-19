using AwesomeAssertions;

namespace Jira.Api.Test.Integration;

public class IssuePropertiesTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task ReporterUserAndAssigneeUserAvailableFromResponse(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "Bug",
			Summary = "Test Summary with assignee",
			Assignee = "admin"
		};

		issue = await issue.SaveChangesAsync(CancellationToken);

		issue.Reporter.Should().Be("admin");
		issue.ReporterUser.Email.Should().Be("admin@example.com");
		issue.Assignee.Should().Be("admin");
		issue.AssigneeUser.Email.Should().Be("admin@example.com");

		issue.ReporterUser.AvatarUrls.Should().NotBeNull();
		issue.ReporterUser.AvatarUrls.XSmall.Should().NotBeNull();
		issue.ReporterUser.AvatarUrls.Small.Should().NotBeNull();
		issue.ReporterUser.AvatarUrls.Medium.Should().NotBeNull();
		issue.ReporterUser.AvatarUrls.Large.Should().NotBeNull();

		issue.AssigneeUser.AvatarUrls.Should().NotBeNull();
		issue.AssigneeUser.AvatarUrls.XSmall.Should().NotBeNull();
		issue.AssigneeUser.AvatarUrls.Small.Should().NotBeNull();
		issue.AssigneeUser.AvatarUrls.Medium.Should().NotBeNull();
		issue.AssigneeUser.AvatarUrls.Large.Should().NotBeNull();

		issue.Assignee = "test";
		issue = await issue.SaveChangesAsync(CancellationToken);
		issue.Assignee.Should().Be("test");
		issue.AssigneeUser.Email.Should().Be("test@qa.com");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TimeTrackingPropertyIncludedInResponses(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with estimates",
			Assignee = "admin"
		};

		var newIssue = await issue.SaveChangesAsync(CancellationToken);

		newIssue.TimeTrackingData.Should().NotBeNull();
		newIssue.TimeTrackingData.OriginalEstimate.Should().BeNull();

		await newIssue.AddWorklogAsync("1d", WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);

		var issuesFromQuery = await jira.Issues.GetIssuesFromJqlAsync($"id = {newIssue.Key.Value}", 0, null, CancellationToken);
		issuesFromQuery.Single().TimeTrackingData.TimeSpent.Should().Be("1d");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task VotesAndHasVotedProperties(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with votes",
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		// verify no votes
		Assert.Equal(0, issue.Votes.Value);
		issue.HasUserVoted.Should().BeFalse();

		// cast a vote with a second user.
		var jiraTester = JiraClient.CreateRestClient(JiraProvider.HOST, "test", "test");
		await jiraTester.RestClient.ExecuteRequestAsync(RestSharp.Method.Post, $"rest/api/2/issue/{issue.Key.Value}/votes", null, CancellationToken);

		// verify votes for first user
		await issue.RefreshAsync(CancellationToken);
		Assert.Equal(1, issue.Votes.Value);
		issue.HasUserVoted.Should().BeFalse();

		// verify votes for second user
		var issueTester = await jiraTester.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		Assert.Equal(1, issueTester.Votes.Value);
		issueTester.HasUserVoted.Should().BeTrue();
	}
}



