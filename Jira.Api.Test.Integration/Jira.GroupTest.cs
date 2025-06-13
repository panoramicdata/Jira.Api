using System;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class JiraGroupTest
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndRemoveGroupWithUser(JiraClient jira)
	{
		// Create the group.
		var groupName = $"test-group-{_random.Next(int.MaxValue)}";
		await jira.Groups.CreateGroupAsync(groupName, default);

		// Add user to group
		await jira.Groups.AddUserAsync(groupName, "admin", default);

		// Get users from group.
		var users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, default);
		Assert.Contains(users, u => u.Username == "admin");

		// Delete user from group.
		await jira.Groups.RemoveUserAsync(groupName, "admin", default);
		users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, default);
		Assert.Empty(users);

		// Delete group
		await jira.Groups.DeleteGroupAsync(groupName, null, default);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndRemoveGroupWithSpecialCharacterAndUser(JiraClient jira)
	{
		// Create the group.
		var groupName = $"test-group-@@@@-{_random.Next(int.MaxValue)}";
		await jira.Groups.CreateGroupAsync(groupName, default);

		// Add user to group
		await jira.Groups.AddUserAsync(groupName, "admin", default);

		// Get users from group.
		var users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, default);
		Assert.Contains(users, u => u.Username == "admin");

		// Delete user from group.
		await jira.Groups.RemoveUserAsync(groupName, "admin", default);
		users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, default);
		Assert.Empty(users);

		// Delete group
		await jira.Groups.DeleteGroupAsync(groupName, null, default);
	}
}
