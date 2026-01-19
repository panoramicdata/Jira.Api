namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue link of jira.
/// </summary>
public interface IIssueLinkService
{
	/// <summary>
	/// Returns all available issue link types.
	/// </summary>
	Task<IEnumerable<IssueLinkType>> GetLinkTypesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates an issue link between two issues.
	/// </summary>
	Task CreateLinkAsync(string outwardIssueKey, string inwardIssueKey, string linkName, string? comment = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all issue links associated with a given issue.
	/// </summary>
	Task<IEnumerable<IssueLink>> GetLinksForIssueAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all issue links associated with a given issue.
	/// </summary>
	Task<IEnumerable<IssueLink>> GetLinksForIssueAsync(Issue issue, IEnumerable<string>? linkTypeNames = null, CancellationToken cancellationToken = default);
}
