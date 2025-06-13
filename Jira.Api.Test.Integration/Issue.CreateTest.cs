using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class IssueCreateTest
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueWithIssueTypesPerProject(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "Bug",
			Summary = "Test Summary " + _random.Next(int.MaxValue),
			Assignee = "admin"
		};

		issue.Type.SearchByProjectOnly = true;
		var newIssue = await issue.SaveChangesAsync(default);

		Assert.Equal("Bug", newIssue.Type.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueWithOriginalEstimate(Jira jira)
	{
		var fields = new CreateIssueFields("TST")
		{
			TimeTrackingData = new IssueTimeTrackingData("1d")
		};

		var issue = new Issue(jira, fields)
		{
			Type = "1",
			Summary = "Test Summary " + _random.Next(int.MaxValue),
			Assignee = "admin"
		};

		var newIssue = await issue.SaveChangesAsync(default);
		Assert.Equal("1d", newIssue.TimeTrackingData.OriginalEstimate);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueAsync(Jira jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		var newIssue = await issue.SaveChangesAsync(default);
		Assert.Equal(summaryValue, newIssue.Summary);
		Assert.Equal("TST", newIssue.Project);
		Assert.Equal("1", newIssue.Type.Id);

		// Create a subtask async.
		var subTask = new Issue(jira, "TST", newIssue.Key.Value)
		{
			Type = "5",
			Summary = "My Subtask",
			Assignee = "admin"
		};

		var newSubTask = await subTask.SaveChangesAsync(default);

		Assert.Equal(newIssue.Key.Value, newSubTask.ParentIssueKey);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithMinimumFieldsSet(Jira jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		var issues = (from i in jira.Issues.Queryable
					  where i.Key == issue.Key
					  select i).ToArray();

		Assert.Single(issues);

		Assert.Equal(summaryValue, issues[0].Summary);
		Assert.Equal("TST", issues[0].Project);
		Assert.Equal("1", issues[0].Type.Id);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithAllFieldsSet(Jira jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var expectedDueDate = new DateTime(2011, 12, 12);
		var issue = jira.CreateIssue("TST");
		await issue.AffectsVersions.AddAsync("1.0", default);
		issue.Assignee = "admin";
		await issue.Components.AddAsync("Server", default);
		issue["Custom Text Field"] = "Test Value";  // custom field
		issue.Description = "Test Description";
		issue.DueDate = expectedDueDate;
		issue.Environment = "Test Environment";
		await issue.FixVersions.AddAsync("2.0", default);
		issue.Priority = "Major";
		issue.Reporter = "admin";
		issue.Summary = summaryValue;
		issue.Type = "1";
		issue.Labels.Add("testLabel");

		await issue.SaveChangesAsync(default);

		var queriedIssue = (from i in jira.Issues.Queryable
							where i.Key == issue.Key
							select i).ToArray().First();

		Assert.Equal(summaryValue, queriedIssue.Summary);
		Assert.NotNull(queriedIssue.JiraIdentifier);
		Assert.Equal(expectedDueDate, queriedIssue.DueDate.Value);
		Assert.NotNull(queriedIssue.Priority.IconUrl);
		Assert.NotNull(queriedIssue.Type.IconUrl);
		Assert.NotNull(queriedIssue.Status.IconUrl);
		Assert.Contains("testLabel", queriedIssue.Labels);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithSubTask(Jira jira)
	{
		var parentTask = jira.CreateIssue("TST");
		parentTask.Type = "1";
		parentTask.Summary = "Test issue with SubTask" + _random.Next(int.MaxValue);
		await parentTask.SaveChangesAsync(default);

		var subTask = jira.CreateIssue("TST", parentTask.Key.Value);
		subTask.Type = "5"; // SubTask issue type.
		subTask.Summary = "Test SubTask" + _random.Next(int.MaxValue);
		await subTask.SaveChangesAsync(default);

		Assert.False(parentTask.Type.IsSubTask);
		Assert.True(subTask.Type.IsSubTask);
		Assert.Equal(parentTask.Key.Value, subTask.ParentIssueKey);

		// query the subtask again to make sure it loads everything from server.
		subTask = await jira.Issues.GetIssueAsync(subTask.Key.Value, default);
		Assert.False(parentTask.Type.IsSubTask);
		Assert.True(subTask.Type.IsSubTask);
		Assert.Equal(parentTask.Key.Value, subTask.ParentIssueKey);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithVersions(Jira jira)
	{
		var summaryValue = "Test issue with versions (Created)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.AffectsVersions.AddAsync("1.0", default);
		await issue.AffectsVersions.AddAsync("2.0", default);

		await issue.FixVersions.AddAsync("3.0", default);
		await issue.FixVersions.AddAsync("2.0", default);

		await issue.SaveChangesAsync(default);

		var newIssue = (from i in jira.Issues.Queryable
						where i.AffectsVersions == "1.0" && i.AffectsVersions == "2.0"
								&& i.FixVersions == "2.0" && i.FixVersions == "3.0"
						select i).First();

		Assert.Equal(2, newIssue.AffectsVersions.Count);
		Assert.Contains(newIssue.AffectsVersions, v => v.Name == "1.0");
		Assert.Contains(newIssue.AffectsVersions, v => v.Name == "2.0");

		Assert.Equal(2, newIssue.FixVersions.Count);
		Assert.Contains(newIssue.FixVersions, v => v.Name == "2.0");
		Assert.Contains(newIssue.FixVersions, v => v.Name == "3.0");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithComponents(Jira jira)
	{
		var summaryValue = "Test issue with components (Created)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.Components.AddAsync("Server", default);
		await issue.Components.AddAsync("Client", default);

		await issue.SaveChangesAsync(default);

		var newIssue = (from i in jira.Issues.Queryable
						where i.Summary == summaryValue && i.Components == "Server" && i.Components == "Client"
						select i).First();

		Assert.Equal(2, newIssue.Components.Count);
		Assert.Contains(newIssue.Components, c => c.Name == "Server");
		Assert.Contains(newIssue.Components, c => c.Name == "Client");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithCustomField(Jira jira)
	{
		var summaryValue = "Test issue with custom field (Created)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		issue["Custom Text Field"] = "My new value";
		issue["Custom User Field"] = "admin";

		await issue.SaveChangesAsync(default);

		var newIssue = (from i in jira.Issues.Queryable
						where i.Summary == summaryValue && i["Custom Text Field"] == "My new value"
						select i).First();

		Assert.Equal("My new value", newIssue["Custom Text Field"]);
		Assert.Equal("admin", newIssue["Custom User Field"]);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueAsSubtask(Jira jira)
	{
		var summaryValue = "Test issue as subtask " + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST", "TST-1")
		{
			Type = "5", //subtask
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);

		var subtasks = await jira.Issues.GetIssuesFromJqlAsync("project = TST and parent = TST-1", 0, null, default);

		Assert.True(subtasks.Any(s => s.Summary.Equals(summaryValue)),
			$"'{summaryValue}' was not found as a sub-task of TST-1");
	}
}
