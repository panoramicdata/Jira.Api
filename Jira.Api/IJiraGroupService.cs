using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations on the user groups of jira.
/// </summary>
public interface IJiraGroupService
{
	/// <summary>
	/// Creates a new user group.
	/// </summary>
	/// <param name="groupName">Name of group to create.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task CreateGroupAsync(string groupName, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes the group specified.
	/// </summary>
	/// <param name="groupName">Name of group to delete.</param>
	/// <param name="swapGroupName">Optional group name to transfer the restrictions (comments and worklogs only) to.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteGroupAsync(string groupName, string? swapGroupName, CancellationToken cancellationToken);

	/// <summary>
	/// Returns users that are members of the group specified.
	/// </summary>
	/// <param name="groupName">The name of group to return users for.</param>
	/// <param name="includeInactiveUsers">Whether to include inactive users.</param>
	/// <param name="skip">Index of the first user in group to return (0 based).</param>
	/// <param name="take">the maximum number of users to return.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IPagedQueryResult<JiraUser>> GetUsersAsync(
		string groupName,
		bool includeInactiveUsers,
		int skip,
		int take,
		CancellationToken cancellationToken);

	/// <summary>
	/// Adds a user to a the group specified.
	/// </summary>
	/// <param name="groupName">Name of group to add the user to.</param>
	/// <param name="usernameOrAccountId">User name or account id of user to add.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task AddUserAsync(string groupName, string usernameOrAccountId, CancellationToken cancellationToken);

	/// <summary>
	/// Removes a user from the group specified.
	/// </summary>
	/// <param name="groupName">Name of the group to remove the user from.</param>
	/// <param name="usernameOrAccountId">Username or account id of user to remove.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task RemoveUserAsync(string groupName, string usernameOrAccountId, CancellationToken cancellationToken);
}
