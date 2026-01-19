using AwesomeAssertions;
using RestSharp;

namespace Jira.Api.Test.Integration;

public class IssueUpdateTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateIssueAsync(JiraClient jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);

		//retrieve the issue from server and update
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken.None);
		issue.Type = "2";

		var _ = await issue.SaveChangesAsync(CancellationToken);
		issue.Type.Id.Should().Be("2");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateNamedEntities_ById(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "AutoLoadNamedEntities_ById " + _random.Next(int.MaxValue);
		issue.Type = "1";
		issue.Priority = "5";
		await issue.SaveChangesAsync(CancellationToken);

		issue.Type.Id.Should().Be("1");
		issue.Type.Name.Should().Be("Bug");

		issue.Priority.Id.Should().Be("5");
		issue.Priority.Name.Should().Be("Trivial");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateNamedEntities_ByName(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "AutoLoadNamedEntities_Name " + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		issue.Priority = "Trivial";
		await issue.SaveChangesAsync(CancellationToken);

		issue.Type.Id.Should().Be("1");
		issue.Type.Name.Should().Be("Bug");

		issue.Priority.Id.Should().Be("5");
		issue.Priority.Name.Should().Be("Trivial");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateIssueType(JiraClient jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);

		//retrieve the issue from server and update
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		issue.Type = "2";
		await issue.SaveChangesAsync(CancellationToken);

		//retrieve again and verify
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		issue.Type.Id.Should().Be("2");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateWithAllFieldsSet(JiraClient jira)
	{
		// arrange, create an issue to test.
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Assignee = "admin",
			Description = "Test Description",
			DueDate = new DateTime(2011, 12, 12),
			Environment = "Test Environment",
			Reporter = "admin",
			Type = "1",
			Summary = summaryValue
		};
		await issue.SaveChangesAsync(CancellationToken);

		// act, get an issue and update it
		var serverIssue = (from i in jira.Issues.Queryable
						   where i.Key == issue.Key
						   select i).ToArray().First();

		serverIssue.Description = "Updated Description";
		serverIssue.DueDate = new DateTime(2011, 10, 10);
		serverIssue.Environment = "Updated Environment";
		serverIssue.Summary = "Updated " + summaryValue;
		serverIssue.Labels.Add("testLabel");
		await serverIssue.SaveChangesAsync(CancellationToken);

		// assert, get the issue again and verify
		var newServerIssue = (from i in jira.Issues.Queryable
							  where i.Key == issue.Key
							  select i).ToArray().First();

		newServerIssue.Summary.Should().Be("Updated " + summaryValue);
		newServerIssue.Description.Should().Be("Updated Description");
		newServerIssue.Environment.Should().Be("Updated Environment");
		newServerIssue.Labels.Should().Contain("testLabel");
		Assert.Equal(serverIssue.DueDate, newServerIssue.DueDate);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateAssignee(JiraClient jira)
	{
		var summaryValue = "Test issue with assignee (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		issue.Assignee = "test"; //username
		await issue.SaveChangesAsync(CancellationToken);
		issue.Assignee.Should().Be("test");

		issue.Assignee = "admin";
		await issue.SaveChangesAsync(CancellationToken);
		issue.Assignee.Should().Be("admin");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateComment(JiraClient jira)
	{
		var summaryValue = "Test Summary with comments " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		// create an issue, verify no comments
		await issue.SaveChangesAsync(CancellationToken);
		var comments = await issue.GetPagedCommentsAsync(0, null, CancellationToken);
		comments.Should().BeEmpty();

		// Add a comment
		var commentFromAdd = await issue.AddCommentAsync("new comment", CancellationToken);
		commentFromAdd.Body.Should().Be("new comment");

		// Verify comment retrieval
		comments = await issue.GetPagedCommentsAsync(0, null, CancellationToken);

		comments.Should().ContainSingle();
		var commentFromGet = comments.First();
		commentFromGet.Id.Should().Be(commentFromAdd.Id);
		commentFromGet.Body.Should().Be("new comment");
		commentFromGet.Properties.Should().BeEmpty();

		//Update Comment
		commentFromGet.Body = "new body";
		var commentFromUpdate = await issue.UpdateCommentAsync(commentFromGet, CancellationToken);

		//Verify comment updated
		comments = await issue.GetPagedCommentsAsync(0, null, CancellationToken);
		commentFromGet = comments.First();

		commentFromUpdate.Id.Should().Be(commentFromGet.Id);
		commentFromGet.Body.Should().Be("new body");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveVersions(JiraClient jira)
	{
		var summaryValue = "Test issue with versions (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		await issue.AffectsVersions.AddAsync("1.0", CancellationToken);
		await issue.FixVersions.AddAsync("2.0", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		issue.AffectsVersions.Should().ContainSingle();
		issue.FixVersions.Should().ContainSingle();
		issue.AffectsVersions.First().Name.Should().Be("1.0");
		issue.FixVersions.First().Name.Should().Be("2.0");

		issue.AffectsVersions.Remove("1.0");
		await issue.AffectsVersions.AddAsync("2.0", CancellationToken);
		issue.FixVersions.Remove("2.0");
		await issue.FixVersions.AddAsync("3.0", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		issue.AffectsVersions.Should().ContainSingle();
		issue.FixVersions.Should().ContainSingle();
		issue.AffectsVersions.First().Name.Should().Be("2.0");
		issue.FixVersions.First().Name.Should().Be("3.0");

		issue.AffectsVersions.Remove("2.0");
		issue.FixVersions.Remove("3.0");
		await issue.SaveChangesAsync(CancellationToken);
		issue.AffectsVersions.Should().BeEmpty();
		issue.FixVersions.Should().BeEmpty();

		await issue.AffectsVersions.AddAsync("1.0", CancellationToken);
		await issue.AffectsVersions.AddAsync("2.0", CancellationToken);
		await issue.FixVersions.AddAsync("2.0", CancellationToken);
		await issue.FixVersions.AddAsync("3.0", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);

		Assert.Equal(2, issue.FixVersions.Count);
		issue.FixVersions.Should().Contain(v => v.Name == "2.0");
		issue.FixVersions.Should().Contain(v => v.Name == "3.0");

		Assert.Equal(2, issue.AffectsVersions.Count);
		issue.AffectsVersions.Should().Contain(v => v.Name == "1.0");
		issue.AffectsVersions.Should().Contain(v => v.Name == "2.0");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveComponents(JiraClient jira)
	{
		var summaryValue = "Test issue with components (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		await issue.Components.AddAsync("Client", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		issue.Components.Should().ContainSingle();
		issue.Components.First().Name.Should().Be("Client");

		issue.Components.Remove("Client");
		await issue.Components.AddAsync("Server", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		issue.Components.Should().ContainSingle();
		issue.Components.First().Name.Should().Be("Server");

		issue.Components.Remove("Server");
		await issue.SaveChangesAsync(CancellationToken);
		issue.Components.Should().BeEmpty();

		await issue.Components.AddAsync("Client", CancellationToken);
		await issue.Components.AddAsync("Server", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		Assert.Equal(2, issue.Components.Count);
		issue.Components.Should().Contain(c => c.Name == "Server");
		issue.Components.Should().Contain(c => c.Name == "Client");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveLabelsFromIssue(JiraClient jira)
	{
		var summaryValue = "Test issue with labels (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue.Labels.Add("label1", "label2");
		await issue.SaveChangesAsync(CancellationToken);
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		issue.Labels.Should().HaveCount(2);

		issue.Labels.RemoveAt(0);
		await issue.SaveChangesAsync(CancellationToken);
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		issue.Labels.Should().ContainSingle();

		issue.Labels.Clear();
		await issue.SaveChangesAsync(CancellationToken);
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		issue.Labels.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateIssueWithCustomField(JiraClient jira)
	{
		var summaryValue = "Test issue with custom field (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		issue["Custom Text Field"] = "My new value";

		await issue.SaveChangesAsync(CancellationToken);

		issue["Custom Text Field"] = "My updated value";
		await issue.SaveChangesAsync(CancellationToken);

		Assert.Equal("My updated value", issue["Custom Text Field"]);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanAccessSecurityLevel(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "Bug",
			Summary = "Test Summary " + _random.Next(int.MaxValue),
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);
		issue.SecurityLevel.Should().BeNull();

		var resource = $"rest/api/2/issue/{issue.Key.Value}";
		var body = new
		{
			fields = new
			{
				security = new
				{
					id = "10000"
				}
			}
		};
		await jira.RestClient.ExecuteRequestAsync(Method.Put, resource, body, CancellationToken);

		await issue.SaveChangesAsync(CancellationToken);
		issue.SecurityLevel.Name.Should().Be("Test Issue Security Level");
	}
}



