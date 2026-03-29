namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class JiraGroupTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndRemoveGroupWithUser(JiraClient jira)
	{
		// Create the group.
		var groupName = $"test-group-{RandomNumberGenerator.GetInt32(int.MaxValue)}";
		await jira.Groups.CreateGroupAsync(groupName, CancellationToken);

		// Add user to group
		await jira.Groups.AddUserAsync(groupName, "admin", CancellationToken);

		// Get users from group.
		var users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, CancellationToken);
		users.Should().Contain(u => u.Username == "admin");

		// Delete user from group.
		await jira.Groups.RemoveUserAsync(groupName, "admin", CancellationToken);
		users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, CancellationToken);
		users.Should().BeEmpty();

		// Delete group
		await jira.Groups.DeleteGroupAsync(groupName, null, CancellationToken);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndRemoveGroupWithSpecialCharacterAndUser(JiraClient jira)
	{
		// Create the group.
		var groupName = $"test-group-@@@@-{RandomNumberGenerator.GetInt32(int.MaxValue)}";
		await jira.Groups.CreateGroupAsync(groupName, CancellationToken);

		// Add user to group
		await jira.Groups.AddUserAsync(groupName, "admin", CancellationToken);

		// Get users from group.
		var users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, CancellationToken);
		users.Should().Contain(u => u.Username == "admin");

		// Delete user from group.
		await jira.Groups.RemoveUserAsync(groupName, "admin", CancellationToken);
		users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, CancellationToken);
		users.Should().BeEmpty();

		// Delete group
		await jira.Groups.DeleteGroupAsync(groupName, null, CancellationToken);
	}
}



