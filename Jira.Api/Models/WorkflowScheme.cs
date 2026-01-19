namespace Jira.Api.Models;

/// <summary>
/// Represents a workflow scheme in Jira.
/// </summary>
public class WorkflowScheme
{
	/// <summary>
	/// Creates an instance of WorkflowScheme from a remote entity.
	/// </summary>
	internal WorkflowScheme(RemoteWorkflowScheme remoteWorkflowScheme)
	{
		Id = remoteWorkflowScheme.Id ?? throw new ArgumentNullException(nameof(remoteWorkflowScheme.Id));
		Name = remoteWorkflowScheme.Name;
		Description = remoteWorkflowScheme.Description;
		DefaultWorkflow = remoteWorkflowScheme.DefaultWorkflow;
		IssueTypeMappings = remoteWorkflowScheme.IssueTypeMappings ?? new Dictionary<string, string>();
		IsDraft = remoteWorkflowScheme.IsDraft;
		Self = remoteWorkflowScheme.Self;
	}

	/// <summary>
	/// The ID of the workflow scheme.
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// The name of the workflow scheme.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// The description of the workflow scheme.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// The name of the default workflow for the workflow scheme.
	/// </summary>
	public string? DefaultWorkflow { get; }

	/// <summary>
	/// The issue type to workflow mappings.
	/// Key: Issue type ID, Value: Workflow name.
	/// </summary>
	public IDictionary<string, string> IssueTypeMappings { get; }

	/// <summary>
	/// Whether the workflow scheme is a draft.
	/// </summary>
	public bool IsDraft { get; }

	/// <summary>
	/// The URL of the workflow scheme.
	/// </summary>
	public string? Self { get; }

	/// <summary>
	/// Returns the string representation of this workflow scheme.
	/// </summary>
	public override string ToString() => Name ?? Id;
}
