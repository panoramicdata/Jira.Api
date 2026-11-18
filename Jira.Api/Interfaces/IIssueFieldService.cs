namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue link types of jira.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
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
