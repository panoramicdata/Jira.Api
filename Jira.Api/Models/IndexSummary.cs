namespace Jira.Api.Models;

/// <summary>
/// Represents the Jira index summary.
/// </summary>
/// <param name="remote">The remote index summary DTO.</param>
public class IndexSummary(RemoteIndexSummary remote)
{
	/// <summary>
	/// Gets the number of issues in the database.
	/// </summary>
	public long IssueCountInDatabase { get; } = remote.issueCountInDatabase;

	/// <summary>
	/// Gets the number of issues in the index.
	/// </summary>
	public long IssueCountInIndex { get; } = remote.issueCountInIndex;

	/// <summary>
	/// Gets the current indexing status.
	/// </summary>
	public string CurrentIndexingStatus { get; } = remote.currentIndexingStatus;

	/// <summary>
	/// Gets the time of the last issue update.
	/// </summary>
	public DateTimeOffset? LastIssueUpdateTime { get; } = remote.lastIssueUpdateTime;

	/// <summary>
	/// Gets a value indicating whether the index is readable.
	/// </summary>
	public bool IndexReadable { get; } = remote.indexReadable;

	/// <summary>
	/// Gets a value indicating whether the index is writeable.
	/// </summary>
	public bool IndexWriteable { get; } = remote.indexWriteable;
}
