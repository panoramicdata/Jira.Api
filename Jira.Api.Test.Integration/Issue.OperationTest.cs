using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class IssueOperationsTest
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	async Task AssignIssue(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Type = "1";
		issue.Summary = "Test issue to assign" + _random.Next(int.MaxValue);

		await issue.SaveChangesAsync(default);
		Assert.Equal("admin", issue.Assignee);

		await issue.AssignAsync("test", default);
		Assert.Equal("test", issue.Assignee);

		issue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("test", issue.Assignee);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetChangeLogsForIssue(Jira jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", default);
		var changelogs = (await issue.GetChangeLogsAsync(default)).OrderBy(log => log.CreatedDate);
		Assert.True(changelogs.Count() >= 4);

		var firstChangeLog = changelogs.First();
		Assert.Equal("admin", firstChangeLog.Author.Username);
		//Assert.NotNull(firstChangeLog.CreatedDate); this can never be null
		Assert.Equal(2, firstChangeLog.Items.Count());

		var firstItem = firstChangeLog.Items.First();
		Assert.Equal("Attachment", firstItem.FieldName);
		Assert.Equal("jira", firstItem.FieldType);
		Assert.Null(firstItem.FromValue);
		Assert.Null(firstItem.FromId);
		Assert.NotNull(firstItem.ToId);
		Assert.Equal("SampleImage.png", firstItem.ToValue);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveWatchersToIssue(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Type = "1";
		issue.Summary = "Test issue with watchers" + _random.Next(int.MaxValue);
		await issue.SaveChangesAsync(default);

		await issue.AddWatcherAsync("test", default);
		Assert.Equal(2, (await issue.GetWatchersAsync(default)).Count());

		await issue.DeleteWatcherAsync("admin", default);
		Assert.Single(await issue.GetWatchersAsync(default));

		var user = (await issue.GetWatchersAsync(default)).First();
		Assert.Equal("test", user.Username);
		Assert.True(user.IsActive);
		Assert.Equal("Tester", user.DisplayName);
		Assert.Equal("test@qa.com", user.Email);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveWatchersToIssueWithEmailAsUsername(Jira jira)
	{
		// Create issue.
		var issue = jira.CreateIssue("TST");
		issue.Type = "1";
		issue.Summary = "Test issue with watchers" + _random.Next(int.MaxValue);
		await issue.SaveChangesAsync(default);

		// Create user with e-mail as username.
		var rand = _random.Next(int.MaxValue);
		var userInfo = new JiraUserCreationInfo()
		{
			Username = $"test{rand}@user.com",
			DisplayName = $"Test User {rand}",
			Email = $"test{rand}@user.com",
			Password = $"MyPass{rand}",
		};

		await jira.Users.CreateUserAsync(userInfo, default);

		// Add the user as a watcher on the issue.
		await issue.AddWatcherAsync(userInfo.Email, default);

		// Verify the watchers of the issue contains the username.
		var watchers = await issue.GetWatchersAsync(default);
		Assert.Contains(watchers, w => string.Equals(w.Username, userInfo.Username));

		// Delete user.
		await jira.Users.DeleteUserAsync(userInfo.Username, default);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetSubTasks(Jira jira)
	{
		var parentTask = jira.CreateIssue("TST");
		parentTask.Type = "1";
		parentTask.Summary = "Test issue with SubTask" + _random.Next(int.MaxValue);
		await parentTask.SaveChangesAsync(default);

		var subTask = jira.CreateIssue("TST", parentTask.Key.Value);
		subTask.Type = "5"; // SubTask issue type.
		subTask.Summary = "Test SubTask" + _random.Next(int.MaxValue);
		await subTask.SaveChangesAsync(default);

		var results = await parentTask.GetSubTasksAsync(0, null, default);
		Assert.Single(results);
		Assert.Equal(results.First().Summary, subTask.Summary);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task RetrieveEmptyIssueLinks(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue with no links " + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(default);

		Assert.Empty(await issue.GetIssueLinksAsync(default));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRetrieveIssueLinks(Jira jira)
	{
		var issue1 = jira.CreateIssue("TST");
		issue1.Summary = "Issue to link from" + _random.Next(int.MaxValue);
		issue1.Type = "Bug";
		await issue1.SaveChangesAsync(default);

		var issue2 = jira.CreateIssue("TST");
		issue2.Summary = "Issue to link to " + _random.Next(int.MaxValue);
		issue2.Type = "Bug";
		await issue2.SaveChangesAsync(default);

		var issue3 = jira.CreateIssue("TST");
		issue3.Summary = "Issue to link to " + _random.Next(int.MaxValue);
		issue3.Type = "Bug";
		await issue3.SaveChangesAsync(default);

		// link the 1st issue to 2 and 3.
		await issue1.LinkToIssueAsync(issue2.Key.Value, "Duplicate", null, default);
		await issue1.LinkToIssueAsync(issue3.Key.Value, "Related", null, default);

		// Verify links of 1st issue.
		var issueLinks = await issue1.GetIssueLinksAsync(default);
		Assert.Equal(2, issueLinks.Count());
		Assert.True(issueLinks.All(l => l.OutwardIssue.Key.Value == issue1.Key.Value));
		Assert.Contains(issueLinks, l => l.LinkType.Name == "Duplicate");
		Assert.Contains(issueLinks, l => l.LinkType.Name == "Related");
		Assert.Contains(issueLinks, l => l.InwardIssue.Key.Value == issue2.Key.Value);
		Assert.Contains(issueLinks, l => l.InwardIssue.Key.Value == issue3.Key.Value);

		// Verify link of 2nd issue.
		var issueLink = (await issue2.GetIssueLinksAsync(default)).Single();
		Assert.Equal("Duplicate", issueLink.LinkType.Name);
		Assert.Equal(issue1.Key.Value, issueLink.OutwardIssue.Key.Value);
		Assert.Equal(issue2.Key.Value, issueLink.InwardIssue.Key.Value);

		// Verify link of 3rd issue.
		issueLink = (await issue3.GetIssueLinksAsync(default)).Single();
		Assert.Equal("Related", issueLink.LinkType.Name);
		Assert.Equal(issue1.Key.Value, issueLink.OutwardIssue.Key.Value);
		Assert.Equal(issue3.Key.Value, issueLink.InwardIssue.Key.Value);

		// Verify retrieving subset of links of 1st issue
		var issueLinkOfType = (await issue1.GetIssueLinksAsync(["Duplicate"], default)).Single();
		Assert.Equal("Duplicate", issueLinkOfType.LinkType.Name);
		Assert.Equal(issue1.Key.Value, issueLinkOfType.OutwardIssue.Key.Value);
		Assert.Equal(issue2.Key.Value, issueLinkOfType.InwardIssue.Key.Value);

		issueLinkOfType = (await issue1.GetIssueLinksAsync(["Related"], default)).Single();
		Assert.Equal("Related", issueLinkOfType.LinkType.Name);
		Assert.Equal(issue1.Key.Value, issueLinkOfType.OutwardIssue.Key.Value);
		Assert.Equal(issue3.Key.Value, issueLinkOfType.InwardIssue.Key.Value);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRetrieveRemoteLinks(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to link from" + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(default);

		// Verify issue with no remote links.
		Assert.Empty(await issue.GetRemoteLinksAsync(default));

		var url1 = "https://google.com";
		var title1 = "Google";
		var summary1 = "Search engine";

		var url2 = "https://bing.com";
		var title2 = "Bing";

		// Add remote links
		await issue.AddRemoteLinkAsync(url1, title1, summary1, default);
		await issue.AddRemoteLinkAsync(url2, title2, null, default);

		// Verify remote links of issue.
		var remoteLinks = await issue.GetRemoteLinksAsync(default);
		Assert.Equal(2, remoteLinks.Count());
		Assert.Contains(remoteLinks, l => l.RemoteUrl == url1 && l.Title == title1 && l.Summary == summary1);
		Assert.Contains(remoteLinks, l => l.RemoteUrl == url2 && l.Title == title2 && l.Summary == null);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetActionsAsync(Jira jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", default);
		var transitions = await issue.GetAvailableActionsAsync(default);
		var resolveTransition = transitions.ElementAt(1);
		var resolveIssueStatus = resolveTransition.To;

		// assert
		Assert.Equal(3, transitions.Count());
		Assert.Equal("5", resolveTransition.Id);
		Assert.Equal("Resolve Issue", resolveTransition.Name);
		Assert.Equal("Resolved", resolveIssueStatus.Name);
		Assert.Equal("5", resolveIssueStatus.Id);
		Assert.Null(resolveTransition.Fields);

		var transition = transitions.Single(t => t.Name.Equals("Resolve Issue", StringComparison.OrdinalIgnoreCase));
		Assert.False(transition.HasScreen);
		Assert.False(transition.IsInitial);
		Assert.False(transition.IsInitial);
		Assert.False(transition.IsGlobal);
		Assert.Equal("Resolved", transition.To.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetActionsWithFields(Jira jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", default);
		var transitions = await issue.GetAvailableActionsAsync(true, default);
		var resolveTransition = transitions.ElementAt(1);
		var fields = resolveTransition.Fields;
		var resolution = fields["resolution"];
		var allowedValues = resolution.AllowedValues.ToString();

		// assert
		Assert.Equal(3, fields.Count);
		Assert.Equal("Resolution", resolution.Name);
		Assert.True(resolution.IsRequired);
		Assert.Single(resolution.Operations);
		Assert.Equal(IssueFieldEditMetadataOperation.SET, resolution.Operations.ElementAt(0));
		Assert.Contains("Fixed", allowedValues);
		Assert.Contains("Won't Fix", allowedValues);
		Assert.Contains("Duplicate", allowedValues);
		Assert.Contains("Incomplete", allowedValues);
		Assert.Contains("Cannot Reproduce", allowedValues);
		Assert.Contains("Done", allowedValues);
		Assert.Contains("Won't Do", allowedValues);
		Assert.False(resolution.HasDefaultValue);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueAsync(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(default);

		Assert.Null(issue.ResolutionDate);

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, default);

		Assert.Equal("Resolved", issue.Status.Name);
		Assert.Equal("Fixed", issue.Resolution.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueByIdAsync(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(default);

		var transitions = await issue.GetAvailableActionsAsync(default);
		var transition = transitions.Single(t => t.Name.Equals("Resolve Issue", StringComparison.OrdinalIgnoreCase));

		await issue.WorkflowTransitionAsync(transition.Id, null, default);

		Assert.Equal("Resolved", issue.Status.Name);
		Assert.Equal("Fixed", issue.Resolution.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TransitionIssueAsyncWithCommentAndFields(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve with async" + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(default);

		Assert.Null(issue.ResolutionDate);
		var updates = new WorkflowTransitionUpdates() { Comment = "Comment with transition" };
		await issue.FixVersions.AddAsync("2.0", default);

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, updates, CancellationToken.None);

		var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("Resolved", updatedIssue.Status.Name);
		Assert.Equal("Fixed", updatedIssue.Resolution.Name);
		Assert.Equal("2.0", updatedIssue.FixVersions.First().Name);

		var comments = await updatedIssue.GetCommentsAsync(default);
		Assert.Single(comments);
		Assert.Equal("Comment with transition", comments.First().Body);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task Transition_ResolveIssue(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(default);

		Assert.Null(issue.ResolutionDate);

		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, default);

		Assert.Equal("Resolved", issue.Status.Name);
		Assert.Equal("Fixed", issue.Resolution.Name);
		Assert.NotNull(issue.ResolutionDate);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task Transition_ResolveIssue_AsWontFix(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue to resolve " + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(default);

		issue.Resolution = "Won't Fix";
		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, default);

		Assert.Equal("Resolved", issue.Status.Name);
		Assert.Equal("Won't Fix", issue.Resolution.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetTimeTrackingDataForIssue(Jira jira)
	{
		var issue = jira.CreateIssue("TST");
		issue.Summary = "Issue with timetracking " + _random.Next(int.MaxValue);
		issue.Type = "Bug";
		await issue.SaveChangesAsync(default);

		var timetracking = await issue.GetTimeTrackingDataAsync(default);
		Assert.Null(timetracking.TimeSpent);

		await issue.AddWorklogAsync("2d", WorklogStrategy.AutoAdjustRemainingEstimate, null, default);

		timetracking = await issue.GetTimeTrackingDataAsync(default);
		Assert.Equal("2d", timetracking.TimeSpent);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetResolutionDate(Jira jira)
	{
		// Arrange
		var issue = jira.CreateIssue("TST");
		var currentDate = DateTime.Now;
		issue.Summary = "Issue to resolve " + Guid.NewGuid().ToString();
		issue.Type = "Bug";

		// Act, Assert: Returns null for unsaved issue.
		Assert.Null(issue.ResolutionDate);

		// Act, Assert: Returns null for saved unresolved issue.
		await issue.SaveChangesAsync(default);
		Assert.Null(issue.ResolutionDate);

		// Act, Assert: returns date for saved resolved issue.
		await issue.WorkflowTransitionAsync(WorkflowActions.Resolve, null, default);
		Assert.NotNull(issue.ResolutionDate);
		Assert.Equal(issue.ResolutionDate.Value.Year, currentDate.Year);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddGetRemoveAttachmentsFromIssue(Jira jira)
	{
		var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		// create an issue, verify no attachments
		await issue.SaveChangesAsync(default);
		Assert.Empty(await issue.GetAttachmentsAsync(default));

		// upload multiple attachments
		File.WriteAllText("testfile1.txt", "Test File Content 1");
		File.WriteAllText("testfile2.txt", "Test File Content 2");
		await issue.AddAttachmentAsync(new FileInfo[] { new("testfile1.txt"), new("testfile2.txt") }, default);

		// verify all attachments can be retrieved.
		var attachments = await issue.GetAttachmentsAsync(default);
		Assert.Equal(2, attachments.Count());
		Assert.True(attachments.Any(a => a.FileName.Equals("testfile1.txt")), "'testfile1.txt' was not downloaded from server");
		Assert.True(attachments.Any(a => a.FileName.Equals("testfile2.txt")), "'testfile2.txt' was not downloaded from server");

		// verify properties of an attachment
		var attachment = attachments.First();
		Assert.Equal("admin", attachment.Author);
		Assert.Equal("admin", attachment.AuthorUser.DisplayName);
		Assert.NotNull(attachment.CreatedDate);
		Assert.True(attachment.FileSize > 0);
		Assert.NotEmpty(attachment.MimeType);

		// download an attachment
		var tempFile = Path.GetTempFileName();
		var attachment2 = attachments.First(a => a.FileName.Equals("testfile1.txt"));
		await attachment2.DownloadAsync(tempFile, default);
		Assert.Equal("Test File Content 1", File.ReadAllText(tempFile));

		// remove an attachment
		await issue.DeleteAttachmentAsync(attachments.First(), default);
		Assert.Single(await issue.GetAttachmentsAsync(default));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DownloadAttachments(Jira jira)
	{
		// create an issue
		var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);

		// upload multiple attachments
		File.WriteAllText("testfile1.txt", "Test File Content 1");
		File.WriteAllText("testfile2.txt", "Test File Content 2");
		await issue.AddAttachmentAsync(new FileInfo[] { new("testfile1.txt"), new("testfile2.txt") }, default);

		// Get attachment metadata
		var attachments = await issue.GetAttachmentsAsync(CancellationToken.None);
		Assert.Equal(2, attachments.Count());
		Assert.True(attachments.Any(a => a.FileName.Equals("testfile1.txt")), "'testfile1.txt' was not downloaded from server");
		Assert.True(attachments.Any(a => a.FileName.Equals("testfile2.txt")), "'testfile2.txt' was not downloaded from server");

		// download an attachment.
		var tempFile = Path.GetTempFileName();
		var attachment = attachments.First(a => a.FileName.Equals("testfile1.txt"));

		await attachment.DownloadAsync(tempFile, default);
		Assert.Equal("Test File Content 1", File.ReadAllText(tempFile));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DownloadAttachmentData(Jira jira)
	{
		// create an issue
		var summaryValue = "Test Summary with attachment " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);

		// upload attachment
		File.WriteAllText("testfile.txt", "Test File Content");
		await issue.AddAttachmentAsync(new FileInfo("testfile.txt"), default);

		// Get attachment metadata
		var attachments = await issue.GetAttachmentsAsync(CancellationToken.None);
		Assert.Equal("testfile.txt", attachments.Single().FileName);

		// download attachment as byte array
		var bytes = await attachments.Single().DownloadDataAsync(default);

		Assert.Equal(17, bytes.Length);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndGetComments(Jira jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		// create an issue, verify no comments
		await issue.SaveChangesAsync(default);
		Assert.Empty(await issue.GetCommentsAsync(default));

		// Add a comment
		await issue.AddCommentAsync("new comment", default);

		var options = new CommentQueryOptions();
		options.Expand.Add("renderedBody");
		var comments = await issue.GetCommentsAsync(options, default);
		Assert.Single(comments);

		var comment = comments.First();
		Assert.Equal("new comment", comment.Body);
		Assert.Equal(DateTime.Now.Year, comment.CreatedDate.Value.Year);
		Assert.Null(comment.Visibility);
		Assert.Equal("new comment", comment.RenderedBody);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndUpdateComments(Jira jira)
	{
		var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		// Add a comment
		var comment = new Comment()
		{
			Author = (await jira.Users.GetMyselfAsync(default)).Username,
			Body = "New comment",
			Visibility = new CommentVisibility("Developers")
		};
		var newComment = await issue.AddCommentAsync(comment, default);

		// Verify
		Assert.Equal("role", newComment.Visibility.Type);
		Assert.Equal("Developers", newComment.Visibility.Value);
		Assert.Equal("admin", newComment.Author);
		Assert.Equal("admin", newComment.AuthorUser.DisplayName);
		Assert.Equal("admin", newComment.UpdateAuthor);
		Assert.Equal("admin", newComment.UpdateAuthorUser.DisplayName);

		// Update the comment
		newComment.Visibility.Value = "Users";
		newComment.Body = "changed body";
		var updatedComment = await issue.UpdateCommentAsync(newComment, default);

		// verify changes.
		Assert.Equal("role", updatedComment.Visibility.Type);
		Assert.Equal("Users", updatedComment.Visibility.Value);
		Assert.Equal("changed body", updatedComment.Body);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddGetAndDeleteCommentsAsync(Jira jira)
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

		// Delete comment.
		await issue.DeleteCommentAsync(commentFromGet, default);

		// Verify no comments
		comments = await issue.GetPagedCommentsAsync(0, null, default);
		Assert.Empty(comments);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanRetrievePagedCommentsAsync(Jira jira)
	{
		var summaryValue = "Test Summary with comments " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		// Add a comments
		await issue.AddCommentAsync("new comment1", default);
		await issue.AddCommentAsync("new comment2", default);
		await issue.AddCommentAsync("new comment3", default);
		await issue.AddCommentAsync("new comment4", default);

		// Verify first page of comments
		var comments = await issue.GetPagedCommentsAsync(2, 0, default);
		Assert.Equal(2, comments.Count());
		Assert.Equal("new comment1", comments.First().Body);
		Assert.Equal("new comment2", comments.Skip(1).First().Body);

		// Verify second page of comments
		comments = await issue.GetPagedCommentsAsync(2, 2, default);
		Assert.Equal(2, comments.Count());
		Assert.Equal("new comment3", comments.First().Body);
		Assert.Equal("new comment4", comments.Skip(1).First().Body);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DeleteIssue(Jira jira)
	{
		// Create issue and verify it is found in server.
		var issue = jira.CreateIssue("TST");
		issue.Type = "1";
		issue.Summary = $"Issue to delete ({_random.Next(int.MaxValue)})";
		await issue.SaveChangesAsync(default);
		Assert.True(jira.Issues.Queryable.Where(i => i.Key == issue.Key).Any(), "Expected issue in server");

		// Delete issue and verify it is no longer found.
		await jira.Issues.DeleteIssueAsync(issue.Key.Value, default);
		await Assert.ThrowsAsync<AggregateException>(() => jira.Issues.GetIssueAsync(issue.Key.Value, default));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndGetWorklogs(Jira jira)
	{
		var summaryValue = "Test issue with work logs" + _random.Next(int.MaxValue);

		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);

		await issue.AddWorklogAsync("1d", WorklogStrategy.AutoAdjustRemainingEstimate, null, default);
		await issue.AddWorklogAsync("1h", WorklogStrategy.RetainRemainingEstimate, null, default);
		await issue.AddWorklogAsync("1m", WorklogStrategy.NewRemainingEstimate, "2d", default);
		await issue.AddWorklogAsync(new Worklog("2d", new DateTime(2012, 1, 1), "comment"), WorklogStrategy.AutoAdjustRemainingEstimate, null, default);

		var logs = await issue.GetWorklogsAsync(default);
		Assert.Equal(4, logs.Count());
		Assert.Equal("comment", logs.ElementAt(3).Comment);
		Assert.Equal(new DateTime(2012, 1, 1), logs.ElementAt(3).StartDate);
		Assert.Equal("admin", logs.First().Author);
		Assert.Equal("admin", logs.First().AuthorUser.DisplayName);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task DeleteWorklog(Jira jira)
	{
		var summary = "Test issue with worklogs" + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summary,
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);

		var worklog = await issue.AddWorklogAsync("1h", WorklogStrategy.AutoAdjustRemainingEstimate, null, default);
		Assert.Single(await issue.GetWorklogsAsync(default));

		await issue.DeleteWorklogAsync(worklog, WorklogStrategy.AutoAdjustRemainingEstimate, null, default);
		Assert.Empty(await issue.GetWorklogsAsync(default));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemovePropertyAndVerifyProperties(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(default);

		// Verify no properties exist
		var propertyKeys = await jira.Issues.GetPropertyKeysAsync(issue.Key.Value, default);
		Assert.Empty(propertyKeys);

		// Set new property on issue
		var keyString = "test-property";
		var keyValue = JToken.FromObject("test-string");
		await issue.SetPropertyAsync(keyString, keyValue, default);

		// Verify one property exists.
		propertyKeys = await jira.Issues.GetPropertyKeysAsync(issue.Key.Value, default);
		Assert.True(propertyKeys.SequenceEqual([keyString]));

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString, "non-existent-property"], default);

		var truth = new Dictionary<string, JToken>()
			{
				{ keyString, keyValue },
			};

		Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
		Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));

		// Delete the property
		await issue.DeletePropertyAsync(keyString, default);

		// Verify dictionary is empty
		issueProperties = await issue.GetPropertiesAsync([keyString], default);
		Assert.False(issueProperties.Any());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task RemoveInexistantPropertyAndVerifyNoOp(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(default);

		var keyString = "test-property-nonexist";
		await issue.DeletePropertyAsync(keyString, default);

		// Verify the property isn't returned by the service
		var issueProperties = await issue.GetPropertiesAsync([keyString], default);
		Assert.False(issueProperties.Any());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddNullPropertyAndVerify(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(default);

		// Set new property on issue
		var keyString = "test-property-null";
		JToken keyValue = JToken.Parse("null");
		await issue.SetPropertyAsync(keyString, keyValue, default);

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString], default);
		var truth = new Dictionary<string, JToken>()
			{
                // WARN; JToken of null is effectively returned as null.
                // This probably depends on the serializersettings!
                { keyString, null },
			};

		Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
		Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]

	public async Task AddObjectPropertyAndVerify(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(default);

		// Set new property on issue
		var keyString = "test-property-object";
		var valueObject = new
		{
			KeyName = "TestKey",
		};
		JToken keyValue = JToken.FromObject(valueObject);
		await issue.SetPropertyAsync(keyString, keyValue, default);

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString], default);

		var truth = new Dictionary<string, JToken>()
			{
				{ keyString, keyValue },
			};

		Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
		Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]

	public async Task AddBoolPropertyAndVerify(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(default);

		// Set new property on issue
		var keyString = "test-property-bool";
		JToken keyValue = JToken.FromObject(true);
		await issue.SetPropertyAsync(keyString, keyValue, default);

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString], default);

		var truth = new Dictionary<string, JToken>()
			{
				{ keyString, keyValue },
			};

		Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
		Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]

	public async Task AddListPropertyAndVerify(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with properties",
			Assignee = "admin",
		};

		await issue.SaveChangesAsync(default);

		// Set new property on issue
		var keyString = "test-property-list";
		var valueObject = new List<string>() { "One", "Two", "Three" };
		JToken keyValue = JToken.FromObject(valueObject);
		await issue.SetPropertyAsync(keyString, keyValue, default);

		// Verify the property key returns the exact value
		var issueProperties = await issue.GetPropertiesAsync([keyString], default);

		var truth = new Dictionary<string, JToken>()
			{
				{ keyString, keyValue },
			};

		Assert.True(issueProperties.Keys.SequenceEqual(truth.Keys));
		Assert.True(issueProperties.Values.SequenceEqual(truth.Values, new JTokenEqualityComparer()));
	}
}
