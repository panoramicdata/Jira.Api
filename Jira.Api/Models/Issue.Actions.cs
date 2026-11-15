using Jira.Api.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Jira.Api.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public partial class Issue
{
	/// <summary>
	/// Updates all fields from server.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task RefreshAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to refresh, issue has not been saved to server.");
		}

		var serverIssue = await Jira.Issues.GetIssueAsync(OriginalRemoteIssue.key, cancellationToken).ConfigureAwait(false);
		Initialize(serverIssue.OriginalRemoteIssue);
	}

	/// <summary>
	/// Gets the workflow actions that the issue can be transitioned to.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueTransition>> GetAvailableActionsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve actions, issue has not been saved to server.");
		}

		return Jira.Issues.GetActionsAsync(OriginalRemoteIssue.key, cancellationToken);
	}

	/// <summary>
	/// Gets the workflow actions that the issue can be transitioned to including the fields that are required per action.
	/// </summary>
	/// <param name="expandTransitionFields">Whether to expand the transition fields in the response.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueTransition>> GetAvailableActionsAsync(bool expandTransitionFields, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve actions, issue has not been saved to server.");
		}

		return Jira.Issues.GetActionsAsync(OriginalRemoteIssue.key, expandTransitionFields, cancellationToken);
	}

	/// <summary>
	/// Gets time tracking information for this issue.
	/// </summary>
	/// <remarks>
	/// - Returns information as it was at the moment the issue was read from server, to get latest data use the GetTimeTrackingData method.
	/// - Use the AddWorklog methods to edit the time tracking information.
	/// </remarks>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IssueTimeTrackingData> GetTimeTrackingDataAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve time tracking data, issue has not been saved to server.");
		}

		return Jira.Issues.GetTimeTrackingDataAsync(OriginalRemoteIssue.key, cancellationToken);
	}

	/// <summary>
	/// Adds a user to the watchers of the issue.
	/// </summary>
	/// <param name="usernameOrAccountId">Username or account id of the user to add as a watcher.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task AddWatcherAsync(string usernameOrAccountId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to add watcher, issue has not been saved to server.");
		}

		return Jira.Issues.AddWatcherAsync(OriginalRemoteIssue.key, usernameOrAccountId, cancellationToken);
	}

	/// <summary>
	/// Gets the users that are watching the issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<JiraUser>> GetWatchersAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to get watchers, issue has not been saved to server.");
		}

		return Jira.Issues.GetWatchersAsync(OriginalRemoteIssue.key, cancellationToken);
	}

	/// <summary>
	/// Removes a user from the watchers of the issue.
	/// </summary>
	/// <param name="usernameOrAccountId">Username or account id of the user to remove as a watcher.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task DeleteWatcherAsync(string usernameOrAccountId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to remove watcher, issue has not been saved to server.");
		}

		return Jira.Issues.DeleteWatcherAsync(OriginalRemoteIssue.key, usernameOrAccountId, cancellationToken);
	}

	/// <summary>
	/// Assigns this issue to the specified user.
	/// </summary>
	/// <param name="assignee">The username or account id of the user to assign this issue to.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task AssignAsync(string assignee, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to assign issue, issue has not been saved to server.");
		}

		await Jira.Issues.AssignIssueAsync(Key.Value, assignee, cancellationToken);
		var issue = await Jira.Issues.GetIssueAsync(OriginalRemoteIssue.key, cancellationToken).ConfigureAwait(false);
		Initialize(issue.OriginalRemoteIssue);
	}

	/// <summary>
	/// Fetches the requested properties and their mapping.
	/// </summary>
	/// <remarks>
	/// Property keys yielded by <paramref name="propertyKeys"/> must have a length between 0 and 256 (both exclusive).
	/// </remarks>
	/// <param name="propertyKeys">Enumeration of requested property keys.</param>
	/// <param name="cancellationToken">Asynchronous operation control token.</param>
	/// <returns>A dictionary of property values mapped to their keys.</returns>
	public Task<ReadOnlyDictionary<string, JToken>> GetPropertiesAsync(IEnumerable<string> propertyKeys, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to fetch issue properties, issue has not been saved to server.");
		}

		return Jira.Issues.GetPropertiesAsync(OriginalRemoteIssue.key, propertyKeys, cancellationToken);
	}

	/// <summary>
	/// Sets the value of the specified property key. The key is created if it didn't already exist.
	/// </summary>
	/// <remarks>
	/// The property key (<paramref name="propertyKey"/>) must have a length between 0 and 256 (both exclusive).
	/// </remarks>
	/// <param name="propertyKey">The property key.</param>
	/// <param name="obj">The value to store.</param>
	/// <param name="cancellationToken">Asynchronous operation control token.</param>
	public Task SetPropertyAsync(string propertyKey, JToken obj, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to add issue properties, issue has not been saved to server.");
		}

		return Jira.Issues.SetPropertyAsync(OriginalRemoteIssue.key, propertyKey, obj, cancellationToken);
	}

	/// <summary>
	/// Remove the specified property key and its stored value.
	/// </summary>
	/// <remarks>
	/// The property key (<paramref name="propertyKey"/>) must have a length between 0 and 256 (both exclusive).
	/// </remarks>
	/// <param name="propertyKey">The property key.</param>
	/// <param name="cancellationToken">Asynchronous operation control token.</param>
	public Task DeletePropertyAsync(string propertyKey, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to remove issue properties, issue has not been saved to server.");
		}

		return Jira.Issues.DeletePropertyAsync(OriginalRemoteIssue.key, propertyKey, cancellationToken);
	}
}
