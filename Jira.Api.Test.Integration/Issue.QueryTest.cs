using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class IssueQueryTest
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueThatIncludesOnlyOneBasicField(JiraClient jira)
	{
		var options = new IssueSearchOptions("key = TST-1")
		{
			FetchBasicFields = false,
			AdditionalFields = ["summary"]
		};

		var issues = await jira.Issues.GetIssuesFromJqlAsync(options, default);
		Assert.NotNull(issues.First().Summary);
		Assert.Null(issues.First().Assignee);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueThatIncludesOnlyOneNonBasicField(JiraClient jira)
	{
		var options = new IssueSearchOptions("key = TST-1")
		{
			FetchBasicFields = false,
			AdditionalFields = ["attachment"]
		};

		var issues = await jira.Issues.GetIssuesFromJqlAsync(options, default);
		var issue = issues.First();
		Assert.Null(issue.Summary);
		Assert.NotEmpty(issue.AdditionalFields.Attachments);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueThatIncludesOnlyAllNonBasicFields(JiraClient jira)
	{
		// Arrange
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue",
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		await issue.AddCommentAsync("My comment", default);
		await issue.AddWorklogAsync("1d", WorklogStrategy.AutoAdjustRemainingEstimate, null, default);

		// Act
		var options = new IssueSearchOptions($"key = {issue.Key.Value}")
		{
			FetchBasicFields = false,
			AdditionalFields = ["comment", "watches", "worklog"]
		};

		var issues = await jira.Issues.GetIssuesFromJqlAsync(options, default);
		var serverIssue = issues.First();

		// Assert
		Assert.Null(serverIssue.Summary);
		Assert.True(serverIssue.AdditionalFields.ContainsKey("watches"));

		var worklogs = serverIssue.AdditionalFields.Worklogs;
		Assert.Equal(20, worklogs.ItemsPerPage);
		Assert.Equal(0, worklogs.StartAt);
		Assert.Equal(1, worklogs.TotalItems);
		Assert.Equal("1d", worklogs.First().TimeSpent);

		var comments = serverIssue.AdditionalFields.Comments;
		Assert.Equal(1, comments.ItemsPerPage);
		Assert.Equal(0, comments.StartAt);
		Assert.Equal(1, comments.TotalItems);
		Assert.Equal("My comment", comments.First().Body);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesAsyncWhenIssueDoesNotExist(JiraClient jira)
	{
		var dict = await jira.Issues.GetIssuesAsync(["TST-9999"], default);

		Assert.False(dict.ContainsKey("TST-9999"));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesWithPagingMetadata(JiraClient jira)
	{
		// Arrange: Create 3 issues to query.
		var summaryValue = "Test-Summary-" + Guid.NewGuid().ToString();
		for (int i = 0; i < 3; i++)
		{
			await new Issue(jira, "TST")
			{
				Type = "1",
				Summary = summaryValue,
				Assignee = "admin"
			}.SaveChangesAsync(default);
		}

		// Act: Query for paged issues.
		var jql = $"summary ~ \"{summaryValue}\"";
		var result = await jira.Issues.GetIssuesFromJqlAsync(jql, 5, 1, default);

		// Assert
		Assert.Equal(1, result.StartAt);
		Assert.Equal(2, result.Count());
		Assert.Equal(3, result.TotalItems);
		Assert.Equal(5, result.ItemsPerPage);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromFilterWithByName(JiraClient jira)
	{
		var issues = await jira.Filters.GetIssuesFromFavoriteAsync("One Issue Filter", 0, null, default);

		Assert.Single(issues);
		var issue = issues.First();
		Assert.Equal("TST-1", issue.Key.Value);
		Assert.NotNull(issue.Summary);
		Assert.False(issue.AdditionalFields.ContainsKey("watches"), "Watches should be excluded by default.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromFilterWithByNameWithFields(JiraClient jira)
	{
		var issues = await jira.Filters.GetIssuesFromFavoriteWithFieldsAsync("One Issue Filter", 0, null, ["watches"], default);

		Assert.Single(issues);
		var issue = issues.First();
		Assert.Equal("TST-1", issue.Key.Value);
		Assert.Null(issue.Summary);
		Assert.True(issue.AdditionalFields.ContainsKey("watches"), "Watches should be included by query.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromFilterById(JiraClient jira)
	{
		var issues = await jira.Filters.GetIssuesFromFilterAsync("10000", 0, null, default);

		Assert.Single(issues);
		var issue = issues.First();
		Assert.Equal("TST-1", issue.Key.Value);
		Assert.NotNull(issue.Summary);
		Assert.False(issue.AdditionalFields.ContainsKey("watches"), "Watches should be excluded by default.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromFilterByIdWithFields(JiraClient jira)
	{
		var issues = await jira.Filters.GetIssuesFromFilterWithFieldsAsync("10000", 0, null, ["watches"], default);

		Assert.Single(issues);
		var issue = issues.First();
		Assert.Equal("TST-1", issue.Key.Value);
		Assert.Null(issue.Summary);
		Assert.True(issue.AdditionalFields.ContainsKey("watches"), "Watches should be included by query.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public void QueryWithZeroResults(JiraClient jira)
	{
		var issues = from i in jira.Issues.Queryable
					 where i.Created == new DateTime(2010, 1, 1)
					 select i;

		Assert.Equal(0, issues.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task QueryIssueWithLabel(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with labels",
			Assignee = "admin"
		};

		issue.Labels.Add("test-label");
		await issue.SaveChangesAsync(default);

		var serverIssue = (from i in jira.Issues.Queryable
						   where i.Labels == "test-label"
						   select i).First();

		Assert.Contains("test-label", serverIssue.Labels);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public void QueryIssueWithCustomDateField(JiraClient jira)
	{
		var issue = (from i in jira.Issues.Queryable
					 where i["Custom Date Field"] <= new DateTime(2012, 4, 1)
					 select i).First();

		Assert.Equal("Sample bug in Test Project", issue.Summary);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task QueryIssuesWithTakeExpression(JiraClient jira)
	{
		// create 2 issues with same summary
		var randomNumber = _random.Next(int.MaxValue);
		await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync(default);
		await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync(default);

		// query with take method to only return 1
		var issues = (from i in jira.Issues.Queryable
					  where i.Summary == randomNumber.ToString()
					  select i).Take(1);

		Assert.Equal(1, issues.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task MaximumNumberOfIssuesPerRequest(JiraClient jira)
	{
		// create 2 issues with same summary
		var randomNumber = _random.Next(int.MaxValue);
		await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync(default);
		await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync(default);

		//set maximum issues and query
		jira.Issues.MaxIssuesPerRequest = 1;
		var issues = from i in jira.Issues.Queryable
					 where i.Summary == randomNumber.ToString()
					 select i;

		Assert.Equal(1, issues.Count());

	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromJqlAsync(JiraClient jira)
	{
		var issues = await jira.Issues.GetIssuesFromJqlAsync("key = TST-1", 0, null, default);
		Assert.Single(issues);
	}
}
