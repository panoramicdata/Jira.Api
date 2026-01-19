namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on workflow schemes in Jira.
/// </summary>
public interface IWorkflowSchemeService
{
	/// <summary>
	/// Returns all workflow schemes in Jira.
	/// </summary>
	/// <param name="startAt">The index of the first item to return.</param>
	/// <param name="maxResults">The maximum number of items to return.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IPagedQueryResult<WorkflowScheme>> GetWorkflowSchemesAsync(int startAt = 0, int maxResults = 50, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns a workflow scheme by ID.
	/// </summary>
	/// <param name="schemeId">The ID of the workflow scheme.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<WorkflowScheme> GetWorkflowSchemeAsync(string schemeId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the workflow scheme associated with a project.
	/// </summary>
	/// <param name="projectKey">The project key or ID.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<WorkflowScheme> GetWorkflowSchemeForProjectAsync(string projectKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new workflow scheme.
	/// </summary>
	/// <param name="name">The name of the workflow scheme.</param>
	/// <param name="description">The description of the workflow scheme.</param>
	/// <param name="defaultWorkflow">The name of the default workflow.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<WorkflowScheme> CreateWorkflowSchemeAsync(string name, string? description = null, string? defaultWorkflow = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing workflow scheme.
	/// </summary>
	/// <param name="schemeId">The ID of the workflow scheme to update.</param>
	/// <param name="name">The new name of the workflow scheme.</param>
	/// <param name="description">The new description of the workflow scheme.</param>
	/// <param name="defaultWorkflow">The new default workflow name.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<WorkflowScheme> UpdateWorkflowSchemeAsync(string schemeId, string? name = null, string? description = null, string? defaultWorkflow = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a workflow scheme.
	/// </summary>
	/// <param name="schemeId">The ID of the workflow scheme to delete.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteWorkflowSchemeAsync(string schemeId, CancellationToken cancellationToken = default);
}
