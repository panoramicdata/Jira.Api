namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on workflows in Jira.
/// </summary>
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
