using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class JiraUserTest
{
	private readonly Random _random = new();

	private JiraUserCreationInfo BuildUserInfo()
	{
		var rand = _random.Next(int.MaxValue);
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
		var user = await jira.Users.CreateUserAsync(userInfo, default);
		Assert.Equal(user.Email, userInfo.Email);
		Assert.Equal(user.DisplayName, userInfo.DisplayName);
		Assert.Equal(user.Username, userInfo.Username);
		Assert.NotNull(user.Key);
		Assert.True(user.Active);
		Assert.False(string.IsNullOrEmpty(user.Locale));

		// verify retrieve a user.
		user = await jira.Users.GetUserAsync(userInfo.Username, default);
		Assert.Equal(user.DisplayName, userInfo.DisplayName);

		// verify search for a user
		var users = await jira.Users.SearchUsersAsync("test", JiraUserStatus.Active, 50, 0, default);
		Assert.Contains(users, u => u.Username == userInfo.Username);

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, default);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, default);
		Assert.Empty(users);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateGetAndDeleteUsersWithEmailAsUsername(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, default);
		Assert.Equal(user.Email, userInfo.Email);
		Assert.Equal(user.DisplayName, userInfo.DisplayName);
		Assert.Equal(user.Username, userInfo.Username);
		Assert.NotNull(user.Key);
		Assert.True(user.Active);
		Assert.False(string.IsNullOrEmpty(user.Locale));

		// verify retrieve a user.
		user = await jira.Users.GetUserAsync(userInfo.Username, default);
		Assert.Equal(user.DisplayName, userInfo.DisplayName);

		// verify search for a user
		var users = await jira.Users.SearchUsersAsync("test", JiraUserStatus.Active, 50, 0, default);
		Assert.Contains(users, u => u.Username == userInfo.Username);

		// verify equality override (see https://bitbucket.org/farmas/atlassian.net-sdk/issues/570)
		Assert.True(users.First().Equals(users.First()));

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, default);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, default);
		Assert.Empty(users);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanAccessAvatarUrls(JiraClient jira)
	{
		var user = await jira.Users.GetUserAsync("admin", default);
		Assert.NotNull(user.AvatarUrls);
		Assert.NotNull(user.AvatarUrls.XSmall);
		Assert.NotNull(user.AvatarUrls.Small);
		Assert.NotNull(user.AvatarUrls.Medium);
		Assert.NotNull(user.AvatarUrls.Large);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task SearchAssignableUsersForIssue(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, default);
		Assert.NotNull(user);

		// any user can be assigned to SCRUM issues.
		var users = await jira.Users.SearchAssignableUsersForIssueAsync("test", "SCRUM-1", 0, 50, default);
		Assert.NotNull(users.FirstOrDefault(u => u.Username == user.Username));

		// only developers can be assigned to TST issues.
		users = await jira.Users.SearchAssignableUsersForIssueAsync("test", "TST-1", 0, 50, default);
		Assert.Null(users.FirstOrDefault(u => u.Username == user.Username));

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, default);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, default);
		Assert.Empty(users);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task SearchAssignableUsersForProject(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, default);
		Assert.NotNull(user);

		// any user can be assigned to SCRUM issues.
		var users = await jira.Users.SearchAssignableUsersForProjectAsync("test", "SCRUM", 0, 50, default);
		Assert.NotNull(users.FirstOrDefault(u => u.Username == user.Username));

		// only developers can be assigned to TST issues.
		users = await jira.Users.SearchAssignableUsersForProjectAsync("test", "TST", 0, 50, default);
		Assert.Null(users.FirstOrDefault(u => u.Username == user.Username));

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, default);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, default);
		Assert.Empty(users);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task SearchAssignableUsersForProjects(JiraClient jira)
	{
		var userInfo = BuildUserInfo();
		userInfo.Username = userInfo.Email;

		// verify create a user.
		var user = await jira.Users.CreateUserAsync(userInfo, default);
		Assert.NotNull(user);

		// test user is assignable because any user can be assigned to SCRUM issues.
		var users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["SCRUM"], 0, 50, default);
		Assert.NotNull(users.FirstOrDefault(u => u.Username == user.Username));

		// test user is not assignable because only developers can be assigned to TST issues.
		users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["TST"], 0, 50, default);
		Assert.Null(users.FirstOrDefault(u => u.Username == user.Username));

		// test user is not assignable because only developers can be assigned to both SCRUM and TST issues.
		users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", ["SCRUM", "TST"], 0, 50, default);
		Assert.Null(users.FirstOrDefault(u => u.Username == user.Username));

		// verify delete a user
		await jira.Users.DeleteUserAsync(userInfo.Username, default);
		users = await jira.Users.SearchUsersAsync(userInfo.Username, JiraUserStatus.Active, 50, 0, default);
		Assert.Empty(users);
	}
}
