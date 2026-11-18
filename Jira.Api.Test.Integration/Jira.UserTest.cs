namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class JiraUserTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private static JiraUserCreationInfo BuildUserInfo()
	{
		var rand = RandomNumberGenerator.GetInt32(int.MaxValue);
		return new JiraUserCreationInfo
		{
			Username = "test" + rand,
			DisplayName = "Test User " + rand,
			Email = $"test{rand}@user.com",
			Password = "MyPass" + rand
		};
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	[Trait("Category", "WritesToApi")]
	public async Task CreateGetAndDeleteUsers(JiraClient jira)
	{
		var userInfo = BuildUserInfo();

		await CreateGetAndSearchUserAsync(jira, userInfo);

		await DeleteUserAndVerifyAsync(jira, userInfo.Username);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	[Trait("Category", "WritesToApi")]
	public async Task CreateGetAndDeleteUsersWithEmailAsUsername(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		var users = await CreateGetAndSearchUserAsync(jira, userInfo);

		// verify equality override (see https://bitbucket.org/farmas/atlassian.net-sdk/issues/570)
		users.First().Equals(users.First()).Should().BeTrue();

		await DeleteUserAndVerifyAsync(jira, userInfo.Username);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanAccessAvatarUrls(JiraClient jira)
	{
		var user = await jira.Users.GetUserAsync("admin", CancellationToken);
		user.AvatarUrls.Should().NotBeNull();
		user.AvatarUrls.XSmall.Should().NotBeNull();
		user.AvatarUrls.Small.Should().NotBeNull();
		user.AvatarUrls.Medium.Should().NotBeNull();
		user.AvatarUrls.Large.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	[Trait("Category", "WritesToApi")]
	public async Task SearchAssignableUsersForIssue(JiraClient jira)
	{
		var (userInfo, user) = await CreateUserWithEmailAsUsernameAsync(jira);

		// any user can be assigned to SCRUM issues.
		AssertAssignable(await jira.Users.SearchAssignableUsersForIssueAsync("test", "SCRUM-1", 0, 50, CancellationToken), user, true);

		// only developers can be assigned to TST issues.
		AssertAssignable(await jira.Users.SearchAssignableUsersForIssueAsync("test", "TST-1", 0, 50, CancellationToken), user, false);

		await DeleteUserAndVerifyAsync(jira, userInfo.Username);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	[Trait("Category", "WritesToApi")]
	public async Task SearchAssignableUsersForProject(JiraClient jira)
	{
		var (userInfo, user) = await CreateUserWithEmailAsUsernameAsync(jira);

		// any user can be assigned to SCRUM issues.
		AssertAssignable(await jira.Users.SearchAssignableUsersForProjectAsync("test", "SCRUM", 0, 50, CancellationToken), user, true);

		// only developers can be assigned to TST issues.
		AssertAssignable(await jira.Users.SearchAssignableUsersForProjectAsync("test", "TST", 0, 50, CancellationToken), user, false);

		await DeleteUserAndVerifyAsync(jira, userInfo.Username);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	[Trait("Category", "WritesToApi")]
	public async Task SearchAssignableUsersForProjects(JiraClient jira)
	{
		var (userInfo, user) = await CreateUserWithEmailAsUsernameAsync(jira);

		// any user can be assigned to SCRUM issues.
		AssertAssignable(await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["SCRUM"], 0, 50, CancellationToken), user, true);

		// only developers can be assigned to TST issues, alone or alongside SCRUM.
		AssertAssignable(await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["TST"], 0, 50, CancellationToken), user, false);
		AssertAssignable(await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["SCRUM", "TST"], 0, 50, CancellationToken), user, false);

		await DeleteUserAndVerifyAsync(jira, userInfo.Username);
	}

	private static async Task<IEnumerable<JiraUser>> CreateGetAndSearchUserAsync(JiraClient jira, JiraUserCreationInfo userInfo)
	{
		var user = await jira.Users.CreateUserAsync(userInfo, CancellationToken);
		userInfo.Email.Should().Be(user.Email);
		userInfo.DisplayName.Should().Be(user.DisplayName);
		userInfo.Username.Should().Be(user.Username);
		user.Key.Should().NotBeNull();
		user.Active.Should().BeTrue();
		user.Locale.Should().NotBeNullOrEmpty();

		user = await jira.Users.GetUserAsync(userInfo.Username, CancellationToken);
		userInfo.DisplayName.Should().Be(user.DisplayName);

		var users = await jira.Users.SearchUsersAsync("test", JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().Contain(u => u.Username == userInfo.Username);
		return users;
	}

	private static async Task<(JiraUserCreationInfo UserInfo, JiraUser User)> CreateUserWithEmailAsUsernameAsync(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		var user = await jira.Users.CreateUserAsync(userInfo, CancellationToken);
		user.Should().NotBeNull();
		return (userInfo, user);
	}

	private static void AssertAssignable(IEnumerable<JiraUser> users, JiraUser user, bool expected)
	{
		var match = users.FirstOrDefault(u => u.Username == user.Username);
		if (expected)
		{
			match.Should().NotBeNull();
		}
		else
		{
			match.Should().BeNull();
		}
	}

	private static async Task DeleteUserAndVerifyAsync(JiraClient jira, string username)
	{
		await jira.Users.DeleteUserAsync(username, CancellationToken);
		var users = await jira.Users.SearchUsersAsync(username, JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().BeEmpty();
	}
}
