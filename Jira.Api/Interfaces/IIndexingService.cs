namespace Jira.Api.Interfaces;

/// <summary>
/// Represents operations on the Jira indexing endpoints.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IIndexingService
{
	/// <summary>
	/// Gets a summary of the current Jira index state.
	/// Requires Jira System Administrator permission.
	/// </summary>
	Task<IndexSummary> GetIndexSummaryAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the status of the most recent reindex operation.
	/// Requires Jira System Administrator permission.
	/// </summary>
	Task<ReindexStatus> GetReindexStatusAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Triggers a reindex of the Jira instance.
	/// Requires Jira System Administrator permission.
	/// </summary>
	Task<ReindexStatus> TriggerReindexAsync(CancellationToken cancellationToken = default);
}
