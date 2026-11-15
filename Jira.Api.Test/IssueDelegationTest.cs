namespace Jira.Api.Test;

public class IssueDelegationTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private const string Key = "ABC-1";

	public static TheoryData<Func<Issue, Task>> Operations => new()
	{
		i => i.GetChangeLogsAsync(CancellationToken),
		i => i.GetCommentsAsync(CancellationToken),
		i => i.GetCommentsAsync(new CommentQueryOptions(), CancellationToken),
		i => i.GetPagedCommentsAsync(0, 10, CancellationToken),
		i => i.DeleteCommentAsync(new Comment(), CancellationToken),
		i => i.AddCommentAsync(new Comment { Author = "bob", Body = "b" }, CancellationToken),
		i => i.UpdateCommentAsync(new Comment { Author = "bob", Body = "b" }, CancellationToken),
		i => i.AddWorklogAsync(new Worklog("1h", new DateTime(2025, 1, 1)), WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken),
		i => i.AddWorklogAsync("1h", WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken),
		i => i.DeleteWorklogAsync(new Worklog("1h", new DateTime(2025, 1, 1)), WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken),
		i => i.GetWorklogsAsync(CancellationToken),
		i => i.GetAvailableActionsAsync(CancellationToken),
		i => i.GetAvailableActionsAsync(true, CancellationToken),
		i => i.GetTimeTrackingDataAsync(CancellationToken),
		i => i.AddWatcherAsync("bob", CancellationToken),
		i => i.GetWatchersAsync(CancellationToken),
		i => i.DeleteWatcherAsync("bob", CancellationToken),
		i => i.GetPropertiesAsync(["k"], CancellationToken),
		i => i.SetPropertyAsync("k", JToken.Parse("{}"), CancellationToken),
		i => i.DeletePropertyAsync("k", CancellationToken),
		i => i.LinkToIssueAsync("ABC-2", "Blocks", null, CancellationToken),
		i => i.GetIssueLinksAsync(CancellationToken),
		i => i.GetIssueLinksAsync(["Blocks"], CancellationToken),
		i => i.AddRemoteLinkAsync("http://x.test", "title", null, CancellationToken),
		i => i.GetRemoteLinksAsync(CancellationToken),
		i => i.GetSubTasksAsync(0, 10, CancellationToken),
		i => i.GetAttachmentsAsync(CancellationToken),
		i => i.AddAttachmentAsync("a.txt", [1], CancellationToken),
		i => i.DeleteAttachmentAsync(new Attachment(i.Jira, new RemoteAttachment { id = "1" }), CancellationToken),
		i => i.GetIssueFieldsEditMetadataAsync(CancellationToken),
		i => i.RefreshAsync(CancellationToken),
		i => i.AssignAsync("bob", CancellationToken),
		i => i.WorkflowTransitionAsync("Done", null, CancellationToken)
	};

	[Theory]
	[MemberData(nameof(Operations))]
	public async Task UnsavedIssue_Throws(Func<Issue, Task> operation)
	{
		var jira = TestableJira.Create();
		var issue = new Issue(jira, "ABC");

		var act = () => operation(issue);

		await act.Should().ThrowExactlyAsync<InvalidOperationException>();
	}

	[Theory]
	[MemberData(nameof(Operations))]
	public async Task SavedIssue_DelegatesToTheService(Func<Issue, Task> operation)
	{
		var jira = TestableJira.Create();
		var issue = new RemoteIssue { key = Key }.ToLocal(jira);
		jira.IssueService
			.Setup(s => s.GetIssueAsync(Key, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RemoteIssue { key = Key }.ToLocal(jira));
		var links = new Mock<IIssueLinkService>();
		var remoteLinks = new Mock<IIssueRemoteLinkService>();
		jira.Services.Register(() => links.Object);
		jira.Services.Register(() => remoteLinks.Object);

		var act = () => operation(issue);

		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task SaveChangesAsync_ForNewIssue_CreatesThenReloads()
	{
		var jira = TestableJira.Create();
		jira.IssueService.Setup(s => s.CreateIssueAsync(It.IsAny<Issue>(), It.IsAny<CancellationToken>())).ReturnsAsync(Key);
		jira.IssueService.Setup(s => s.GetIssueAsync(Key, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RemoteIssue { key = Key, summary = "saved" }.ToLocal(jira));
		var issue = new Issue(jira, "ABC") { Summary = "draft" };

		var saved = await issue.SaveChangesAsync(CancellationToken);

		saved.Key.Value.Should().Be(Key);
		issue.Summary.Should().Be("saved");
		jira.IssueService.Verify(s => s.UpdateIssueAsync(It.IsAny<Issue>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task SaveChangesAsync_ForExistingIssue_UpdatesThenReloads()
	{
		var jira = TestableJira.Create();
		jira.IssueService.Setup(s => s.GetIssueAsync(Key, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RemoteIssue { key = Key, summary = "reloaded" }.ToLocal(jira));
		var issue = new RemoteIssue { key = Key }.ToLocal(jira);

		await issue.SaveChangesAsync(CancellationToken);

		jira.IssueService.Verify(s => s.UpdateIssueAsync(issue, It.IsAny<CancellationToken>()), Times.Once);
		issue.Summary.Should().Be("reloaded");
	}
}
