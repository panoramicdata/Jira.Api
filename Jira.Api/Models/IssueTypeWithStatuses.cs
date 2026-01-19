namespace Jira.Api.Models;

/// <summary>
/// Represents an issue type with its associated statuses.
/// Returned by GET /rest/api/2/project/{projectKeyOrId}/statuses
/// </summary>
public class IssueTypeWithStatuses
{
	/// <summary>
	/// Creates an instance from the remote entity.
	/// </summary>
	internal IssueTypeWithStatuses(RemoteProjectStatusesByIssueType remote)
	{
		Id = remote.Id;
		Name = remote.Name;
		IsSubtask = remote.Subtask;
		Statuses = remote.Statuses?.Select(s => new IssueStatus(s)).ToList() ?? [];
	}

	/// <summary>
	/// The issue type ID.
	/// </summary>
	public string? Id { get; }

	/// <summary>
	/// The issue type name (e.g., "Bug", "Task", "Story").
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// Whether this is a subtask type.
	/// </summary>
	public bool IsSubtask { get; }

	/// <summary>
	/// The statuses available for this issue type in this project.
	/// </summary>
	public IReadOnlyList<IssueStatus> Statuses { get; }

	/// <summary>
	/// Returns a string representation of this object.
	/// </summary>
	public override string ToString() => $"{Name} ({Statuses.Count} statuses)";
}
