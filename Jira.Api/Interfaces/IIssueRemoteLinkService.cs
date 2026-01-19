namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the remote links of a jira issue.
/// </summary>
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
