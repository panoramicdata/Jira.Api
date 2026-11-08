namespace Jira.Api.Test.Integration;

public abstract class TestBase(ITestOutputHelper outputHelper)
{
	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	protected ITestOutputHelper OutputHelper { get; } = outputHelper;

	protected static string WithRandomSuffix(string prefix) => prefix + RandomNumberGenerator.GetInt32(int.MaxValue);

	protected static Issue NewAdminIssue(JiraClient jira, string summary) => NewAdminIssue(jira, "TST", "1", summary);

	protected static Issue NewAdminIssue(JiraClient jira, string project, string type, string summary) => new(jira, project)
	{
		Type = type,
		Summary = summary,
		Assignee = "admin"
	};

	protected static async Task<Issue> SaveNewAdminIssueAsync(JiraClient jira, string summary)
	{
		var issue = NewAdminIssue(jira, summary);
		await issue.SaveChangesAsync(CancellationToken);
		return issue;
	}

	protected static async Task<Issue> SaveNewIssueAsync(JiraClient jira, string type, string summary)
	{
		var issue = jira.CreateIssue("TST");
		issue.Type = type;
		issue.Summary = summary;
		await issue.SaveChangesAsync(CancellationToken);
		return issue;
	}

	protected static void AssertNames(IEnumerable<JiraNamedEntity> entities, params string[] names)
	{
		entities.Should().HaveCount(names.Length);
		foreach (var name in names)
		{
			entities.Should().Contain(e => e.Name == name);
		}
	}

	protected static async Task<Comment> AddCommentAndVerifyAsync(Issue issue)
	{
		var comments = await issue.GetPagedCommentsAsync(0, null, CancellationToken);
		comments.Should().BeEmpty();

		var commentFromAdd = await issue.AddCommentAsync("new comment", CancellationToken);
		commentFromAdd.Body.Should().Be("new comment");

		comments = await issue.GetPagedCommentsAsync(0, null, CancellationToken);
		comments.Should().ContainSingle();
		var commentFromGet = comments.First();
		commentFromGet.Id.Should().Be(commentFromAdd.Id);
		commentFromGet.Body.Should().Be("new comment");
		commentFromGet.Properties.Should().BeEmpty();
		return commentFromGet;
	}
}
