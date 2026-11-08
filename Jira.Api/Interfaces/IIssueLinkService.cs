namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue link of jira.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
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
