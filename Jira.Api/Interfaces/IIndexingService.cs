namespace Jira.Api.Interfaces;

/// <summary>
/// Represents operations on the Jira indexing endpoints.
/// </summary>
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
