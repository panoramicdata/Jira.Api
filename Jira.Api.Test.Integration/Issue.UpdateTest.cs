using RestSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class IssueUpdateTest
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateIssueAsync(Jira jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);

		//retrieve the issue from server and update
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken.None);
		issue.Type = "2";

		var newIssue = await issue.SaveChangesAsync(default);
		Assert.Equal("2", issue.Type.Id);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateNamedEntities_ById(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "AutoLoadNamedEntities_ById " + _random.Next(int.MaxValue);
		issue.Type = "1";
		issue.Priority = "5";
		await issue.SaveChangesAsync(default);

		Assert.Equal("1", issue.Type.Id);
		Assert.Equal("Bug", issue.Type.Name);

		Assert.Equal("5", issue.Priority.Id);
		Assert.Equal("Trivial", issue.Priority.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateNamedEntities_ByName(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "AutoLoadNamedEntities_Name " + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		issue.Priority = "Trivial";
		await issue.SaveChangesAsync(default);

		Assert.Equal("1", issue.Type.Id);
		Assert.Equal("Bug", issue.Type.Name);

		Assert.Equal("5", issue.Priority.Id);
		Assert.Equal("Trivial", issue.Priority.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateIssueType(Jira jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);

		//retrieve the issue from server and update
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		issue.Type = "2";
		await issue.SaveChangesAsync(default);

		//retrieve again and verify
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("2", issue.Type.Id);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateWithAllFieldsSet(Jira jira)
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
		await issue.SaveChangesAsync(default);

		// act, get an issue and update it
		var serverIssue = (from i in jira.Issues.Queryable
						   where i.Key == issue.Key
						   select i).ToArray().First();

		serverIssue.Description = "Updated Description";
		serverIssue.DueDate = new DateTime(2011, 10, 10);
		serverIssue.Environment = "Updated Environment";
		serverIssue.Summary = "Updated " + summaryValue;
		serverIssue.Labels.Add("testLabel");
		await serverIssue.SaveChangesAsync(default);

		// assert, get the issue again and verify
		var newServerIssue = (from i in jira.Issues.Queryable
							  where i.Key == issue.Key
							  select i).ToArray().First();

		Assert.Equal("Updated " + summaryValue, newServerIssue.Summary);
		Assert.Equal("Updated Description", newServerIssue.Description);
		Assert.Equal("Updated Environment", newServerIssue.Environment);
		Assert.Contains("testLabel", newServerIssue.Labels);
		Assert.Equal(serverIssue.DueDate, newServerIssue.DueDate);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateAssignee(Jira jira)
	{
		var summaryValue = "Test issue with assignee (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		issue.Assignee = "test"; //username
		await issue.SaveChangesAsync(default);
		Assert.Equal("test", issue.Assignee);

		issue.Assignee = "admin";
		await issue.SaveChangesAsync(default);
		Assert.Equal("admin", issue.Assignee);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateComment(Jira jira)
	{
		var summaryValue = "Test Summary with comments " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		// create an issue, verify no comments
		await issue.SaveChangesAsync(default);
		var comments = await issue.GetPagedCommentsAsync(0, null, default);
		Assert.Empty(comments);

		// Add a comment
		var commentFromAdd = await issue.AddCommentAsync("new comment", default);
		Assert.Equal("new comment", commentFromAdd.Body);

		// Verify comment retrieval
		comments = await issue.GetPagedCommentsAsync(0, null, default);

		Assert.Single(comments);
		var commentFromGet = comments.First();
		Assert.Equal(commentFromAdd.Id, commentFromGet.Id);
		Assert.Equal("new comment", commentFromGet.Body);
		Assert.Empty(commentFromGet.Properties);

		//Update Comment
		commentFromGet.Body = "new body";
		var commentFromUpdate = await issue.UpdateCommentAsync(commentFromGet, default);

		//Verify comment updated
		comments = await issue.GetPagedCommentsAsync(0, null, default);
		commentFromGet = comments.First();

		Assert.Equal(commentFromGet.Id, commentFromUpdate.Id);
		Assert.Equal("new body", commentFromGet.Body);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveVersions(Jira jira)
	{
		var summaryValue = "Test issue with versions (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		await issue.AffectsVersions.AddAsync("1.0", default);
		await issue.FixVersions.AddAsync("2.0", default);
		await issue.SaveChangesAsync(default);
		Assert.Single(issue.AffectsVersions);
		Assert.Single(issue.FixVersions);
		Assert.Equal("1.0", issue.AffectsVersions.First().Name);
		Assert.Equal("2.0", issue.FixVersions.First().Name);

		issue.AffectsVersions.Remove("1.0");
		await issue.AffectsVersions.AddAsync("2.0", default);
		issue.FixVersions.Remove("2.0");
		await issue.FixVersions.AddAsync("3.0", default);
		await issue.SaveChangesAsync(default);
		Assert.Single(issue.AffectsVersions);
		Assert.Single(issue.FixVersions);
		Assert.Equal("2.0", issue.AffectsVersions.First().Name);
		Assert.Equal("3.0", issue.FixVersions.First().Name);

		issue.AffectsVersions.Remove("2.0");
		issue.FixVersions.Remove("3.0");
		await issue.SaveChangesAsync(default);
		Assert.Empty(issue.AffectsVersions);
		Assert.Empty(issue.FixVersions);

		await issue.AffectsVersions.AddAsync("1.0", default);
		await issue.AffectsVersions.AddAsync("2.0", default);
		await issue.FixVersions.AddAsync("2.0", default);
		await issue.FixVersions.AddAsync("3.0", default);
		await issue.SaveChangesAsync(default);

		Assert.Equal(2, issue.FixVersions.Count);
		Assert.Contains(issue.FixVersions, v => v.Name == "2.0");
		Assert.Contains(issue.FixVersions, v => v.Name == "3.0");

		Assert.Equal(2, issue.AffectsVersions.Count);
		Assert.Contains(issue.AffectsVersions, v => v.Name == "1.0");
		Assert.Contains(issue.AffectsVersions, v => v.Name == "2.0");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveComponents(Jira jira)
	{
		var summaryValue = "Test issue with components (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		await issue.Components.AddAsync("Client", default);
		await issue.SaveChangesAsync(default);
		Assert.Single(issue.Components);
		Assert.Equal("Client", issue.Components.First().Name);

		issue.Components.Remove("Client");
		await issue.Components.AddAsync("Server", default);
		await issue.SaveChangesAsync(default);
		Assert.Single(issue.Components);
		Assert.Equal("Server", issue.Components.First().Name);

		issue.Components.Remove("Server");
		await issue.SaveChangesAsync(default);
		Assert.Empty(issue.Components);

		await issue.Components.AddAsync("Client", default);
		await issue.Components.AddAsync("Server", default);
		await issue.SaveChangesAsync(default);
		Assert.Equal(2, issue.Components.Count);
		Assert.Contains(issue.Components, c => c.Name == "Server");
		Assert.Contains(issue.Components, c => c.Name == "Client");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveLabelsFromIssue(Jira jira)
	{
		var summaryValue = "Test issue with labels (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue.Labels.Add("label1", "label2");
		await issue.SaveChangesAsync(default);
		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal(2, issue.Labels.Count);

		issue.Labels.RemoveAt(0);
		await issue.SaveChangesAsync(default);
		issue = jira.Issues.GetIssueAsync(issue.Key.Value, default).Result;
		Assert.Single(issue.Labels);

		issue.Labels.Clear();
		await issue.SaveChangesAsync(default);
		issue = jira.Issues.GetIssueAsync(issue.Key.Value, default).Result;
		Assert.Empty(issue.Labels);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateIssueWithCustomField(Jira jira)
	{
		var summaryValue = "Test issue with custom field (Updated)" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		issue["Custom Text Field"] = "My new value";

		await issue.SaveChangesAsync(default);

		issue["Custom Text Field"] = "My updated value";
		await issue.SaveChangesAsync(default);

		Assert.Equal("My updated value", issue["Custom Text Field"]);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanAccessSecurityLevel(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "Bug",
			Summary = "Test Summary " + _random.Next(int.MaxValue),
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);
		Assert.Null(issue.SecurityLevel);

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
		await jira.RestClient.ExecuteRequestAsync(Method.Put, resource, body, default);

		await issue.SaveChangesAsync(default);
		Assert.Equal("Test Issue Security Level", issue.SecurityLevel.Name);
	}
}
