namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class IssueCreateTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueWithIssueTypesPerProject(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "Bug",
			Summary = "Test Summary " + RandomNumberGenerator.GetInt32(int.MaxValue),
			Assignee = "admin"
		};

		issue.Type.SearchByProjectOnly = true;
		var newIssue = await issue.SaveChangesAsync(CancellationToken);

		newIssue.Type.Name.Should().Be("Bug");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueWithOriginalEstimate(JiraClient jira)
	{
		var fields = new CreateIssueFields("TST")
		{
			TimeTrackingData = new IssueTimeTrackingData("1d")
		};

		var issue = new Issue(jira, fields)
		{
			Type = "1",
			Summary = "Test Summary " + RandomNumberGenerator.GetInt32(int.MaxValue),
			Assignee = "admin"
		};

		var newIssue = await issue.SaveChangesAsync(CancellationToken);
		newIssue.TimeTrackingData.OriginalEstimate.Should().Be("1d");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueAsync(JiraClient jira)
	{
		var summaryValue = "Test Summary " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		var newIssue = await issue.SaveChangesAsync(CancellationToken);
		newIssue.Summary.Should().Be(summaryValue);
		newIssue.Project.Should().Be("TST");
		newIssue.Type.Id.Should().Be("1");

		// Create a subtask async.
		var subTask = new Issue(jira, "TST", newIssue.Key.Value)
		{
			Type = "5",
			Summary = "My Subtask",
			Assignee = "admin"
		};

		var newSubTask = await subTask.SaveChangesAsync(CancellationToken);

		newSubTask.ParentIssueKey.Should().Be(newIssue.Key.Value);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithMinimumFieldsSet(JiraClient jira)
	{
		var summaryValue = "Test Summary " + RandomNumberGenerator.GetInt32(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		var issues = (from i in jira.Issues.Queryable
					  where i.Key == issue.Key
					  select i).ToArray();

		issues.Should().ContainSingle();

		issues[0].Summary.Should().Be(summaryValue);
		issues[0].Project.Should().Be("TST");
		issues[0].Type.Id.Should().Be("1");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithAllFieldsSet(JiraClient jira)
	{
		var summaryValue = "Test Summary " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var expectedDueDate = new DateTime(2011, 12, 12);
		var issue = jira.CreateIssue("TST");
		await issue.AffectsVersions.AddAsync("1.0", CancellationToken);
		issue.Assignee = "admin";
		await issue.Components.AddAsync("Server", CancellationToken);
		issue["Custom Text Field"] = "Test Value";  // custom field
		issue.Description = "Test Description";
		issue.DueDate = expectedDueDate;
		issue.Environment = "Test Environment";
		await issue.FixVersions.AddAsync("2.0", CancellationToken);
		issue.Priority = "Major";
		issue.Reporter = "admin";
		issue.Summary = summaryValue;
		issue.Type = "1";
		issue.Labels.Add("testLabel");

		await issue.SaveChangesAsync(CancellationToken);

		var queriedIssue = (from i in jira.Issues.Queryable
							where i.Key == issue.Key
							select i).ToArray().First();

		queriedIssue.Summary.Should().Be(summaryValue);
		queriedIssue.JiraIdentifier.Should().NotBeNull();
		queriedIssue.DueDate.Value.Should().Be(expectedDueDate);
		queriedIssue.Priority.IconUrl.Should().NotBeNull();
		queriedIssue.Type.IconUrl.Should().NotBeNull();
		queriedIssue.Status.IconUrl.Should().NotBeNull();
		queriedIssue.Labels.Should().Contain("testLabel");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithSubTask(JiraClient jira)
	{
		var parentTask = jira.CreateIssue("TST");
		parentTask.Type = "1";
		parentTask.Summary = "Test issue with SubTask" + RandomNumberGenerator.GetInt32(int.MaxValue);
		await parentTask.SaveChangesAsync(CancellationToken);

		var subTask = jira.CreateIssue("TST", parentTask.Key.Value);
		subTask.Type = "5"; // SubTask issue type.
		subTask.Summary = "Test SubTask" + RandomNumberGenerator.GetInt32(int.MaxValue);
		await subTask.SaveChangesAsync(CancellationToken);

		parentTask.Type.IsSubTask.Should().BeFalse();
		subTask.Type.IsSubTask.Should().BeTrue();
		subTask.ParentIssueKey.Should().Be(parentTask.Key.Value);

		// query the subtask again to make sure it loads everything from server.
		subTask = await jira.Issues.GetIssueAsync(subTask.Key.Value, CancellationToken);
		parentTask.Type.IsSubTask.Should().BeFalse();
		subTask.Type.IsSubTask.Should().BeTrue();
		subTask.ParentIssueKey.Should().Be(parentTask.Key.Value);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithVersions(JiraClient jira)
	{
		var summaryValue = "Test issue with versions (Created)" + RandomNumberGenerator.GetInt32(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.AffectsVersions.AddAsync("1.0", CancellationToken);
		await issue.AffectsVersions.AddAsync("2.0", CancellationToken);

		await issue.FixVersions.AddAsync("3.0", CancellationToken);
		await issue.FixVersions.AddAsync("2.0", CancellationToken);

		await issue.SaveChangesAsync(CancellationToken);

		var newIssue = (from i in jira.Issues.Queryable
						where i.AffectsVersions == "1.0" && i.AffectsVersions == "2.0"
								&& i.FixVersions == "2.0" && i.FixVersions == "3.0"
						select i).First();

		newIssue.AffectsVersions.Should().HaveCount(2);
		newIssue.AffectsVersions.Should().Contain(v => v.Name == "1.0");
		newIssue.AffectsVersions.Should().Contain(v => v.Name == "2.0");

		newIssue.FixVersions.Should().HaveCount(2);
		newIssue.FixVersions.Should().Contain(v => v.Name == "2.0");
		newIssue.FixVersions.Should().Contain(v => v.Name == "3.0");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithComponents(JiraClient jira)
	{
		var summaryValue = "Test issue with components (Created)" + RandomNumberGenerator.GetInt32(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.Components.AddAsync("Server", CancellationToken);
		await issue.Components.AddAsync("Client", CancellationToken);

		await issue.SaveChangesAsync(CancellationToken);

		var newIssue = (from i in jira.Issues.Queryable
						where i.Summary == summaryValue && i.Components == "Server" && i.Components == "Client"
						select i).First();

		newIssue.Components.Should().HaveCount(2);
		newIssue.Components.Should().Contain(c => c.Name == "Server");
		newIssue.Components.Should().Contain(c => c.Name == "Client");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithCustomField(JiraClient jira)
	{
		var summaryValue = "Test issue with custom field (Created)" + RandomNumberGenerator.GetInt32(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		issue["Custom Text Field"] = "My new value";
		issue["Custom User Field"] = "admin";

		await issue.SaveChangesAsync(CancellationToken);

		var newIssue = (from i in jira.Issues.Queryable
						where i.Summary == summaryValue && i["Custom Text Field"] == "My new value"
						select i).First();

		newIssue["Custom Text Field"].Should().Be("My new value");
		newIssue["Custom User Field"].Should().Be("admin");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueAsSubtask(JiraClient jira)
	{
		var summaryValue = "Test issue as subtask " + RandomNumberGenerator.GetInt32(int.MaxValue);

		var issue = new Issue(jira, "TST", "TST-1")
		{
			Type = "5", //subtask
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);

		var subtasks = await jira.Issues.GetIssuesFromJqlAsync("project = TST and parent = TST-1", 0, null, CancellationToken);

		subtasks.Should().Contain(s => s.Summary.Equals(summaryValue), $"'{summaryValue}' was not found as a sub-task of TST-1");
	}
}



