using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations on the users of jira.
/// </summary>
public interface IJiraUserService
{
	/// <summary>
	/// Retrieve user specified by username.
	/// </summary>
	/// <param name="usernameOrAccountId">The username or account id of the user to get.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<JiraUser> GetUserAsync(
		string usernameOrAccountId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a user by the given username.
	/// </summary>
	/// <param name="usernameOrAccountId">User name or account id of user to delete.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteUserAsync(
		string usernameOrAccountId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Returns a list of users that match the search string.
	/// </summary>
	/// <param name="query">String used to search username, name or e-mail address.</param>
	/// <param name="userStatus">The status(es) of users to include in the result.  Suggest JiraUserStatus.Active</param>
	/// <param name="take">Maximum number of users to return (suggest 50). The maximum allowed value is 1000. If you specify a value that is higher than this number, your search results will be truncated.</param>
	/// <param name="skip">Index of the first user to return (0-based).</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<JiraUser>> SearchUsersAsync(
		string query,
		JiraUserStatus userStatus,
		int skip,
		int take,
		CancellationToken cancellationToken);

	/// <summary>
	/// Searches assignable users for an issue.
	/// </summary>
	/// <param name="username">The username.</param>
	/// <param name="issueKey">The issue key.</param>
	/// <param name="skip">Index of the first user to return (0-based).</param>
	/// <param name="take">The maximum results.  Suggest 50.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<JiraUser>> SearchAssignableUsersForIssueAsync(
		string username,
		string issueKey,
		int skip,
		int take,
		CancellationToken cancellationToken);

	/// <summary>
	/// Searches assignable users for a project.
	/// </summary>
	/// <param name="username">The username.</param>
	/// <param name="projectKey">The project key.</param>
	/// <param name="skip">Index of the first user to return (0-based).</param>
	/// <param name="take">The maximum results.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<JiraUser>> SearchAssignableUsersForProjectAsync(
		string username,
		string projectKey,
		int skip,
		int take,
		CancellationToken cancellationToken);

	/// <summary>
	/// Searches the assignable users for a list of projects.
	/// </summary>
	/// <param name="username">The username.</param>
	/// <param name="projectKeys">The project keys.</param>
	/// <param name="skip">The start at.</param>
	/// <param name="take">Maximum number of users to return (suggest 50). The
	/// maximum allowed value is 1000. If you specify a value that is higher than this number,
	/// your search results will be truncated.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<JiraUser>> SearchAssignableUsersForProjectsAsync(
		string username,
		IEnumerable<string> projectKeys,
		int skip,
		int take,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a user.
	/// </summary>
	/// <param name="user">The information about the user to be created.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<JiraUser> CreateUserAsync(JiraUserCreationInfo user, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieve user currently connected.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<JiraUser> GetMyselfAsync(CancellationToken cancellationToken);
}
