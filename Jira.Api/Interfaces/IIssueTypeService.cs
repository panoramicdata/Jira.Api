namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue types of jira.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IIssueTypeService
{
	/// <summary>
	/// Returns all the issue types within JIRA.
	/// </summary>
	Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the issue types within JIRA for the project specified.
	/// </summary>
	Task<IEnumerable<IssueType>> GetIssueTypesForProjectAsync(string projectKey, CancellationToken cancellationToken = default);
}
