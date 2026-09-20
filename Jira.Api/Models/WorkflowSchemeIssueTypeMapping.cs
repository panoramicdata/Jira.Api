namespace Jira.Api.Models;

/// <summary>
/// Represents an issue type to workflow mapping in a workflow scheme.
/// </summary>
public class WorkflowSchemeIssueTypeMapping
{
	internal WorkflowSchemeIssueTypeMapping(RemoteWorkflowSchemeIssueTypeMapping remote)
	{
		IssueType = remote.IssueType ?? throw new ArgumentNullException(nameof(remote.IssueType));
		Workflow = remote.Workflow ?? throw new ArgumentNullException(nameof(remote.Workflow));
	}

	/// <summary>The issue type ID.</summary>
	public string IssueType { get; }

	/// <summary>The workflow name.</summary>
	public string Workflow { get; }
}
