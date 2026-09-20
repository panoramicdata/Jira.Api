namespace Jira.Api.Services;

internal class WorkflowSchemeService(JiraClient jira) : IWorkflowSchemeService
{
	private readonly JiraClient _jira = jira;

	public async Task<IPagedQueryResult<WorkflowScheme>> GetWorkflowSchemesAsync(int startAt = 0, int maxResults = 50, CancellationToken cancellationToken = default)
	{
		var resource = $"rest/api/2/workflowscheme?startAt={startAt}&maxResults={maxResults}";
		var response = await _jira.RestClient.ExecuteRequestAsync<RemotePagedResult<RemoteWorkflowScheme>>(
			Method.Get,
			resource,
			null,
			cancellationToken).ConfigureAwait(false);

		var items = response.Values?.Select(s => new WorkflowScheme(s)).ToList() ?? [];
		return new PagedQueryResult<WorkflowScheme>(items, response.StartAt, response.MaxResults, response.Total);
	}

	public async Task<WorkflowScheme> GetWorkflowSchemeAsync(string schemeId, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(schemeId);

		var resource = $"rest/api/2/workflowscheme/{schemeId}";
		var remoteScheme = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowScheme>(
			Method.Get,
			resource,
			null,
			cancellationToken).ConfigureAwait(false);

		return new WorkflowScheme(remoteScheme);
	}

	public async Task<WorkflowScheme> GetWorkflowSchemeForProjectAsync(string projectKey, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(projectKey);

		var resource = $"rest/api/2/project/{projectKey}/workflowscheme";
		var remoteScheme = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowScheme>(
			Method.Get,
			resource,
			null,
			cancellationToken).ConfigureAwait(false);

		return new WorkflowScheme(remoteScheme);
	}

	public async Task<WorkflowScheme> CreateWorkflowSchemeAsync(string name, string? description = null, string? defaultWorkflow = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(name);

		var requestBody = new Dictionary<string, object?>
		{
			["name"] = name,
			["description"] = description,
			["defaultWorkflow"] = defaultWorkflow
		};

		var remoteScheme = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowScheme>(
			Method.Post,
			"rest/api/2/workflowscheme",
			requestBody,
			cancellationToken).ConfigureAwait(false);

		return new WorkflowScheme(remoteScheme);
	}

	public async Task<WorkflowScheme> UpdateWorkflowSchemeAsync(string schemeId, string? name = null, string? description = null, string? defaultWorkflow = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(schemeId);

		var requestBody = new Dictionary<string, object?>();

		if (name != null)
		{
			requestBody["name"] = name;
		}

		if (description != null)
		{
			requestBody["description"] = description;
		}

		if (defaultWorkflow != null)
		{
			requestBody["defaultWorkflow"] = defaultWorkflow;
		}

		var resource = $"rest/api/2/workflowscheme/{schemeId}";
		var remoteScheme = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowScheme>(
			Method.Put,
			resource,
			requestBody,
			cancellationToken).ConfigureAwait(false);

		return new WorkflowScheme(remoteScheme);
	}

	public async Task DeleteWorkflowSchemeAsync(string schemeId, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(schemeId);

		var resource = $"rest/api/2/workflowscheme/{schemeId}";
		await _jira.RestClient.ExecuteRequestAsync(
			Method.Delete,
			resource,
			null,
			cancellationToken).ConfigureAwait(false);
	}

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
		var body = new Dictionary<string, object?>(); if (name != null) body["name"] = name; if (description != null) body["description"] = description; if (defaultWorkflow != null) body["defaultWorkflow"] = defaultWorkflow; return body;
	}
	private static void Validate(params string[] values) { foreach (var value in values) ArgumentException.ThrowIfNullOrEmpty(value); }
}
