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
}
