namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class IssueOperationsTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
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

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DownloadAttachmentData(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary with attachment "));

		File.WriteAllText("testfile.txt", "Test File Content");
		await issue.AddAttachmentAsync(new FileInfo("testfile.txt"), CancellationToken);

		var attachments = await issue.GetAttachmentsAsync(CancellationToken.None);
		attachments.Single().FileName.Should().Be("testfile.txt");

		var bytes = await attachments.Single().DownloadDataAsync(CancellationToken);

		bytes.Should().HaveCount(17);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndGetComments(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary "));
		(await issue.GetCommentsAsync(CancellationToken)).Should().BeEmpty();

		await issue.AddCommentAsync("new comment", CancellationToken);

		var options = new CommentQueryOptions();
		options.Expand.Add("renderedBody");
		var comments = await issue.GetCommentsAsync(options, CancellationToken);
		comments.Should().ContainSingle();

		var comment = comments.First();
		comment.Body.Should().Be("new comment");
		comment.CreatedDate.Value.Year.Should().Be(DateTime.Now.Year);
		comment.Visibility.Should().BeNull();
		comment.RenderedBody.Should().Be("new comment");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndUpdateComments(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary "));

		var comment = new Comment
		{
			Author = (await jira.Users.GetMyselfAsync(CancellationToken)).Username,
			Body = "New comment",
			Visibility = new CommentVisibility("Developers")
		};
		var newComment = await issue.AddCommentAsync(comment, CancellationToken);

		newComment.Visibility.Type.Should().Be("role");
		newComment.Visibility.Value.Should().Be("Developers");
		newComment.Author.Should().Be("admin");
		newComment.AuthorUser.DisplayName.Should().Be("admin");
		newComment.UpdateAuthor.Should().Be("admin");
		newComment.UpdateAuthorUser.DisplayName.Should().Be("admin");

		newComment.Visibility.Value = "Users";
		newComment.Body = "changed body";
		var updatedComment = await issue.UpdateCommentAsync(newComment, CancellationToken);

		updatedComment.Visibility.Type.Should().Be("role");
		updatedComment.Visibility.Value.Should().Be("Users");
		updatedComment.Body.Should().Be("changed body");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddGetAndDeleteCommentsAsync(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary with comments "));

		var commentFromGet = await AddCommentAndVerifyAsync(issue);

		await issue.DeleteCommentAsync(commentFromGet, CancellationToken);

		var comments = await issue.GetPagedCommentsAsync(0, null, CancellationToken);
		comments.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanRetrievePagedCommentsAsync(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test Summary with comments "));

		for (var i = 1; i <= 4; i++)
		{
			await issue.AddCommentAsync($"new comment{i}", CancellationToken);
		}

		var comments = await issue.GetPagedCommentsAsync(2, 0, CancellationToken);
		comments.Select(c => c.Body).Should().Equal("new comment1", "new comment2");

		comments = await issue.GetPagedCommentsAsync(2, 2, CancellationToken);
		comments.Select(c => c.Body).Should().Equal("new comment3", "new comment4");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DeleteIssue(JiraClient jira)
	{
		var issue = await SaveNewIssueAsync(jira, "1", $"Issue to delete ({RandomNumberGenerator.GetInt32(int.MaxValue)})");
		jira.Issues.Queryable.Where(i => i.Key == issue.Key).Any().Should().BeTrue("Expected issue in server");

		await jira.Issues.DeleteIssueAsync(issue.Key.Value, CancellationToken);
		var act = () => jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		await act.Should().ThrowExactlyAsync<AggregateException>();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndGetWorklogs(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test issue with work logs"));

		await issue.AddWorklogAsync("1d", WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);
		await issue.AddWorklogAsync("1h", WorklogStrategy.RetainRemainingEstimate, null, CancellationToken);
		await issue.AddWorklogAsync("1m", WorklogStrategy.NewRemainingEstimate, "2d", CancellationToken);
		await issue.AddWorklogAsync(new Worklog("2d", new DateTime(2012, 1, 1), "comment"), WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);

		var logs = await issue.GetWorklogsAsync(CancellationToken);
		logs.Should().HaveCount(4);
		logs.ElementAt(3).Comment.Should().Be("comment");
		logs.ElementAt(3).StartDate.Should().Be(new DateTime(2012, 1, 1));
		logs.First().Author.Should().Be("admin");
		logs.First().AuthorUser.DisplayName.Should().Be("admin");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DeleteWorklog(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, WithRandomSuffix("Test issue with worklogs"));

		var worklog = await issue.AddWorklogAsync("1h", WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);
		(await issue.GetWorklogsAsync(CancellationToken)).Should().ContainSingle();

		await issue.DeleteWorklogAsync(worklog, WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);
		(await issue.GetWorklogsAsync(CancellationToken)).Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemovePropertyAndVerifyProperties(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, "Test issue with properties");

		var propertyKeys = await jira.Issues.GetPropertyKeysAsync(issue.Key.Value, CancellationToken);
		propertyKeys.Should().BeEmpty();

		var keyString = "test-property";
		var keyValue = JToken.FromObject("test-string");
		await issue.SetPropertyAsync(keyString, keyValue, CancellationToken);

		propertyKeys = await jira.Issues.GetPropertyKeysAsync(issue.Key.Value, CancellationToken);
		propertyKeys.SequenceEqual([keyString]).Should().BeTrue();

		var issueProperties = await issue.GetPropertiesAsync([keyString, "non-existent-property"], CancellationToken);
		AssertProperty(issueProperties, keyString, keyValue);

		await issue.DeletePropertyAsync(keyString, CancellationToken);

		issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);
		issueProperties.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task RemoveInexistantPropertyAndVerifyNoOp(JiraClient jira)
	{
		var issue = await SaveNewAdminIssueAsync(jira, "Test issue with properties");

		var keyString = "test-property-nonexist";
		await issue.DeletePropertyAsync(keyString, CancellationToken);

		var issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);
		issueProperties.Should().BeEmpty();
	}

	// A JSON null property value comes back as a C# null.
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public Task AddNullPropertyAndVerify(JiraClient jira)
		=> SetPropertyAndVerifyAsync(jira, "test-property-null", JToken.Parse("null"), null);

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public Task AddObjectPropertyAndVerify(JiraClient jira)
		=> SetPropertyAndVerifyAsync(jira, "test-property-object", JToken.FromObject(new { KeyName = "TestKey" }));

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public Task AddBoolPropertyAndVerify(JiraClient jira)
		=> SetPropertyAndVerifyAsync(jira, "test-property-bool", JToken.FromObject(true));

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public Task AddListPropertyAndVerify(JiraClient jira)
		=> SetPropertyAndVerifyAsync(jira, "test-property-list", JToken.FromObject(new List<string> { "One", "Two", "Three" }));

	private static void AssertLink(IssueLink link, string linkType, Issue outward, Issue inward)
	{
		link.LinkType.Name.Should().Be(linkType);
		link.OutwardIssue.Key.Value.Should().Be(outward.Key.Value);
		link.InwardIssue.Key.Value.Should().Be(inward.Key.Value);
	}

	private static void AssertResolved(Issue issue, string resolution)
	{
		issue.Status.Name.Should().Be("Resolved");
		issue.Resolution.Name.Should().Be(resolution);
	}

	private static async Task<IEnumerable<Attachment>> UploadTwoTestFilesAsync(Issue issue, CancellationToken getCancellationToken)
	{
		File.WriteAllText("testfile1.txt", "Test File Content 1");
		File.WriteAllText("testfile2.txt", "Test File Content 2");
		await issue.AddAttachmentAsync([new FileInfo("testfile1.txt"), new FileInfo("testfile2.txt")], CancellationToken);

		var attachments = await issue.GetAttachmentsAsync(getCancellationToken);
		attachments.Should().HaveCount(2);
		attachments.Should().Contain(a => a.FileName.Equals("testfile1.txt"), "'testfile1.txt' was not downloaded from server");
		attachments.Should().Contain(a => a.FileName.Equals("testfile2.txt"), "'testfile2.txt' was not downloaded from server");
		return attachments;
	}

	private static async Task AssertFirstTestFileDownloadsAsync(IEnumerable<Attachment> attachments)
	{
		var tempFile = Path.GetTempFileName();
		var attachment = attachments.First(a => a.FileName.Equals("testfile1.txt"));
		await attachment.DownloadAsync(tempFile, CancellationToken);
		File.ReadAllText(tempFile).Should().Be("Test File Content 1");
	}

	private static Task SetPropertyAndVerifyAsync(JiraClient jira, string keyString, JToken keyValue)
		=> SetPropertyAndVerifyAsync(jira, keyString, keyValue, keyValue);

	private static async Task SetPropertyAndVerifyAsync(JiraClient jira, string keyString, JToken keyValue, JToken? expected)
	{
		var issue = await SaveNewAdminIssueAsync(jira, "Test issue with properties");

		await issue.SetPropertyAsync(keyString, keyValue, CancellationToken);

		var issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);
		AssertProperty(issueProperties, keyString, expected);
	}

	private static void AssertProperty(IReadOnlyDictionary<string, JToken> issueProperties, string keyString, JToken? expected)
	{
		var truth = new Dictionary<string, JToken?> { { keyString, expected } };
		issueProperties.Keys.SequenceEqual(truth.Keys).Should().BeTrue();
		issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()).Should().BeTrue();
	}
}
