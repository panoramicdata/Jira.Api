namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class IssueOperationsTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{

	[Theory]
	[ClassData(typeof(JiraProvider))]
	async Task AssignIssue(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Type = "1";
		issue.Summary = "Test issue to assign" + RandomNumberGenerator.GetInt32(int.MaxValue);

		await issue.SaveChangesAsync(CancellationToken);
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
		//Assert.NotNull(firstChangeLog.CreatedDate); this can never be null
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
		var issue = jira.CreateIssue("TST");
		issue.Type = "1";
		issue.Summary = "Test issue with watchers" + RandomNumberGenerator.GetInt32(int.MaxValue);
		await issue.SaveChangesAsync(CancellationToken);

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
		// Create issue.
		var issue = jira.CreateIssue("TST");
		issue.Type = "1";
		issue.Summary = "Test issue with watchers" + RandomNumberGenerator.GetInt32(int.MaxValue);
		await issue.SaveChangesAsync(CancellationToken);

		// Create user with e-mail as username.
		var rand = RandomNumberGenerator.GetInt32(int.MaxValue);
		var userInfo = new JiraUserCreationInfo()
		{
			Username = $"test{rand}@user.com",
			DisplayName = $"Test User {rand}",
			Email = $"test{rand}@user.com",
			Password = $"MyPass{rand}",
		};

		await jira.Users.CreateUserAsync(userInfo, CancellationToken);

		// Add the user as a watcher on the issue.
		await issue.AddWatcherAsync(userInfo.Email, CancellationToken);

		// Verify the watchers of the issue contains the username.
		var watchers = await issue.GetWatchersAsync(CancellationToken);
		watchers.Should().Contain(w => string.Equals(w.Username, userInfo.Username));

		// Delete user.
		await jira.Users.DeleteUserAsync(userInfo.Username, CancellationToken);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetSubTasks(JiraClient jira)
	{
		var parentTask = jira.CreateIssue("TST");
		parentTask.Type = "1";
		parentTask.Summary = "Test issue with SubTask" + RandomNumberGenerator.GetInt32(int.MaxValue);
		await parentTask.SaveChangesAsync(CancellationToken);

		var subTask = jira.CreateIssue("TST", parentTask.Key.Value);
		subTask.Type = "5"; // SubTask issue type.
		subTask.Summary = "Test SubTask" + RandomNumberGenerator.GetInt32(int.MaxValue);
		await subTask.SaveChangesAsync(CancellationToken);

		var results = await parentTask.GetSubTasksAsync(0, null, CancellationToken);
		results.Should().ContainSingle();
		subTask.Summary.Should().Be(results.First().Summary);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task RetrieveEmptyIssueLinks(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue with no links " + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(CancellationToken);

		(await issue.GetIssueLinksAsync(CancellationToken)).Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRetrieveIssueLinks(JiraClient jira)
	{
		var issue1 = jira.CreateIssue("TST");
		issue1.Summary = "Issue to link from" + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue1.Type = "Bug";
		await issue1.SaveChangesAsync(CancellationToken);

		var issue2 = jira.CreateIssue("TST");
		issue2.Summary = "Issue to link to " + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue2.Type = "Bug";
		await issue2.SaveChangesAsync(CancellationToken);

		var issue3 = jira.CreateIssue("TST");
		issue3.Summary = "Issue to link to " + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue3.Type = "Bug";
		await issue3.SaveChangesAsync(CancellationToken);

		// link the 1st issue to 2 and 3.
		await issue1.LinkToIssueAsync(issue2.Key.Value, "Duplicate", null, CancellationToken);
		await issue1.LinkToIssueAsync(issue3.Key.Value, "Related", null, CancellationToken);

		// Verify links of 1st issue.
		var issueLinks = await issue1.GetIssueLinksAsync(CancellationToken);
		issueLinks.Should().HaveCount(2);
		issueLinks.Should().OnlyContain(l => l.OutwardIssue.Key.Value == issue1.Key.Value);
		issueLinks.Should().Contain(l => l.LinkType.Name == "Duplicate");
		issueLinks.Should().Contain(l => l.LinkType.Name == "Related");
		issueLinks.Should().Contain(l => l.InwardIssue.Key.Value == issue2.Key.Value);
		issueLinks.Should().Contain(l => l.InwardIssue.Key.Value == issue3.Key.Value);

		// Verify link of 2nd issue.
		var issueLink = (await issue2.GetIssueLinksAsync(CancellationToken)).Single();
		issueLink.LinkType.Name.Should().Be("Duplicate");
		issueLink.OutwardIssue.Key.Value.Should().Be(issue1.Key.Value);
		issueLink.InwardIssue.Key.Value.Should().Be(issue2.Key.Value);

		// Verify link of 3rd issue.
		issueLink = (await issue3.GetIssueLinksAsync(CancellationToken)).Single();
		issueLink.LinkType.Name.Should().Be("Related");
		issueLink.OutwardIssue.Key.Value.Should().Be(issue1.Key.Value);
		issueLink.InwardIssue.Key.Value.Should().Be(issue3.Key.Value);

		// Verify retrieving subset of links of 1st issue
		var issueLinkOfType = (await issue1.GetIssueLinksAsync(["Duplicate"], CancellationToken)).Single();
		issueLinkOfType.LinkType.Name.Should().Be("Duplicate");
		issueLinkOfType.OutwardIssue.Key.Value.Should().Be(issue1.Key.Value);
		issueLinkOfType.InwardIssue.Key.Value.Should().Be(issue2.Key.Value);

		issueLinkOfType = (await issue1.GetIssueLinksAsync(["Related"], CancellationToken)).Single();
		issueLinkOfType.LinkType.Name.Should().Be("Related");
		issueLinkOfType.OutwardIssue.Key.Value.Should().Be(issue1.Key.Value);
		issueLinkOfType.InwardIssue.Key.Value.Should().Be(issue3.Key.Value);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRetrieveRemoteLinks(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to link from" + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(CancellationToken);

		// Verify issue with no remote links.
		(await issue.GetRemoteLinksAsync(CancellationToken)).Should().BeEmpty();

		var url1 = "https://google.com";
		var title1 = "Google";
		var summary1 = "Search engine";

		var url2 = "https://bing.com";
		var title2 = "Bing";

		// Add remote links
		await issue.AddRemoteLinkAsync(url1, title1, summary1, CancellationToken);
		await issue.AddRemoteLinkAsync(url2, title2, null, CancellationToken);

		// Verify remote links of issue.
		var remoteLinks = await issue.GetRemoteLinksAsync(CancellationToken);
		remoteLinks.Should().HaveCount(2);
		remoteLinks.Should().Contain(l => l.RemoteUrl == url1 && l.Title == title1 && l.Summary == summary1);
		remoteLinks.Should().Contain(l => l.RemoteUrl == url2 && l.Title == title2 && l.Summary == null);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetActionsAsync(JiraClient jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);
		var transitions = await issue.GetAvailableActionsAsync(CancellationToken);
		var resolveTransition = transitions.ElementAt(1);
		var resolveIssueStatus = resolveTransition.To;

		// assert
		transitions.Should().HaveCount(3);
		resolveTransition.Id.Should().Be("5");
		resolveTransition.Name.Should().Be("Resolve Issue");
		resolveIssueStatus.Name.Should().Be("Resolved");
		resolveIssueStatus.Id.Should().Be("5");
		resolveTransition.Fields.Should().BeNull();

		var transition = transitions.Single(t => t.Name.Equals("Resolve Issue", StringComparison.OrdinalIgnoreCase));
		transition.HasScreen.Should().BeFalse();
		transition.IsInitial.Should().BeFalse();
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

		// assert
      fields.Should().HaveCount(3);
		resolution.Name.Should().Be("Resolution");
		resolution.IsRequired.Should().BeTrue();
		resolution.Operations.Should().ContainSingle();
      resolution.Operations.ElementAt(0).Should().Be(IssueFieldEditMetadataOperation.SET);
		allowedValues.Should().Contain("Fixed");
		allowedValues.Should().Contain("Won't Fix");
		allowedValues.Should().Contain("Duplicate");
		allowedValues.Should().Contain("Incomplete");
		allowedValues.Should().Contain("Cannot Reproduce");
		allowedValues.Should().Contain("Done");
		allowedValues.Should().Contain("Won't Do");
		resolution.HasDefaultValue.Should().BeFalse();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueAsync(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve with async" + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(CancellationToken);

		issue.ResolutionDate.Should().BeNull();

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, CancellationToken);

		issue.Status.Name.Should().Be("Resolved");
		issue.Resolution.Name.Should().Be("Fixed");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueByIdAsync(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve with async" + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(CancellationToken);

		var transitions = await issue.GetAvailableActionsAsync(CancellationToken);
		var transition = transitions.Single(t => t.Name.Equals("Resolve Issue", StringComparison.OrdinalIgnoreCase));

		await issue.WorkflowTransitionAsync(transition.Id, null, CancellationToken);

		issue.Status.Name.Should().Be("Resolved");
		issue.Resolution.Name.Should().Be("Fixed");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueAsyncWithCommentAndFields(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve with async" + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(CancellationToken);

		issue.ResolutionDate.Should().BeNull();
		var updates = new WorkflowTransitionUpdates() { Comment = "Comment with transition" };
		await issue.FixVersions.AddAsync("2.0", CancellationToken);

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, updates, CancellationToken.None);

		var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		updatedIssue.Status.Name.Should().Be("Resolved");
		updatedIssue.Resolution.Name.Should().Be("Fixed");
		updatedIssue.FixVersions.First().Name.Should().Be("2.0");

		var comments = await updatedIssue.GetCommentsAsync(CancellationToken);
		comments.Should().ContainSingle();
		comments.First().Body.Should().Be("Comment with transition");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task Transition_ResolveIssue(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve " + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(CancellationToken);

		issue.ResolutionDate.Should().BeNull();

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, CancellationToken);

		issue.Status.Name.Should().Be("Resolved");
		issue.Resolution.Name.Should().Be("Fixed");
		issue.ResolutionDate.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task Transition_ResolveIssue_AsWontFix(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve " + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(CancellationToken);

		issue.Resolution = "Won't Fix";
		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, CancellationToken);

		issue.Status.Name.Should().Be("Resolved");
		issue.Resolution.Name.Should().Be("Won't Fix");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetTimeTrackingDataForIssue(JiraClient jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue with timetracking " + RandomNumberGenerator.GetInt32(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(CancellationToken);

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
		// Arrange
		var issue = jira.CreateIssue("TST");
		var currentDate = DateTime.Now;
		issue.Summary = "Issue to resolve " + Guid.NewGuid().ToString();
		issue.Type = "Bug";

		// Act, Assert: Returns null for unsaved issue.
		issue.ResolutionDate.Should().BeNull();

		// Act, Assert: Returns null for saved unresolved issue.
		await issue.SaveChangesAsync(CancellationToken);
		issue.ResolutionDate.Should().BeNull();

		// Act, Assert: returns date for saved resolved issue.
		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, CancellationToken);
		issue.ResolutionDate.Should().NotBeNull();
        issue.ResolutionDate.Value.Year.Should().Be(currentDate.Year);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddGetRemoveAttachmentsFromIssue(JiraClient jira)
	{
		var summaryValue = "Test Summary with attachment " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		// create an issue, verify no attachments
		await issue.SaveChangesAsync(CancellationToken);
		(await issue.GetAttachmentsAsync(CancellationToken)).Should().BeEmpty();

		// upload multiple attachments
		File.WriteAllText("testfile1.txt", "Test File Content 1");
		File.WriteAllText("testfile2.txt", "Test File Content 2");
		await issue.AddAttachmentAsync(new FileInfo[] { new("testfile1.txt"), new("testfile2.txt") }, CancellationToken);

		// verify all attachments can be retrieved.
		var attachments = await issue.GetAttachmentsAsync(CancellationToken);
		attachments.Should().HaveCount(2);
		attachments.Should().Contain(a => a.FileName.Equals("testfile1.txt"), "'testfile1.txt' was not downloaded from server");
		attachments.Should().Contain(a => a.FileName.Equals("testfile2.txt"), "'testfile2.txt' was not downloaded from server");

		// verify properties of an attachment
		var attachment = attachments.First();
		attachment.Author.Should().Be("admin");
		attachment.AuthorUser.DisplayName.Should().Be("admin");
		attachment.CreatedDate.Should().NotBeNull();
		(attachment.FileSize > 0).Should().BeTrue();
       attachment.MimeType.Should().NotBeEmpty();

		// download an attachment
		var tempFile = Path.GetTempFileName();
		var attachment2 = attachments.First(a => a.FileName.Equals("testfile1.txt"));
		await attachment2.DownloadAsync(tempFile, CancellationToken);
		File.ReadAllText(tempFile).Should().Be("Test File Content 1");

		// remove an attachment
		await issue.DeleteAttachmentAsync(attachments.First(), CancellationToken);
		(await issue.GetAttachmentsAsync(CancellationToken)).Should().ContainSingle();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DownloadAttachments(JiraClient jira)
	{
		// create an issue
		var summaryValue = "Test Summary with attachment " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);

		// upload multiple attachments
		File.WriteAllText("testfile1.txt", "Test File Content 1");
		File.WriteAllText("testfile2.txt", "Test File Content 2");
		await issue.AddAttachmentAsync(new FileInfo[] { new("testfile1.txt"), new("testfile2.txt") }, CancellationToken);

		// Get attachment metadata
		var attachments = await issue.GetAttachmentsAsync(CancellationToken.None);
		attachments.Should().HaveCount(2);
		attachments.Should().Contain(a => a.FileName.Equals("testfile1.txt"), "'testfile1.txt' was not downloaded from server");
		attachments.Should().Contain(a => a.FileName.Equals("testfile2.txt"), "'testfile2.txt' was not downloaded from server");

		// download an attachment.
		var tempFile = Path.GetTempFileName();
		var attachment = attachments.First(a => a.FileName.Equals("testfile1.txt"));

		await attachment.DownloadAsync(tempFile, CancellationToken);
		File.ReadAllText(tempFile).Should().Be("Test File Content 1");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DownloadAttachmentData(JiraClient jira)
	{
		// create an issue
		var summaryValue = "Test Summary with attachment " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);

		// upload attachment
		File.WriteAllText("testfile.txt", "Test File Content");
		await issue.AddAttachmentAsync(new FileInfo("testfile.txt"), CancellationToken);

		// Get attachment metadata
		var attachments = await issue.GetAttachmentsAsync(CancellationToken.None);
		attachments.Single().FileName.Should().Be("testfile.txt");

		// download attachment as byte array
		var bytes = await attachments.Single().DownloadDataAsync(CancellationToken);

     bytes.Length.Should().Be(17);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndGetComments(JiraClient jira)
	{
		var summaryValue = "Test Summary " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		// create an issue, verify no comments
		await issue.SaveChangesAsync(CancellationToken);
		(await issue.GetCommentsAsync(CancellationToken)).Should().BeEmpty();

		// Add a comment
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
		var summaryValue = "Test Summary " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		// Add a comment
		var comment = new Comment()
		{
			Author = (await jira.Users.GetMyselfAsync(CancellationToken)).Username,
			Body = "New comment",
			Visibility = new CommentVisibility("Developers")
		};
		var newComment = await issue.AddCommentAsync(comment, CancellationToken);

		// Verify
		newComment.Visibility.Type.Should().Be("role");
		newComment.Visibility.Value.Should().Be("Developers");
		newComment.Author.Should().Be("admin");
		newComment.AuthorUser.DisplayName.Should().Be("admin");
		newComment.UpdateAuthor.Should().Be("admin");
		newComment.UpdateAuthorUser.DisplayName.Should().Be("admin");

		// Update the comment
		newComment.Visibility.Value = "Users";
		newComment.Body = "changed body";
		var updatedComment = await issue.UpdateCommentAsync(newComment, CancellationToken);

		// verify changes.
		updatedComment.Visibility.Type.Should().Be("role");
		updatedComment.Visibility.Value.Should().Be("Users");
		updatedComment.Body.Should().Be("changed body");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddGetAndDeleteCommentsAsync(JiraClient jira)
	{
		var summaryValue = "Test Summary with comments " + RandomNumberGenerator.GetInt32(int.MaxValue);
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

		// Delete comment.
		await issue.DeleteCommentAsync(commentFromGet, CancellationToken);

		// Verify no comments
		comments = await issue.GetPagedCommentsAsync(0, null, CancellationToken);
		comments.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanRetrievePagedCommentsAsync(JiraClient jira)
	{
		var summaryValue = "Test Summary with comments " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		// Add a comments
		await issue.AddCommentAsync("new comment1", CancellationToken);
		await issue.AddCommentAsync("new comment2", CancellationToken);
		await issue.AddCommentAsync("new comment3", CancellationToken);
		await issue.AddCommentAsync("new comment4", CancellationToken);

		// Verify first page of comments
		var comments = await issue.GetPagedCommentsAsync(2, 0, CancellationToken);
		comments.Should().HaveCount(2);
		comments.First().Body.Should().Be("new comment1");
		comments.Skip(1).First().Body.Should().Be("new comment2");

		// Verify second page of comments
		comments = await issue.GetPagedCommentsAsync(2, 2, CancellationToken);
		comments.Should().HaveCount(2);
		comments.First().Body.Should().Be("new comment3");
		comments.Skip(1).First().Body.Should().Be("new comment4");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DeleteIssue(JiraClient jira)
	{
		// Create issue and verify it is found in server.
		var issue = jira.CreateIssue("TST");
		issue.Type = "1";
		issue.Summary = $"Issue to delete ({RandomNumberGenerator.GetInt32(int.MaxValue)})";
		await issue.SaveChangesAsync(CancellationToken);
		jira.Issues.Queryable.Where(i => i.Key == issue.Key).Any().Should().BeTrue("Expected issue in server");

		// Delete issue and verify it is no longer found.
		await jira.Issues.DeleteIssueAsync(issue.Key.Value, CancellationToken);
		var act = () => jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		await act.Should().ThrowExactlyAsync<AggregateException>();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndGetWorklogs(JiraClient jira)
	{
		var summaryValue = "Test issue with work logs" + RandomNumberGenerator.GetInt32(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);

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
		var summary = "Test issue with worklogs" + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summary,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);

		var worklog = await issue.AddWorklogAsync("1h", WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);
		(await issue.GetWorklogsAsync(CancellationToken)).Should().ContainSingle();

		await issue.DeleteWorklogAsync(worklog, WorklogStrategy.AutoAdjustRemainingEstimate, null, CancellationToken);
		(await issue.GetWorklogsAsync(CancellationToken)).Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemovePropertyAndVerifyProperties(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(CancellationToken);

		// Verify no properties exist
		var propertyKeys = await jira.Issues.GetPropertyKeysAsync(issue.Key.Value, CancellationToken);
		propertyKeys.Should().BeEmpty();

		// Set new property on issue
		var keyString = "test-property";
		var keyValue = JToken.FromObject("test-string");
		await issue.SetPropertyAsync(keyString, keyValue, CancellationToken);

		// Verify one property exists.
		propertyKeys = await jira.Issues.GetPropertyKeysAsync(issue.Key.Value, CancellationToken);
		propertyKeys.SequenceEqual([keyString]).Should().BeTrue();

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString, "non-existent-property"], CancellationToken);

		var truth = new Dictionary<string, JToken>()
			{
				{ keyString, keyValue },
			};

		issueProperties.Keys.SequenceEqual(truth.Keys).Should().BeTrue();
		issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()).Should().BeTrue();

		// Delete the property
		await issue.DeletePropertyAsync(keyString, CancellationToken);

		// Verify dictionary is empty
		issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);
		issueProperties.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task RemoveInexistantPropertyAndVerifyNoOp(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(CancellationToken);

		var keyString = "test-property-nonexist";
		await issue.DeletePropertyAsync(keyString, CancellationToken);

		// Verify the property isn't returned by the service
		var issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);
		issueProperties.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddNullPropertyAndVerify(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(CancellationToken);

		// Set new property on issue
		var keyString = "test-property-null";
		JToken keyValue = JToken.Parse("null");
		await issue.SetPropertyAsync(keyString, keyValue, CancellationToken);

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);
		var truth = new Dictionary<string, JToken>()
			{
                // WARN; JToken of null is effectively returned as null.
                // This probably depends on the serializersettings!
                { keyString, null },
			};

		issueProperties.Keys.SequenceEqual(truth.Keys).Should().BeTrue();
		issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()).Should().BeTrue();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]

	public async Task AddObjectPropertyAndVerify(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(CancellationToken);

		// Set new property on issue
		var keyString = "test-property-object";
		var valueObject = new
		{
			KeyName = "TestKey",
		};
		JToken keyValue = JToken.FromObject(valueObject);
		await issue.SetPropertyAsync(keyString, keyValue, CancellationToken);

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);

		var truth = new Dictionary<string, JToken>()
			{
				{ keyString, keyValue },
			};

		issueProperties.Keys.SequenceEqual(truth.Keys).Should().BeTrue();
		issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()).Should().BeTrue();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]

	public async Task AddBoolPropertyAndVerify(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(CancellationToken);

		// Set new property on issue
		var keyString = "test-property-bool";
		JToken keyValue = JToken.FromObject(true);
		await issue.SetPropertyAsync(keyString, keyValue, CancellationToken);

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);

		var truth = new Dictionary<string, JToken>()
			{
				{ keyString, keyValue },
			};

		issueProperties.Keys.SequenceEqual(truth.Keys).Should().BeTrue();
		issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()).Should().BeTrue();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]

	public async Task AddListPropertyAndVerify(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(CancellationToken);

		// Set new property on issue
		var keyString = "test-property-list";
		var valueObject = new List<string>() { "One", "Two", "Three" };
		JToken keyValue = JToken.FromObject(valueObject);
		await issue.SetPropertyAsync(keyString, keyValue, CancellationToken);

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString], CancellationToken);

		var truth = new Dictionary<string, JToken>()
			{
				{ keyString, keyValue },
			};

		issueProperties.Keys.SequenceEqual(truth.Keys).Should().BeTrue();
		issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()).Should().BeTrue();
	}
}



