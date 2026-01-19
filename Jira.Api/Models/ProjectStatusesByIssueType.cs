namespace Jira.Api.Models;

/// <summary>
/// Represents the statuses for a specific issue type in a project.
/// </summary>
public class ProjectStatusesByIssueType
{
	/// <summary>
	/// Creates an instance of ProjectStatusesByIssueType from a remote entity.
	/// </summary>
	internal ProjectStatusesByIssueType(RemoteProjectStatusesByIssueType remoteProjectStatuses)
	{
		Self = remoteProjectStatuses.Self;
		Id = remoteProjectStatuses.Id;
		Name = remoteProjectStatuses.Name;
		IsSubtask = remoteProjectStatuses.Subtask;
		Statuses = remoteProjectStatuses.Statuses?.Select(s => new IssueStatus(s)).ToList() ?? [];
	}

	/// <summary>
	/// The URL of the issue type.
	/// </summary>
	public string? Self { get; }

	/// <summary>
	/// The ID of the issue type.
	/// </summary>
	public string? Id { get; }

	/// <summary>
	/// The name of the issue type.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// Whether this issue type is a subtask.
	/// </summary>
	public bool IsSubtask { get; }

	/// <summary>
	/// The list of statuses available for this issue type.
	/// </summary>
	public IReadOnlyList<IssueStatus> Statuses { get; }

	/// <summary>
	/// Returns the string representation of this entity.
	/// </summary>
	public override string ToString() => Name ?? Id ?? string.Empty;
}
