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

	/// <summary>Gets the workflow mapped to an issue type.</summary>
	Task<WorkflowSchemeIssueTypeMapping> GetIssueTypeMappingAsync(string schemeId, string issueTypeId, CancellationToken cancellationToken = default);
	/// <summary>Sets the workflow mapped to an issue type. Active schemes are updated through a draft by default.</summary>
	Task SetIssueTypeMappingAsync(string schemeId, string issueTypeId, string workflow, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default);
	/// <summary>Deletes an issue type mapping. Active schemes are updated through a draft by default.</summary>
	Task DeleteIssueTypeMappingAsync(string schemeId, string issueTypeId, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default);

	/// <summary>Gets the issue types mapped to a workflow.</summary>
	Task<WorkflowSchemeWorkflowMapping> GetWorkflowMappingAsync(string schemeId, string workflowName, CancellationToken cancellationToken = default);
	/// <summary>Sets the issue types mapped to a workflow. Active schemes are updated through a draft by default.</summary>
	Task SetWorkflowMappingAsync(string schemeId, string workflow, IEnumerable<string> issueTypeIds, bool isDefaultMapping = false, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default);
	/// <summary>Deletes a workflow mapping. Active schemes are updated through a draft by default.</summary>
	Task DeleteWorkflowMappingAsync(string schemeId, string workflowName, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default);

	/// <summary>Gets the scheme default workflow.</summary>
	Task<string> GetDefaultWorkflowAsync(string schemeId, CancellationToken cancellationToken = default);
	/// <summary>Sets the scheme default workflow. Active schemes are updated through a draft by default.</summary>
	Task SetDefaultWorkflowAsync(string schemeId, string workflow, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default);
	/// <summary>Deletes the scheme default workflow. Active schemes are updated through a draft by default.</summary>
	Task DeleteDefaultWorkflowAsync(string schemeId, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default);

	/// <summary>Creates a workflow scheme draft.</summary>
	Task<WorkflowScheme> CreateDraftAsync(string schemeId, CancellationToken cancellationToken = default);
	/// <summary>Gets a workflow scheme draft.</summary>
	Task<WorkflowScheme> GetDraftAsync(string schemeId, CancellationToken cancellationToken = default);
	/// <summary>Updates a workflow scheme draft.</summary>
	Task<WorkflowScheme> UpdateDraftAsync(string schemeId, string? name = null, string? description = null, string? defaultWorkflow = null, CancellationToken cancellationToken = default);
	/// <summary>Deletes a workflow scheme draft.</summary>
	Task DeleteDraftAsync(string schemeId, CancellationToken cancellationToken = default);

	/// <summary>Gets a draft issue type mapping.</summary>
	Task<WorkflowSchemeIssueTypeMapping> GetDraftIssueTypeMappingAsync(string schemeId, string issueTypeId, CancellationToken cancellationToken = default);
	/// <summary>Sets a draft issue type mapping.</summary>
	Task SetDraftIssueTypeMappingAsync(string schemeId, string issueTypeId, string workflow, CancellationToken cancellationToken = default);
	/// <summary>Deletes a draft issue type mapping.</summary>
	Task DeleteDraftIssueTypeMappingAsync(string schemeId, string issueTypeId, CancellationToken cancellationToken = default);
	/// <summary>Gets a draft workflow mapping.</summary>
	Task<WorkflowSchemeWorkflowMapping> GetDraftWorkflowMappingAsync(string schemeId, string workflowName, CancellationToken cancellationToken = default);
	/// <summary>Sets a draft workflow mapping.</summary>
	Task SetDraftWorkflowMappingAsync(string schemeId, string workflow, IEnumerable<string> issueTypeIds, bool isDefaultMapping = false, CancellationToken cancellationToken = default);
	/// <summary>Deletes a draft workflow mapping.</summary>
	Task DeleteDraftWorkflowMappingAsync(string schemeId, string workflowName, CancellationToken cancellationToken = default);
	/// <summary>Gets the draft default workflow.</summary>
	Task<string> GetDraftDefaultWorkflowAsync(string schemeId, CancellationToken cancellationToken = default);
	/// <summary>Sets the draft default workflow.</summary>
	Task SetDraftDefaultWorkflowAsync(string schemeId, string workflow, CancellationToken cancellationToken = default);
	/// <summary>Deletes the draft default workflow.</summary>
	Task DeleteDraftDefaultWorkflowAsync(string schemeId, CancellationToken cancellationToken = default);
}
