namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class JiraGroupTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public Task CreateAndRemoveGroupWithUser(JiraClient jira)
		=> CreateAndRemoveGroupWithAdminAsync(jira, $"test-group-{RandomNumberGenerator.GetInt32(int.MaxValue)}");

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public Task CreateAndRemoveGroupWithSpecialCharacterAndUser(JiraClient jira)
		=> CreateAndRemoveGroupWithAdminAsync(jira, $"test-group-@@@@-{RandomNumberGenerator.GetInt32(int.MaxValue)}");

	private static async Task CreateAndRemoveGroupWithAdminAsync(JiraClient jira, string groupName)
	{
		await jira.Groups.CreateGroupAsync(groupName, CancellationToken);
		await jira.Groups.AddUserAsync(groupName, "admin", CancellationToken);

		var users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, CancellationToken);
		users.Should().Contain(u => u.Username == "admin");

		await jira.Groups.RemoveUserAsync(groupName, "admin", CancellationToken);
		users = await jira.Groups.GetUsersAsync(groupName, false, 0, 50, CancellationToken);
		users.Should().BeEmpty();

		await jira.Groups.DeleteGroupAsync(groupName, null, CancellationToken);
	}
}
