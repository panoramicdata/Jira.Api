namespace Jira.Api.Test;

public class RestServiceRequestTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private const string EmptyPage = "{\"startAt\":0,\"maxResults\":50,\"total\":0,\"values\":[],\"comments\":[]}";
	private const string UserJson = "{\"name\":\"bob\",\"accountId\":\"acc-1\",\"displayName\":\"Bob\",\"emailAddress\":\"bob@example.com\",\"active\":true}";
	private const string CommentJson = "{\"id\":\"7\",\"body\":\"hello\",\"author\":{\"name\":\"bob\"}}";
	private const string WorklogJson = "{\"id\":\"9\",\"timeSpent\":\"1h\",\"timeSpentSeconds\":3600}";

	public static TheoryData<string, Func<JiraClient, Task>, Method, string> Requests => new()
	{
		{ UserJson, j => j.Users.CreateUserAsync(new JiraUserCreationInfo { Username = "bob", DisplayName = "Bob", Email = "b@x.com", Password = "p" }, CancellationToken), Method.Post, "rest/api/2/user" },
		{ "{}", j => j.Users.DeleteUserAsync("bob", CancellationToken), Method.Delete, "rest/api/2/user?username=bob" },
		{ UserJson, j => j.Users.GetUserAsync("bob", CancellationToken), Method.Get, "rest/api/2/user?username=bob" },
		{ "[]", j => j.Users.SearchUsersAsync("bob", JiraUserStatus.Active, 0, 10, CancellationToken), Method.Get, "rest/api/2/user/search?username=bob&includeActive=True&includeInactive=False&startAt=0&maxResults=10" },
		{ "[]", j => j.Users.SearchAssignableUsersForIssueAsync("bob", "ABC-1", 0, 10, CancellationToken), Method.Get, "rest/api/2/user/assignable/search?username=bob&issueKey=ABC-1&startAt=0&maxResults=10" },
		{ "[]", j => j.Users.SearchAssignableUsersForProjectAsync("bob", "ABC", 0, 10, CancellationToken), Method.Get, "rest/api/2/user/assignable/search?username=bob&project=ABC&startAt=0&maxResults=10" },
		{ "[]", j => j.Users.SearchAssignableUsersForProjectsAsync("bob", ["A", "B"], 0, 10, CancellationToken), Method.Get, "rest/api/2/user/assignable/multiProjectSearch?username=bob&projectKeys=A,B&startAt=0&maxResults=10" },
		{ "{}", j => j.Groups.AddUserAsync("devs", "bob", CancellationToken), Method.Post, "rest/api/2/group/user?groupName=devs" },
		{ "{}", j => j.Groups.CreateGroupAsync("devs", CancellationToken), Method.Post, "rest/api/2/group" },
		{ "{}", j => j.Groups.DeleteGroupAsync("devs", null, CancellationToken), Method.Delete, "rest/api/2/group?groupName=devs" },
		{ "{}", j => j.Groups.DeleteGroupAsync("devs", "ops", CancellationToken), Method.Delete, "rest/api/2/group?groupName=devs&swapGroup=ops" },
		{ "{}", j => j.Groups.RemoveUserAsync("devs", "bob", CancellationToken), Method.Delete, "rest/api/2/group/user?groupName=devs&username=bob" },
		{ EmptyPage, j => j.Groups.GetUsersAsync("devs", true, 0, 50, CancellationToken), Method.Get, "rest/api/2/group/member?groupName=devs&includeInactiveUsers=True&startAt=0&maxResults=50" },
		{ "{}", j => j.Links.CreateLinkAsync("A-1", "A-2", "Blocks", "why", CancellationToken), Method.Post, "rest/api/2/issueLink" },
		{ "{}", j => j.Links.CreateLinkAsync("A-1", "A-2", "Blocks", null, CancellationToken), Method.Post, "rest/api/2/issueLink" },
		{ "{}", j => j.RemoteLinks.CreateRemoteLinkAsync("A-1", "http://x.test", "title", "summary", CancellationToken), Method.Post, "rest/api/2/issue/A-1/remotelink" },
		{ "[{\"object\":{\"title\":\"t\",\"url\":\"http://x.test\",\"summary\":\"s\"}}]", j => j.RemoteLinks.GetRemoteLinksForIssueAsync("A-1", CancellationToken), Method.Get, "rest/api/2/issue/A-1/remotelink" },
		{ "[{\"id\":\"1\",\"name\":\"F\",\"jql\":\"project = A\"}]", j => j.Filters.GetFavouritesAsync(CancellationToken), Method.Get, "rest/api/2/filter/favourite" },
		{ "{\"id\":\"1\",\"name\":\"F\",\"jql\":\"project = A\"}", j => j.Filters.GetFilterAsync("1", CancellationToken), Method.Get, "rest/api/2/filter/1" },
		{ CommentJson, j => j.Issues.AddCommentAsync("A-1", new Comment { Author = "bob", Body = "hello" }, CancellationToken), Method.Post, "rest/api/2/issue/A-1/comment" },
		{ "{\"comments\":[" + CommentJson + "]}", j => j.Issues.GetCommentsAsync("A-1", CancellationToken), Method.Get, "rest/api/2/issue/A-1/comment?expand=properties" },
		{ "{}", j => j.Issues.DeleteCommentAsync("A-1", "7", CancellationToken), Method.Delete, "rest/api/2/issue/A-1/comment/7" },
		{ "{\"transitions\":[{\"id\":\"5\",\"name\":\"Done\"}]}", j => j.Issues.GetActionsAsync("A-1", true, CancellationToken), Method.Get, "rest/api/2/issue/A-1/transitions?expand=transitions.fields" },
		{ "{\"transitions\":[]}", j => j.Issues.GetActionsAsync("A-1", CancellationToken), Method.Get, "rest/api/2/issue/A-1/transitions" },
		{ "{\"fields\":{\"attachment\":[{\"id\":\"1\",\"filename\":\"a.txt\"}]}}", j => j.Issues.GetAttachmentsAsync("A-1", CancellationToken), Method.Get, "rest/api/2/issue/A-1?fields=attachment" },
		{ "{\"id\":\"1\",\"filename\":\"a.txt\"}", j => j.Issues.GetAttachmentAsync("1", CancellationToken), Method.Get, "rest/api/2/attachment/1" },
		{ "{}", j => j.Issues.DeleteAttachmentAsync("A-1", "1", CancellationToken), Method.Delete, "rest/api/2/attachment/1" },
		{ "{}", j => j.Issues.AddAttachmentsAsync("A-1", [new UploadAttachmentInfo("a.txt", [1, 2])], CancellationToken), Method.Post, "rest/api/2/issue/A-1/attachments" },
		{ "{\"watchers\":[" + UserJson + "]}", j => j.Issues.GetWatchersAsync("A-1", CancellationToken), Method.Get, "rest/api/2/issue/A-1/watchers" },
		{ "{}", j => j.Issues.AddWatcherAsync("A-1", "bob", CancellationToken), Method.Post, "rest/api/2/issue/A-1/watchers" },
		{ "{}", j => j.Issues.DeleteWatcherAsync("A-1", "bob", CancellationToken), Method.Delete, "rest/api/2/issue/A-1/watchers?username=bob" },
		{ "{\"changelog\":{\"histories\":[{\"id\":\"1\",\"items\":[]}]}}", j => j.Issues.GetChangeLogsAsync("A-1", CancellationToken), Method.Get, "rest/api/2/issue/A-1?fields=created&expand=changelog" },
		{ "{\"worklogs\":[" + WorklogJson + "]}", j => j.Issues.GetWorklogsAsync("A-1", CancellationToken), Method.Get, "rest/api/2/issue/A-1/worklog" },
		{ WorklogJson, j => j.Issues.GetWorklogAsync("A-1", "9", CancellationToken), Method.Get, "rest/api/2/issue/A-1/worklog/9" },
		{ WorklogJson, j => j.Issues.AddWorklogAsync("A-1", new Worklog("1h", new DateTime(2025, 1, 1)), WorklogStrategy.RetainRemainingEstimate, null, CancellationToken), Method.Post, "rest/api/2/issue/A-1/worklog?adjustEstimate=leave" },
		{ WorklogJson, j => j.Issues.AddWorklogAsync("A-1", new Worklog("1h", new DateTime(2025, 1, 1)), WorklogStrategy.NewRemainingEstimate, "2h", CancellationToken), Method.Post, "rest/api/2/issue/A-1/worklog?adjustEstimate=new&newEstimate=2h" },
		{ "{}", j => j.Issues.DeleteWorklogAsync("A-1", "9", WorklogStrategy.NewRemainingEstimate, "2h", CancellationToken), Method.Delete, "rest/api/2/issue/A-1/worklog/9?adjustEstimate=new&newEstimate=2h" },
		{ "{}", j => j.Issues.DeleteWorklogAsync("A-1", "9", WorklogStrategy.RetainRemainingEstimate, null, CancellationToken), Method.Delete, "rest/api/2/issue/A-1/worklog/9?adjustEstimate=leave" },
		{ "{}", j => j.Issues.DeleteIssueAsync("A-1", CancellationToken), Method.Delete, "rest/api/2/issue/A-1" },
		{ "{}", j => j.Issues.AssignIssueAsync("A-1", "bob", CancellationToken), Method.Put, "/rest/api/2/issue/A-1/assignee" },
		{ "{\"keys\":[{\"key\":\"k1\"}]}", j => j.Issues.GetPropertyKeysAsync("A-1", CancellationToken), Method.Get, "rest/api/2/issue/A-1/properties" },
		{ "{\"key\":\"k1\",\"value\":{\"a\":1}}", j => j.Issues.GetPropertiesAsync("A-1", ["k1"], CancellationToken), Method.Get, "rest/api/2/issue/A-1/properties/k1" },
		{ "{}", j => j.Issues.SetPropertyAsync("A-1", "k1", JToken.Parse("{\"a\":1}"), CancellationToken), Method.Put, "rest/api/2/issue/A-1/properties/k1" },
		{ "{}", j => j.Issues.DeletePropertyAsync("A-1", "k1", CancellationToken), Method.Delete, "rest/api/2/issue/A-1/properties/k1" },
		{ "{}", j => j.Issues.ReRankAsync(["A-1"], "A-2", null, CancellationToken), Method.Put, "rest/agile/1.0/issue/rank" }
	};

	[Theory]
	[MemberData(nameof(Requests))]
	public async Task Service_SendsExpectedRequest(string response, Func<JiraClient, Task> call, Method method, string resource)
	{
		var (rest, jira) = RoutedRestClient.Create(response);

		await call(jira);

		rest.Calls.Should().Contain((method, resource));
	}

	[Fact]
	public async Task GetMyselfAsync_CachesTheCurrentUser()
	{
		var (rest, jira) = RoutedRestClient.Create(UserJson);

		var first = await jira.Users.GetMyselfAsync(CancellationToken);
		var second = await jira.Users.GetMyselfAsync(CancellationToken);

		second.Should().BeSameAs(first);
		rest.Calls.Should().ContainSingle();
		first.Username.Should().Be("bob");
	}

	[Fact]
	public async Task GetPagedCommentsAsync_ParsesComments()
	{
		var (rest, jira) = RoutedRestClient.Create("{\"startAt\":5,\"maxResults\":1,\"total\":9,\"comments\":[" + CommentJson + "]}");

		var page = await jira.Issues.GetPagedCommentsAsync("A-1", 5, 1, CancellationToken);

		rest.Calls.Should().Contain((Method.Get, "rest/api/2/issue/A-1/comment?startAt=5&maxResults=1"));
		page.TotalItems.Should().Be(9);
		page.StartAt.Should().Be(5);
		page.Single().Body.Should().Be("hello");
	}

	[Fact]
	public async Task GetLinkTypesAsync_CachesResult()
	{
		var (rest, jira) = RoutedRestClient.Create("{\"issueLinkTypes\":[{\"id\":\"1\",\"name\":\"Blocks\",\"inward\":\"is blocked by\",\"outward\":\"blocks\"}]}");

		var first = (await jira.Links.GetLinkTypesAsync(CancellationToken)).ToArray();
		_ = await jira.Links.GetLinkTypesAsync(CancellationToken);

		first.Should().ContainSingle().Which.Name.Should().Be("Blocks");
		rest.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task GetProperties_SkipsMissingProperties()
	{
		var (_, jira) = RoutedRestClient.Create((_, resource) => resource.EndsWith("/missing", StringComparison.Ordinal)
			? RoutedRestClient.Json("{}", System.Net.HttpStatusCode.NotFound)
			: RoutedRestClient.Json("{\"key\":\"k1\",\"value\":{\"a\":1}}"));

		var properties = await jira.Issues.GetPropertiesAsync("A-1", ["k1", "missing"], CancellationToken);

		properties.Should().ContainSingle().Which.Key.Should().Be("k1");
	}

	[Fact]
	public async Task DeletePropertyAsync_IgnoresMissingProperty()
	{
		var (_, jira) = RoutedRestClient.Create("{}", System.Net.HttpStatusCode.NotFound);

		var act = () => jira.Issues.DeletePropertyAsync("A-1", "k1", CancellationToken);

		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task PrivacyMode_UsesAccountIdParameters()
	{
		var settings = new JiraRestClientSettings("Test/1.0") { EnableUserPrivacyMode = true };
		var (rest, jira) = RoutedRestClient.Create("{}", settings: settings);

		await jira.Users.GetUserAsync("acc-1", CancellationToken);
		await jira.Groups.AddUserAsync("devs", "acc-1", CancellationToken);
		await jira.Issues.DeleteWatcherAsync("A-1", "acc-1", CancellationToken);

		rest.Calls.Should().Contain((Method.Get, "rest/api/2/user?accountId=acc-1"));
		rest.Calls.Should().Contain((Method.Delete, "rest/api/2/issue/A-1/watchers?accountId=acc-1"));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(256)]
	public async Task SetPropertyAsync_RejectsInvalidKeyLength(int length)
	{
		var (_, jira) = RoutedRestClient.Create("{}");
		var key = new string('x', length);

		var act = () => jira.Issues.SetPropertyAsync("A-1", key, JToken.Parse("{}"), CancellationToken);

		await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
	}

	[Theory]
	[InlineData(false, null, null)]
	[InlineData(true, "A", "B")]
	[InlineData(true, null, null)]
	public async Task ReRankAsync_ValidatesArguments(bool hasKeys, string? before, string? after)
	{
		var (_, jira) = RoutedRestClient.Create("{}");
		var keys = hasKeys ? new[] { "A-1" } : [];

		var act = () => jira.Issues.ReRankAsync(keys, before, after, CancellationToken);

		await act.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task AddCommentAsync_WithoutAuthor_Throws()
	{
		var (_, jira) = RoutedRestClient.Create("{}");

		var act = () => jira.Issues.AddCommentAsync("A-1", new Comment(), CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
	}

	[Fact]
	public async Task UpdateCommentAsync_WithoutId_Throws()
	{
		var (_, jira) = RoutedRestClient.Create("{}");

		var act = () => jira.Issues.UpdateCommentAsync("A-1", new Comment { Author = "bob" }, CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
	}

	[Fact]
	public async Task GetWatchersAsync_WithoutKey_Throws()
	{
		var (_, jira) = RoutedRestClient.Create("{}");

		var act = () => jira.Issues.GetWatchersAsync(string.Empty, CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
	}
}
