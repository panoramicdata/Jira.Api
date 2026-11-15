namespace Jira.Api.Test.Integration;

public partial class IssueOperationsTest
{
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
