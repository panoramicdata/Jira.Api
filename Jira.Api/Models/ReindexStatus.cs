namespace Jira.Api.Models;

/// <summary>
/// Represents the Jira reindex operation status.
/// </summary>
/// <param name="remote">The remote reindex status DTO.</param>
public class ReindexStatus(RemoteReindexStatus remote)
{
	/// <summary>
	/// Gets the progress percentage of the reindex operation.
	/// </summary>
	public int ProgressPercent { get; } = remote.progressPercent;

	/// <summary>
	/// Gets the name of the current sub-task being processed.
	/// </summary>
	public string CurrentSubTask { get; } = remote.currentSubTask;

	/// <summary>
	/// Gets the time the reindex was submitted.
	/// </summary>
	public DateTimeOffset? SubmittedTime { get; } = remote.submittedTime;

	/// <summary>
	/// Gets the time the reindex started.
	/// </summary>
	public DateTimeOffset? StartTime { get; } = remote.startTime;

	/// <summary>
	/// Gets the time the reindex finished.
	/// </summary>
	public DateTimeOffset? FinishTime { get; } = remote.finishTime;

	/// <summary>
	/// Gets a value indicating whether the reindex completed successfully.
	/// </summary>
	public bool Success { get; } = remote.success;

	/// <summary>
	/// Gets a value indicating whether the reindex failed.
	/// </summary>
	public bool Failed { get; } = remote.failed;
}
