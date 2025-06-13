using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations on the issue link types of jira.
/// </summary>
public interface IIssueFieldService
{
	/// <summary>
	/// Returns all custom fields within JIRA.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Returns custom fields within JIRA given the options specified.
	/// </summary>
	/// <param name="options">Options to fetch custom fields.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CustomFieldFetchOptions options, CancellationToken cancellationToken);

	/// <summary>
	/// Returns all custom fields within JIRA for the project specified.
	/// </summary>
	/// <param name="projectKey">The project key to retrieve all the custom fields from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	[Obsolete("Use GetCustomFieldsAsync(options) instead.")]
	Task<IEnumerable<CustomField>> GetCustomFieldsForProjectAsync(string projectKey, CancellationToken cancellationToken);
}
