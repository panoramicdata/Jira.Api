namespace Jira.Api.Services;

internal partial class WorkflowSchemeService
{
	public Task<WorkflowSchemeIssueTypeMapping> GetIssueTypeMappingAsync(string schemeId, string issueTypeId, CancellationToken cancellationToken = default) => GetIssueTypeMappingAsync(schemeId, issueTypeId, false, cancellationToken);
	public Task SetIssueTypeMappingAsync(string schemeId, string issueTypeId, string workflow, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default) => SetIssueTypeMappingAsync(schemeId, issueTypeId, workflow, false, updateDraftIfNeeded, cancellationToken);
	public Task DeleteIssueTypeMappingAsync(string schemeId, string issueTypeId, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default) => DeleteIssueTypeMappingAsync(schemeId, issueTypeId, false, updateDraftIfNeeded, cancellationToken);
	public Task<WorkflowSchemeWorkflowMapping> GetWorkflowMappingAsync(string schemeId, string workflowName, CancellationToken cancellationToken = default) => GetWorkflowMappingAsync(schemeId, workflowName, false, cancellationToken);
	public Task SetWorkflowMappingAsync(string schemeId, string workflow, IEnumerable<string> issueTypeIds, bool isDefaultMapping = false, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default) => SetWorkflowMappingAsync(schemeId, workflow, issueTypeIds, isDefaultMapping, false, updateDraftIfNeeded, cancellationToken);
	public Task DeleteWorkflowMappingAsync(string schemeId, string workflowName, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default) => DeleteWorkflowMappingAsync(schemeId, workflowName, false, updateDraftIfNeeded, cancellationToken);
	public Task<string> GetDefaultWorkflowAsync(string schemeId, CancellationToken cancellationToken = default) => GetDefaultWorkflowAsync(schemeId, false, cancellationToken);
	public Task SetDefaultWorkflowAsync(string schemeId, string workflow, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default) => SetDefaultWorkflowAsync(schemeId, workflow, false, updateDraftIfNeeded, cancellationToken);
	public Task DeleteDefaultWorkflowAsync(string schemeId, bool updateDraftIfNeeded = true, CancellationToken cancellationToken = default) => DeleteDefaultWorkflowAsync(schemeId, false, updateDraftIfNeeded, cancellationToken);

	public async Task<WorkflowScheme> CreateDraftAsync(string schemeId, CancellationToken cancellationToken = default)
	{
		var remote = await ExecuteSchemeAsync(Method.Post, schemeId, "createdraft", null, cancellationToken).ConfigureAwait(false);
		return new WorkflowScheme(remote);
	}
	public async Task<WorkflowScheme> GetDraftAsync(string schemeId, CancellationToken cancellationToken = default)
	{
		var remote = await ExecuteSchemeAsync(Method.Get, schemeId, "draft", null, cancellationToken).ConfigureAwait(false);
		return new WorkflowScheme(remote);
	}
	public async Task<WorkflowScheme> UpdateDraftAsync(string schemeId, string? name = null, string? description = null, string? defaultWorkflow = null, CancellationToken cancellationToken = default)
	{
		var body = NewSchemeBody(name, description, defaultWorkflow);
		var remote = await ExecuteSchemeAsync(Method.Put, schemeId, "draft", body, cancellationToken).ConfigureAwait(false);
		return new WorkflowScheme(remote);
	}
	public Task DeleteDraftAsync(string schemeId, CancellationToken cancellationToken = default) => ExecuteActionAsync(Method.Delete, schemeId, "draft", null, cancellationToken);

	public Task<WorkflowSchemeIssueTypeMapping> GetDraftIssueTypeMappingAsync(string schemeId, string issueTypeId, CancellationToken cancellationToken = default) => GetIssueTypeMappingAsync(schemeId, issueTypeId, true, cancellationToken);
	public Task SetDraftIssueTypeMappingAsync(string schemeId, string issueTypeId, string workflow, CancellationToken cancellationToken = default) => SetIssueTypeMappingAsync(schemeId, issueTypeId, workflow, true, false, cancellationToken);
	public Task DeleteDraftIssueTypeMappingAsync(string schemeId, string issueTypeId, CancellationToken cancellationToken = default) => DeleteIssueTypeMappingAsync(schemeId, issueTypeId, true, false, cancellationToken);
	public Task<WorkflowSchemeWorkflowMapping> GetDraftWorkflowMappingAsync(string schemeId, string workflowName, CancellationToken cancellationToken = default) => GetWorkflowMappingAsync(schemeId, workflowName, true, cancellationToken);
	public Task SetDraftWorkflowMappingAsync(string schemeId, string workflow, IEnumerable<string> issueTypeIds, bool isDefaultMapping = false, CancellationToken cancellationToken = default) => SetWorkflowMappingAsync(schemeId, workflow, issueTypeIds, isDefaultMapping, true, false, cancellationToken);
	public Task DeleteDraftWorkflowMappingAsync(string schemeId, string workflowName, CancellationToken cancellationToken = default) => DeleteWorkflowMappingAsync(schemeId, workflowName, true, false, cancellationToken);
	public Task<string> GetDraftDefaultWorkflowAsync(string schemeId, CancellationToken cancellationToken = default) => GetDefaultWorkflowAsync(schemeId, true, cancellationToken);
	public Task SetDraftDefaultWorkflowAsync(string schemeId, string workflow, CancellationToken cancellationToken = default) => SetDefaultWorkflowAsync(schemeId, workflow, true, false, cancellationToken);
	public Task DeleteDraftDefaultWorkflowAsync(string schemeId, CancellationToken cancellationToken = default) => DeleteDefaultWorkflowAsync(schemeId, true, false, cancellationToken);
}
