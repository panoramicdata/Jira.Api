namespace Jira.Api.Test.Integration;

public partial class IssueOperationsTest
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueAsync(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to resolve with async"));
		issue.ResolutionDate.Should().BeNull();

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, CancellationToken);

		AssertResolved(issue, "Fixed");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueByIdAsync(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to resolve with async"));

		var transitions = await issue.GetAvailableActionsAsync(CancellationToken);
		var transition = transitions.Single(t => t.Name.Equals("Resolve Issue", StringComparison.OrdinalIgnoreCase));

		await issue.WorkflowTransitionAsync(transition.Id, null, CancellationToken);

		AssertResolved(issue, "Fixed");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueAsyncWithCommentAndFields(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to resolve with async"));
		issue.ResolutionDate.Should().BeNull();

		var updates = new WorkflowTransitionUpdates { Comment = "Comment with transition" };
		await issue.FixVersions.AddAsync("2.0", CancellationToken);

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, updates, CancellationToken.None);

		var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		AssertResolved(updatedIssue, "Fixed");
		updatedIssue.FixVersions.First().Name.Should().Be("2.0");

		var comments = await updatedIssue.GetCommentsAsync(CancellationToken);
		comments.Should().ContainSingle();
		comments.First().Body.Should().Be("Comment with transition");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task Transition_ResolveIssue(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to resolve "));
		issue.ResolutionDate.Should().BeNull();

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, CancellationToken);

		AssertResolved(issue, "Fixed");
		issue.ResolutionDate.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task Transition_ResolveIssue_AsWontFix(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue to resolve "));

		issue.Resolution = "Won't Fix";
		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, CancellationToken);

		AssertResolved(issue, "Won't Fix");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetTimeTrackingDataForIssue(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "Bug", WithRandomSuffix("Issue with timetracking "));

		var timetracking = await issue.GetTimeTrackingDataAsync(CancellationToken);
		timetracking.TimeSpent.Should().BeNull();

		await issue.AddWorklogAsync("2d", WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);

		timetracking = await issue.GetTimeTrackingDataAsync(CancellationToken);
		timetracking.TimeSpent.Should().Be("2d");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetResolutionDate(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		var currentDate = DateTime.Now;
		issue.Summary = "Issue to resolve " + Guid.NewGuid().ToString();
		issue.Type = "Bug";

		// unsaved issue
		issue.ResolutionDate.Should().BeNull();

		// saved, unresolved issue
		await issue.SaveChangesAsync(CancellationToken);
		issue.ResolutionDate.Should().BeNull();

		// saved, resolved issue
		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, CancellationToken);
		issue.ResolutionDate.Should().NotBeNull();
		issue.ResolutionDate.Value.Year.Should().Be(currentDate.Year);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddGetRemoveAttachmentsFromIssue(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary with attachment "));
		(await issue.GetAttachmentsAsync(CancellationToken)).Should().BeEmpty();

		var attachments = await UploadTwoTestFilesAsync(issue, CancellationToken);

		var attachment = attachments.First();
		attachment.Author.Should().Be("admin");
		attachment.AuthorUser.DisplayName.Should().Be("admin");
		attachment.CreatedDate.Should().NotBeNull();
		(attachment.FileSize > 0).Should().BeTrue();
		attachment.MimeType.Should().NotBeEmpty();

		await AssertFirstTestFileDownloadsAsync(attachments);

		await issue.DeleteAttachmentAsync(attachments.First(), CancellationToken);
		(await issue.GetAttachmentsAsync(CancellationToken)).Should().ContainSingle();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetAttachmentById(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary with attachment "));

		File.WriteAllText("testfile1.txt", "Test File Content 1");
		await issue.AddAttachmentAsync([new FileInfo("testfile1.txt")], CancellationToken);

		var attachments = await issue.GetAttachmentsAsync(CancellationToken);
		var attachmentId = attachments.Single().Id;

		var attachment = await jira.Issues.GetAttachmentAsync(attachmentId, CancellationToken);
		attachment.Id.Should().Be(attachmentId);
		attachment.FileName.Should().Be("testfile1.txt");
		attachment.FileSize.Should().BeGreaterThan(0);
		attachment.MimeType.Should().NotBeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DownloadAttachments(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary with attachment "));

		var attachments = await UploadTwoTestFilesAsync(issue, CancellationToken.None);

		await AssertFirstTestFileDownloadsAsync(attachments);
	}
}
