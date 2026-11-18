namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue statuses of jira.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IIssueStatusService
{
	/// <summary>
	/// Returns all the issue statuses within JIRA.
	/// </summary>
	Task<IEnumerable<IssueStatus>> GetStatusesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns a full representation of the status having the given id or name.
	/// </summary>
	Task<IssueStatus> GetStatusAsync(string idOrName, CancellationToken cancellationToken = default);
}
