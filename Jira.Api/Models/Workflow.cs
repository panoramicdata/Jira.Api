namespace Jira.Api.Models;

/// <summary>
/// Represents a workflow in Jira.
/// </summary>
public class Workflow
{
	/// <summary>
	/// Creates an instance of Workflow from a remote entity.
	/// </summary>
	internal Workflow(RemoteWorkflow remoteWorkflow)
	{
		Name = remoteWorkflow.Name ?? throw new ArgumentNullException(nameof(remoteWorkflow.Name));
		Description = remoteWorkflow.Description;
		IsDefault = remoteWorkflow.IsDefault;
		LastModifiedDate = remoteWorkflow.LastModifiedDate;
		LastModifiedUser = remoteWorkflow.LastModifiedUser;
		LastModifiedUserAccountId = remoteWorkflow.LastModifiedUserAccountId;
		Steps = remoteWorkflow.Steps;
	}

	/// <summary>
	/// The name of the workflow.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The description of the workflow.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Whether this is the default workflow.
	/// </summary>
	public bool IsDefault { get; }

	/// <summary>
	/// The last modified date of the workflow.
	/// </summary>
	public string? LastModifiedDate { get; }

	/// <summary>
	/// The last user who modified the workflow.
	/// </summary>
	public string? LastModifiedUser { get; }

	/// <summary>
	/// The account ID of the last user who modified the workflow.
	/// </summary>
	public string? LastModifiedUserAccountId { get; }

	/// <summary>
	/// The number of steps in the workflow.
	/// </summary>
	public int Steps { get; }

	/// <summary>
	/// Returns the string representation of this workflow.
	/// </summary>
	public override string ToString() => Name;
}
