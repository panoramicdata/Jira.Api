namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class IssueUpdateTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateIssueAsync(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary "));

		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken.None);
		issue.Type = "2";

		await issue.SaveChangesAsync(CancellationToken);
		issue.Type.Id.Should().Be("2");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public Task UpdateNamedEntities_ById(JiraClient jira)
		=> SaveWithTypeAndPriorityAndVerifyAsync(jira, "AutoLoadNamedEntities_ById ", "1", "5");

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public Task UpdateNamedEntities_ByName(JiraClient jira)
		=> SaveWithTypeAndPriorityAndVerifyAsync(jira, "AutoLoadNamedEntities_Name ", "Bug", "Trivial");

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateIssueType(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary "));

		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		issue.Type = "2";
		await issue.SaveChangesAsync(CancellationToken);

		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		issue.Type.Id.Should().Be("2");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateWithAllFieldsSet(JiraClient jira)
	{
		var summaryValue = WithRandomSuffix("Test Summary ");
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

		var serverIssue = (from i in jira.Issues.Queryable
						   where i.Key == issue.Key
						   select i).ToArray().First();

		serverIssue.Description = "Updated Description";
		serverIssue.DueDate = new DateTime(2011, 10, 10);
		serverIssue.Environment = "Updated Environment";
		serverIssue.Summary = "Updated " + summaryValue;
		serverIssue.Labels.Add("testLabel");
		await serverIssue.SaveChangesAsync(CancellationToken);

		var newServerIssue = (from i in jira.Issues.Queryable
							  where i.Key == issue.Key
							  select i).ToArray().First();

		newServerIssue.Summary.Should().Be("Updated " + summaryValue);
		newServerIssue.Description.Should().Be("Updated Description");
		newServerIssue.Environment.Should().Be("Updated Environment");
		newServerIssue.Labels.Should().Contain("testLabel");
		newServerIssue.DueDate.Should().Be(serverIssue.DueDate);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateAssignee(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test issue with assignee (Updated)"));

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
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary with comments "));

		var commentFromGet = await AddCommentAndVerifyAsync(issue);

		commentFromGet.Body = "new body";
		var commentFromUpdate = await issue.UpdateCommentAsync(commentFromGet, CancellationToken);

		var comments = await issue.GetPagedCommentsAsync(0, null, CancellationToken);
		commentFromGet = comments.First();

		commentFromUpdate.Id.Should().Be(commentFromGet.Id);
		commentFromGet.Body.Should().Be("new body");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveVersions(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test issue with versions (Updated)"));

		await issue.AffectsVersions.AddAsync("1.0", CancellationToken);
		await issue.FixVersions.AddAsync("2.0", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		AssertNames(issue.AffectsVersions, "1.0");
		AssertNames(issue.FixVersions, "2.0");

		issue.AffectsVersions.Remove("1.0");
		await issue.AffectsVersions.AddAsync("2.0", CancellationToken);
		issue.FixVersions.Remove("2.0");
		await issue.FixVersions.AddAsync("3.0", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		AssertNames(issue.AffectsVersions, "2.0");
		AssertNames(issue.FixVersions, "3.0");

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
		AssertNames(issue.FixVersions, "2.0", "3.0");
		AssertNames(issue.AffectsVersions, "1.0", "2.0");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveComponents(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test issue with components (Updated)"));

		await issue.Components.AddAsync("Client", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		AssertNames(issue.Components, "Client");

		issue.Components.Remove("Client");
		await issue.Components.AddAsync("Server", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		AssertNames(issue.Components, "Server");

		issue.Components.Remove("Server");
		await issue.SaveChangesAsync(CancellationToken);
		issue.Components.Should().BeEmpty();

		await issue.Components.AddAsync("Client", CancellationToken);
		await issue.Components.AddAsync("Server", CancellationToken);
		await issue.SaveChangesAsync(CancellationToken);
		AssertNames(issue.Components, "Server", "Client");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveLabelsFromIssue(JiraClient jira)
	{
		var issue = NewAdminIssue(jira, WithRandomSuffix("Test issue with labels (Updated)"));

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
		var issue = NewAdminIssue(jira, WithRandomSuffix("Test issue with custom field (Updated)"));
		issue["Custom Text Field"] = "My new value";

		await issue.SaveChangesAsync(CancellationToken);

		issue["Custom Text Field"] = "My updated value";
		await issue.SaveChangesAsync(CancellationToken);

		issue["Custom Text Field"].Should().Be("My updated value");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanAccessSecurityLevel(JiraClient jira)
	{
		var issue = NewAdminIssue(jira, "TST", "Bug", WithRandomSuffix("Test Summary "));
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

	private static async Task SaveWithTypeAndPriorityAndVerifyAsync(JiraClient jira, string summaryPrefix, string type, string priority)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = WithRandomSuffix(summaryPrefix);
		issue.Type = type;
		issue.Priority = priority;
		await issue.SaveChangesAsync(CancellationToken);

		issue.Type.Id.Should().Be("1");
		issue.Type.Name.Should().Be("Bug");

		issue.Priority.Id.Should().Be("5");
		issue.Priority.Name.Should().Be("Trivial");
	}
}
