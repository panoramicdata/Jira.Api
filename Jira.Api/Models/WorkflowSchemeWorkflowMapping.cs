namespace Jira.Api.Models;

/// <summary>
/// Represents the issue types assigned to a workflow in a workflow scheme.
/// </summary>
public class WorkflowSchemeWorkflowMapping
{
	internal WorkflowSchemeWorkflowMapping(RemoteWorkflowSchemeWorkflowMapping remote)
	{
		Workflow = remote.Workflow ?? throw new ArgumentNullException(nameof(remote.Workflow));
		IssueTypes = remote.IssueTypes ?? [];
		IsDefaultMapping = remote.DefaultMapping;
	}

	/// <summary>The workflow name.</summary>
	public string Workflow { get; }

	/// <summary>The issue type IDs assigned to the workflow.</summary>
	public IReadOnlyCollection<string> IssueTypes { get; }

	/// <summary>Whether the workflow is the scheme default.</summary>
	public bool IsDefaultMapping { get; }
}
