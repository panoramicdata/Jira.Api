namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue link types of jira.
/// </summary>
public interface IIssueFieldService
{
	/// <summary>
	/// Returns all custom fields within JIRA.
	/// </summary>
	Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns custom fields within JIRA given the options specified.
	/// </summary>
	Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CustomFieldFetchOptions options, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all custom fields within JIRA for the project specified.
	/// </summary>
	[Obsolete("Use GetCustomFieldsAsync(options) instead.")]
	Task<IEnumerable<CustomField>> GetCustomFieldsForProjectAsync(string projectKey, CancellationToken cancellationToken = default);
}
