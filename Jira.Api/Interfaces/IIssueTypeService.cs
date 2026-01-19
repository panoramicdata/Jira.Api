namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue types of jira.
/// </summary>
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
