namespace Jira.Api.Models;

/// <summary>
/// Represents a project's workflow scheme.
/// Returned by GET /rest/api/2/project/{projectKeyOrId}/workflowscheme
/// </summary>
public class ProjectWorkflowScheme
{
	/// <summary>
	/// Creates an instance from the remote entity.
	/// </summary>
	internal ProjectWorkflowScheme(RemoteWorkflowScheme remote)
	{
		Id = remote.Id;
		Name = remote.Name;
		Description = remote.Description;
		DefaultWorkflow = remote.DefaultWorkflow;
		IssueTypeMappings = remote.IssueTypeMappings;
		IsDraft = remote.IsDraft;
	}

	/// <summary>
	/// The workflow scheme ID.
	/// </summary>
	public string? Id { get; }

	/// <summary>
	/// The workflow scheme name.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// The workflow scheme description.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// The default workflow name used for issue types not explicitly mapped.
	/// </summary>
	public string? DefaultWorkflow { get; }

	/// <summary>
	/// Mappings of issue type ID to workflow name.
	/// Key = issue type ID, Value = workflow name.
	/// </summary>
	public IDictionary<string, string>? IssueTypeMappings { get; }

	/// <summary>
	/// Whether this is a draft workflow scheme.
	/// </summary>
	public bool IsDraft { get; }

	/// <summary>
	/// Returns a string representation of this object.
	/// </summary>
	public override string ToString() => Name ?? Id ?? "Unknown";
}
