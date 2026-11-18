namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the remote links of a jira issue.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IIssueRemoteLinkService
{
	/// <summary>
	/// Creates a remote link for an issue.
	/// </summary>
	Task CreateRemoteLinkAsync(string issueKey, string remoteUrl, string title, string? summary = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all remote links associated with a given issue.
	/// </summary>
	Task<IEnumerable<IssueRemoteLink>> GetRemoteLinksForIssueAsync(string issueKey, CancellationToken cancellationToken = default);
}
