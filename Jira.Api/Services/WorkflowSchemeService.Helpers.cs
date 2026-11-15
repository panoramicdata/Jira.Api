namespace Jira.Api.Services;

internal partial class WorkflowSchemeService
{
	private async Task<WorkflowSchemeIssueTypeMapping> GetIssueTypeMappingAsync(string schemeId, string issueTypeId, bool draft, CancellationToken cancellationToken)
	{
		Validate(schemeId, issueTypeId);
		var remote = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowSchemeIssueTypeMapping>(Method.Get, Resource(schemeId, draft, $"issuetype/{Uri.EscapeDataString(issueTypeId)}"), null, cancellationToken).ConfigureAwait(false);
		return new WorkflowSchemeIssueTypeMapping(remote);
	}
	private Task SetIssueTypeMappingAsync(string schemeId, string issueTypeId, string workflow, bool draft, bool updateDraftIfNeeded, CancellationToken cancellationToken)
	{
		Validate(schemeId, issueTypeId, workflow);
		return ExecuteActionAsync(Method.Put, schemeId, $"{(draft ? "draft/" : string.Empty)}issuetype/{Uri.EscapeDataString(issueTypeId)}", new { issueType = issueTypeId, workflow, updateDraftIfNeeded }, cancellationToken);
	}
	private Task DeleteIssueTypeMappingAsync(string schemeId, string issueTypeId, bool draft, bool updateDraftIfNeeded, CancellationToken cancellationToken)
	{
		Validate(schemeId, issueTypeId);
		return ExecuteActionAsync(Method.Delete, schemeId, $"{(draft ? "draft/" : string.Empty)}issuetype/{Uri.EscapeDataString(issueTypeId)}", new { updateDraftIfNeeded }, cancellationToken);
	}
	private async Task<WorkflowSchemeWorkflowMapping> GetWorkflowMappingAsync(string schemeId, string workflowName, bool draft, CancellationToken cancellationToken)
	{
		Validate(schemeId, workflowName);
		var remote = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowSchemeWorkflowMapping>(Method.Get, $"{Resource(schemeId, draft, "workflow")}?workflowName={Uri.EscapeDataString(workflowName)}", null, cancellationToken).ConfigureAwait(false);
		return new WorkflowSchemeWorkflowMapping(remote);
	}
	private Task SetWorkflowMappingAsync(string schemeId, string workflow, IEnumerable<string> issueTypeIds, bool isDefaultMapping, bool draft, bool updateDraftIfNeeded, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(issueTypeIds); Validate(schemeId, workflow);
		return ExecuteActionAsync(Method.Put, schemeId, $"{(draft ? "draft/" : string.Empty)}workflow", new { workflow, issueTypes = issueTypeIds, defaultMapping = isDefaultMapping, updateDraftIfNeeded }, cancellationToken);
	}
	private Task DeleteWorkflowMappingAsync(string schemeId, string workflowName, bool draft, bool updateDraftIfNeeded, CancellationToken cancellationToken)
	{
		Validate(schemeId, workflowName);
		return ExecuteActionAsync(Method.Delete, schemeId, $"{(draft ? "draft/" : string.Empty)}workflow?workflowName={Uri.EscapeDataString(workflowName)}", new { updateDraftIfNeeded }, cancellationToken);
	}
	private async Task<string> GetDefaultWorkflowAsync(string schemeId, bool draft, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(schemeId);
		var mapping = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowSchemeIssueTypeMapping>(Method.Get, Resource(schemeId, draft, "default"), null, cancellationToken).ConfigureAwait(false);
		return mapping.Workflow ?? throw new InvalidOperationException("Jira returned a default workflow without a workflow name.");
	}
	private Task SetDefaultWorkflowAsync(string schemeId, string workflow, bool draft, bool updateDraftIfNeeded, CancellationToken cancellationToken)
	{
		Validate(schemeId, workflow);
		return ExecuteActionAsync(Method.Put, schemeId, $"{(draft ? "draft/" : string.Empty)}default", new { workflow, updateDraftIfNeeded }, cancellationToken);
	}
	private Task DeleteDefaultWorkflowAsync(string schemeId, bool draft, bool updateDraftIfNeeded, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(schemeId);
		return ExecuteActionAsync(Method.Delete, schemeId, $"{(draft ? "draft/" : string.Empty)}default", new { updateDraftIfNeeded }, cancellationToken);
	}
	private async Task<RemoteWorkflowScheme> ExecuteSchemeAsync(Method method, string schemeId, string suffix, object? body, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(schemeId);
		return await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowScheme>(method, Resource(schemeId, false, suffix), body, cancellationToken).ConfigureAwait(false);
	}
	private async Task ExecuteActionAsync(Method method, string schemeId, string suffix, object? body, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(schemeId);
		await _jira.RestClient.ExecuteRequestAsync(method, Resource(schemeId, false, suffix), body, cancellationToken).ConfigureAwait(false);
	}
	private static string Resource(string schemeId, bool draft, string suffix) => $"rest/api/2/workflowscheme/{Uri.EscapeDataString(schemeId)}/{(draft ? "draft/" : string.Empty)}{suffix}";
	private static Dictionary<string, object?> NewSchemeBody(string? name, string? description, string? defaultWorkflow)
	{
		var body = new Dictionary<string, object?>();
		if (name != null)
		{
			body["name"] = name;
		}

		if (description != null)
		{
			body["description"] = description;
		}

		if (defaultWorkflow != null)
		{
			body["defaultWorkflow"] = defaultWorkflow;
		}

		return body;
	}
	private static void Validate(params string[] values)
	{
		foreach (var value in values)
		{
			ArgumentException.ThrowIfNullOrEmpty(value);
		}
	}
}
