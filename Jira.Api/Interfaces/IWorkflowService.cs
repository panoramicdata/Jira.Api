namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on workflows in Jira.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IWorkflowService
{
	/// <summary>
	/// Returns all workflows in Jira.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<Workflow>> GetWorkflowsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns a workflow by name.
	/// </summary>
	/// <param name="workflowName">The name of the workflow.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<Workflow> GetWorkflowAsync(string workflowName, CancellationToken cancellationToken = default);
}
