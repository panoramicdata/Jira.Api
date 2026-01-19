using AwesomeAssertions;

namespace Jira.Api.Test.Integration;

public class IssueQueryTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
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

		var issues = await jira.Issues.GetIssuesFromJqlAsync(options, CancellationToken);
		issues.First().Summary.Should().NotBeNull();
		issues.First().Assignee.Should().BeNull();
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

		var issues = await jira.Issues.GetIssuesFromJqlAsync(options, CancellationToken);
		var issue = issues.First();
		issue.Summary.Should().BeNull();
		issue.AdditionalFields.Attachments.Should().NotBeEmpty();
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

		await issue.SaveChangesAsync(CancellationToken);

		await issue.AddCommentAsync("My comment", CancellationToken);
		await issue.AddWorklogAsync("1d", WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);

		// Act
		var options = new IssueSearchOptions($"key = {issue.Key.Value}")
		{
			FetchBasicFields = false,
			AdditionalFields = ["comment", "watches", "worklog"]
		};

		var issues = await jira.Issues.GetIssuesFromJqlAsync(options, CancellationToken);
		var serverIssue = issues.First();

		// Assert
		serverIssue.Summary.Should().BeNull();
		serverIssue.AdditionalFields.ContainsKey("watches").Should().BeTrue();

		var worklogs = serverIssue.AdditionalFields.Worklogs;
		worklogs.ItemsPerPage.Should().Be(20);
		worklogs.StartAt.Should().Be(0);
		worklogs.TotalItems.Should().Be(1);
		worklogs.First().TimeSpent.Should().Be("1d");

		var comments = serverIssue.AdditionalFields.Comments;
		comments.ItemsPerPage.Should().Be(1);
		comments.StartAt.Should().Be(0);
		comments.TotalItems.Should().Be(1);
		comments.First().Body.Should().Be("My comment");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesAsyncWhenIssueDoesNotExist(JiraClient jira)
	{
		var dict = await jira.Issues.GetIssuesAsync(["TST-9999"], CancellationToken);

		dict.Should().NotContainKey("TST-9999");
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
			}.SaveChangesAsync(CancellationToken);
		}

		// Act: Query for paged issues.
		var jql = $"summary ~ \"{summaryValue}\"";
		var result = await jira.Issues.GetIssuesFromJqlAsync(jql, 5, 1, CancellationToken);

		// Assert
		result.StartAt.Should().Be(1);
		result.Should().HaveCount(2);
		result.TotalItems.Should().Be(3);
		result.ItemsPerPage.Should().Be(5);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromFilterWithByName(JiraClient jira)
	{
		var issues = await jira.Filters.GetIssuesFromFavoriteAsync("One Issue Filter", 0, null, CancellationToken);

		issues.Should().ContainSingle();
		var issue = issues.First();
		issue.Key.Value.Should().Be("TST-1");
		issue.Summary.Should().NotBeNull();
		issue.AdditionalFields.ContainsKey("watches").Should().BeFalse("Watches should be excluded by CancellationToken.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromFilterWithByNameWithFields(JiraClient jira)
	{
		var issues = await jira.Filters.GetIssuesFromFavoriteWithFieldsAsync("One Issue Filter", 0, null, ["watches"], CancellationToken);

		issues.Should().ContainSingle();
		var issue = issues.First();
		issue.Key.Value.Should().Be("TST-1");
		issue.Summary.Should().BeNull();
		issue.AdditionalFields.ContainsKey("watches").Should().BeTrue("Watches should be included by query.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromFilterById(JiraClient jira)
	{
		var issues = await jira.Filters.GetIssuesFromFilterAsync("10000", 0, null, CancellationToken);

		issues.Should().ContainSingle();
		var issue = issues.First();
		issue.Key.Value.Should().Be("TST-1");
		issue.Summary.Should().NotBeNull();
		issue.AdditionalFields.ContainsKey("watches").Should().BeFalse("Watches should be excluded by CancellationToken.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromFilterByIdWithFields(JiraClient jira)
	{
		var issues = await jira.Filters.GetIssuesFromFilterWithFieldsAsync("10000", 0, null, ["watches"], CancellationToken);

		issues.Should().ContainSingle();
		var issue = issues.First();
		issue.Key.Value.Should().Be("TST-1");
		issue.Summary.Should().BeNull();
		issue.AdditionalFields.ContainsKey("watches").Should().BeTrue("Watches should be included by query.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public void QueryWithZeroResults(JiraClient jira)
	{
		var issues = from i in jira.Issues.Queryable
					 where i.Created == new DateTime(2010, 1, 1)
					 select i;

		issues.Should().BeEmpty();
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
		await issue.SaveChangesAsync(CancellationToken);

		var serverIssue = (from i in jira.Issues.Queryable
						   where i.Labels == "test-label"
						   select i).First();

		serverIssue.Labels.Should().Contain("test-label");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public void QueryIssueWithCustomDateField(JiraClient jira)
	{
		var issue = (from i in jira.Issues.Queryable
					 where i["Custom Date Field"] <= new DateTime(2012, 4, 1)
					 select i).First();

		issue.Summary.Should().Be("Sample bug in Test Project");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task QueryIssuesWithTakeExpression(JiraClient jira)
	{
		// create 2 issues with same summary
		var randomNumber = _random.Next(int.MaxValue);
		await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync(CancellationToken);
		await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync(CancellationToken);

		// query with take method to only return 1
		var issues = (from i in jira.Issues.Queryable
					  where i.Summary == randomNumber.ToString()
					  select i).Take(1);

		issues.Should().ContainSingle();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task MaximumNumberOfIssuesPerRequest(JiraClient jira)
	{
		// create 2 issues with same summary
		var randomNumber = _random.Next(int.MaxValue);
		await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync(CancellationToken);
		await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync(CancellationToken);

		//set maximum issues and query
		jira.Issues.MaxIssuesPerRequest = 1;
		var issues = from i in jira.Issues.Queryable
					 where i.Summary == randomNumber.ToString()
					 select i;

		issues.Should().ContainSingle();

	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuesFromJqlAsync(JiraClient jira)
	{
		var issues = await jira.Issues.GetIssuesFromJqlAsync("key = TST-1", 0, null, CancellationToken);
		issues.Should().ContainSingle();
	}
}



