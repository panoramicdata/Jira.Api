using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations on the issue link of jira.
/// </summary>
public interface IIssueLinkService
{
	/// <summary>
	/// Returns all available issue link types.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<IssueLinkType>> GetLinkTypesAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Creates an issue link between two issues.
	/// </summary>
	/// <param name="outwardIssueKey">Key of the outward issue.</param>
	/// <param name="inwardIssueKey">Key of the inward issue.</param>
	/// <param name="linkName">Name of the issue link.</param>
	/// <param name="comment">Comment to add to the outward issue.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task CreateLinkAsync(string outwardIssueKey, string inwardIssueKey, string linkName, string? comment, CancellationToken cancellationToken);

	/// <summary>
	/// Returns all issue links associated with a given issue.
	/// </summary>
	/// <param name="issueKey">The issue key to retrieve links for.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<IssueLink>> GetLinksForIssueAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Returns all issue links associated with a given issue.
	/// </summary>
	/// <param name="issue">The issue to retrieve links for.</param>
	/// <param name="linkTypeNames">Optional subset of link types to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<IssueLink>> GetLinksForIssueAsync(Issue issue, IEnumerable<string>? linkTypeNames, CancellationToken cancellationToken);
}
