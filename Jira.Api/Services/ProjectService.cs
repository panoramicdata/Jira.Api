namespace Jira.Api.Services;

internal class ProjectService(JiraClient jira) : IProjectService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken cancellationToken = default)
	{
		var remoteProjects = await _jira.RestClient.ExecuteRequestAsync<RemoteProject[]>(Method.Get, "rest/api/2/project?expand=lead,url", null, cancellationToken).ConfigureAwait(false);
		return remoteProjects.Select(p => new Project(_jira, p));
	}

	public async Task<Project> GetProjectAsync(string projectKey, CancellationToken cancellationToken = default)
	{
		var resource = $"rest/api/2/project/{projectKey}?expand=lead,url";
		var remoteProject = await _jira.RestClient.ExecuteRequestAsync<RemoteProject>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		return new Project(_jira, remoteProject);
	}

	public async Task<IEnumerable<IssueTypeWithStatuses>> GetProjectStatusesAsync(string projectKeyOrId, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(projectKeyOrId);

		var resource = $"rest/api/2/project/{Uri.EscapeDataString(projectKeyOrId)}/statuses";
		var results = await _jira.RestClient.ExecuteRequestAsync<RemoteProjectStatusesByIssueType[]>(
			Method.Get,
			resource,
			null,
			cancellationToken).ConfigureAwait(false);

		return results.Select(r => new IssueTypeWithStatuses(r));
	}

	public async Task<ProjectWorkflowScheme?> GetProjectWorkflowSchemeAsync(string projectKeyOrId, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(projectKeyOrId);

		var resource = $"rest/api/2/project/{Uri.EscapeDataString(projectKeyOrId)}/workflowscheme";
		var result = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflowScheme>(
			Method.Get,
			resource,
			null,
			cancellationToken).ConfigureAwait(false);

		return result != null ? new ProjectWorkflowScheme(result) : null;
	}
}
