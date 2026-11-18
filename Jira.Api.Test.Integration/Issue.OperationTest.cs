namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public partial class IssueOperationsTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	async Task AssignIssue(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "1", WithRandomSuffix("Test issue to assign"));
		issue.Assignee.Should().Be("admin");

		await issue.AssignAsync("test", CancellationToken);
		issue.Assignee.Should().Be("test");

		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		issue.Assignee.Should().Be("test");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetChangeLogsForIssue(JiraClient jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);
		var changelogs = (await issue.GetChangeLogsAsync(CancellationToken)).OrderBy(log => log.CreatedDate);
		(changelogs.Count() >= 4).Should().BeTrue();

		var firstChangeLog = changelogs.First();
		firstChangeLog.Author.Username.Should().Be("admin");
		firstChangeLog.Items.Should().HaveCount(2);

		var firstItem = firstChangeLog.Items.First();
		firstItem.FieldName.Should().Be("Attachment");
		firstItem.FieldType.Should().Be("jira");
		firstItem.FromValue.Should().BeNull();
		firstItem.FromId.Should().BeNull();
		firstItem.ToId.Should().NotBeNull();
		firstItem.ToValue.Should().Be("SampleImage.png");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveWatchersToIssue(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "1", WithRandomSuffix("Test issue with watchers"));

		await issue.AddWatcherAsync("test", CancellationToken);
		(await issue.GetWatchersAsync(CancellationToken)).Should().HaveCount(2);

		await issue.DeleteWatcherAsync("admin", CancellationToken);
		(await issue.GetWatchersAsync(CancellationToken)).Should().ContainSingle();

		var user = (await issue.GetWatchersAsync(CancellationToken)).First();
		user.Username.Should().Be("test");
		user.Active.Should().BeTrue();
		user.DisplayName.Should().Be("Tester");
		user.Email.Should().Be("test@qa.com");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveWatchersToIssueWithEmailAsUsername(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "1", WithRandomSuffix("Test issue with watchers"));

		var rand = RandomNumberGenerator.GetInt32(int.MaxValue);
		var userInfo = new JiraUserCreationInfo
		{
			Username = $"test{rand}@user.com",
			DisplayName = $"Test User {rand}",
			Email = $"test{rand}@user.com",
			Password = $"MyPass{rand}",
		};
		await jira.Users.CreateUserAsync(userInfo, CancellationToken);

		await issue.AddWatcherAsync(userInfo.Email, CancellationToken);

		var watchers = await issue.GetWatchersAsync(CancellationToken);
		watchers.Should().Contain(w => string.Equals(w.Username, userInfo.Username));

		await jira.Users.DeleteUserAsync(userInfo.Username, CancellationToken);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetSubTasks(JiraClient jira)
	{
		var parentTask = await SaveNewIssueAsync(jira, "1", WithRandomSuffix("Test issue with SubTask"));

		var subTask = jira.CreateIssue("TST", parentTask.Key.Value);
		subTask.Type = "5"; // SubTask issue type.
		subTask.Summary = WithRandomSuffix("Test SubTask");
		await subTask.SaveChangesAsync(CancellationToken);

		var results = await parentTask.GetSubTasksAsync(0, null, CancellationToken);
		results.Should().ContainSingle();
		subTask.Summary.Should().Be(results.First().Summary);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task RetrieveEmptyIssueLinks(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue with no links "));

		(await issue.GetIssueLinksAsync(CancellationToken)).Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRetrieveIssueLinks(JiraClient jira)
	{
		var issue1 = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to link from"));
		var issue2 = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to link to "));
		var issue3 = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to link to "));

		await issue1.LinkToIssueAsync(issue2.Key.Value, "Duplicate", null, CancellationToken);
		await issue1.LinkToIssueAsync(issue3.Key.Value, "Related", null, CancellationToken);

		var issueLinks = await issue1.GetIssueLinksAsync(CancellationToken);
		issueLinks.Should().HaveCount(2);
		issueLinks.Should().OnlyContain(l => l.OutwardIssue.Key.Value == issue1.Key.Value);
		issueLinks.Should().Contain(l => l.LinkType.Name == "Duplicate");
		issueLinks.Should().Contain(l => l.LinkType.Name == "Related");
		issueLinks.Should().Contain(l => l.InwardIssue.Key.Value == issue2.Key.Value);
		issueLinks.Should().Contain(l => l.InwardIssue.Key.Value == issue3.Key.Value);

		AssertLink((await issue2.GetIssueLinksAsync(CancellationToken)).Single(), "Duplicate", issue1, issue2);
		AssertLink((await issue3.GetIssueLinksAsync(CancellationToken)).Single(), "Related", issue1, issue3);

		// subsets of the first issue's links, by link type
		AssertLink((await issue1.GetIssueLinksAsync(["Duplicate"], CancellationToken)).Single(), "Duplicate", issue1, issue2);
		AssertLink((await issue1.GetIssueLinksAsync(["Related"], CancellationToken)).Single(), "Related", issue1, issue3);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRetrieveRemoteLinks(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to link from"));

		(await issue.GetRemoteLinksAsync(CancellationToken)).Should().BeEmpty();

		var url1 = "https://google.com";
		var title1 = "Google";
		var summary1 = "Search engine";

		var url2 = "https://bing.com";
		var title2 = "Bing";

		await issue.AddRemoteLinkAsync(url1, title1, summary1, CancellationToken);
		await issue.AddRemoteLinkAsync(url2, title2, null, CancellationToken);

		var remoteLinks = await issue.GetRemoteLinksAsync(CancellationToken);
		remoteLinks.Should().HaveCount(2);
		remoteLinks.Should().Contain(l => l.RemoteUrl == url1 && l.Title == title1 && l.Summary == summary1);
		remoteLinks.Should().Contain(l => l.RemoteUrl == url2 && l.Title == title2 && l.Summary == null);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task ReorderIssueWithinOperationsBacklogToItsExistingPosition(JiraClient jira)
	{
		var backlogIssuesJql = "project = OPS AND sprint is EMPTY ORDER BY Rank ASC";
		var rankedIssuesBefore = (await jira.Issues.GetIssuesFromJqlAsync(backlogIssuesJql, 0, 2, CancellationToken)).ToList();

		rankedIssuesBefore.Count.Should().BeGreaterThanOrEqualTo(2, "the OPS project backlog must contain at least two ranked issues");

		var issueToKeepInPlace = rankedIssuesBefore[0].Key.Value;
		var issueImmediatelyAfter = rankedIssuesBefore[1].Key.Value;

		await jira.Issues.ReRankAsync(
			new[] { issueToKeepInPlace },
			rankBeforeIssue: issueImmediatelyAfter,
			cancellationToken: CancellationToken);

		var rankedIssuesAfter = (await jira.Issues.GetIssuesFromJqlAsync(backlogIssuesJql, 0, 2, CancellationToken)).ToList();
		rankedIssuesAfter.Select(i => i.Key.Value).Should().Equal(issueToKeepInPlace, issueImmediatelyAfter);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetActionsAsync(JiraClient jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);
		var transitions = await issue.GetAvailableActionsAsync(CancellationToken);
		var resolveTransition = transitions.ElementAt(1);
		var resolveIssueStatus = resolveTransition.To;

		transitions.Should().HaveCount(3);
		resolveTransition.Id.Should().Be("5");
		resolveTransition.Name.Should().Be("Resolve Issue");
		resolveIssueStatus.Name.Should().Be("Resolved");
		resolveIssueStatus.Id.Should().Be("5");
		resolveTransition.Fields.Should().BeNull();

		var transition = transitions.Single(t => t.Name.Equals("Resolve Issue", StringComparison.OrdinalIgnoreCase));
		transition.HasScreen.Should().BeFalse();
		transition.IsInitial.Should().BeFalse();
		transition.IsGlobal.Should().BeFalse();
		transition.To.Name.Should().Be("Resolved");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetActionsWithFields(JiraClient jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);
		var transitions = await issue.GetAvailableActionsAsync(true, CancellationToken);
		var resolveTransition = transitions.ElementAt(1);
		var fields = resolveTransition.Fields;
		var resolution = fields["resolution"];
		var allowedValues = resolution.AllowedValues.ToString();

		fields.Should().HaveCount(3);
		resolution.Name.Should().Be("Resolution");
		resolution.IsRequired.Should().BeTrue();
		resolution.Operations.Should().ContainSingle();
		resolution.Operations.ElementAt(0).Should().Be(IssueFieldEditMetadataOperation.SET);
		foreach (var value in new[] { "Fixed", "Won't Fix", "Duplicate", "Incomplete", "Cannot Reproduce", "Done", "Won't Do" })
		{
			allowedValues.Should().Contain(value);
		}

		resolution.HasDefaultValue.Should().BeFalse();
	}
}
