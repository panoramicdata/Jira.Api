using AwesomeAssertions;
using System.Security.Cryptography;

namespace Jira.Api.Test.Integration;

public class JiraUserTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private JiraUserCreationInfo BuildUserInfo()
	{
		var rand = RandomNumberGenerator.GetInt32(int.MaxValue);
		return new JiraUserCreationInfo()
		{
			Username = "test" + rand,
			DisplayName = "Test User " + rand,
			Email = $"test{rand}@user.com",
			Password = "MyPass" + rand
		};
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateGetAndDeleteUsers(JiraClient jira)
	{
		var userInfo = BuildUserInfo();

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, CancellationToken);
		userInfo.Email.Should().Be(user.Email);
		userInfo.DisplayName.Should().Be(user.DisplayName);
		userInfo.Username.Should().Be(user.Username);
		user.Key.Should().NotBeNull();
		user.Active.Should().BeTrue();
		user.Locale.Should().NotBeNullOrEmpty();

		// verify retrieve a user.
		user = await jira.Users.GetUserAsync(userInfo.Username, CancellationToken);
		userInfo.DisplayName.Should().Be(user.DisplayName);

		// verify search for a user
		var users = await jira.Users.SearchUsersAsync("test", JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().Contain(u => u.Username == userInfo.Username);

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, CancellationToken);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateGetAndDeleteUsersWithEmailAsUsername(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, CancellationToken);
		userInfo.Email.Should().Be(user.Email);
		userInfo.DisplayName.Should().Be(user.DisplayName);
		userInfo.Username.Should().Be(user.Username);
		user.Key.Should().NotBeNull();
		user.Active.Should().BeTrue();
		user.Locale.Should().NotBeNullOrEmpty();

		// verify retrieve a user.
		user = await jira.Users.GetUserAsync(userInfo.Username, CancellationToken);
		userInfo.DisplayName.Should().Be(user.DisplayName);

		// verify search for a user
		var users = await jira.Users.SearchUsersAsync("test", JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().Contain(u => u.Username == userInfo.Username);

		// verify equality override (see https://bitbucket.org/farmas/atlassian.net-sdk/issues/570)
		users.First().Equals(users.First()).Should().BeTrue();

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, CancellationToken);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().BeEmpty();
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
	public async Task SearchAssignableUsersForIssue(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, CancellationToken);
		user.Should().NotBeNull();

		// any user can be assigned to SCRUM issues.
		var users = await jira.Users.SearchAssignableUsersForIssueAsync("test", "SCRUM-1", 0, 50, CancellationToken);
		users.FirstOrDefault(u => u.Username == user.Username).Should().NotBeNull();

		// only developers can be assigned to TST issues.
		users = await jira.Users.SearchAssignableUsersForIssueAsync("test", "TST-1", 0, 50, CancellationToken);
		users.FirstOrDefault(u => u.Username == user.Username).Should().BeNull();

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, CancellationToken);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task SearchAssignableUsersForProject(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, CancellationToken);
		user.Should().NotBeNull();

		// any user can be assigned to SCRUM issues.
		var users = await jira.Users.SearchAssignableUsersForProjectAsync("test", "SCRUM", 0, 50, CancellationToken);
		users.FirstOrDefault(u => u.Username == user.Username).Should().NotBeNull();

		// only developers can be assigned to TST issues.
		users = await jira.Users.SearchAssignableUsersForProjectAsync("test", "TST", 0, 50, CancellationToken);
		users.FirstOrDefault(u => u.Username == user.Username).Should().BeNull();

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, CancellationToken);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task SearchAssignableUsersForProjects(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, CancellationToken);
		user.Should().NotBeNull();

		// test user is assignable because any user can be assigned to SCRUM issues.
		var users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["SCRUM"], 0, 50, CancellationToken);
		users.FirstOrDefault(u => u.Username == user.Username).Should().NotBeNull();

		// test user is not assignable because only developers can be assigned to TST issues.
		users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["TST"], 0, 50, CancellationToken);
		users.FirstOrDefault(u => u.Username == user.Username).Should().BeNull();

		// test user is not assignable because only developers can be assigned to both SCRUM and TST issues.
		users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["SCRUM", "TST"], 0, 50, CancellationToken);
		users.FirstOrDefault(u => u.Username == user.Username).Should().BeNull();

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, CancellationToken);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, CancellationToken);
		users.Should().BeEmpty();
	}
}



