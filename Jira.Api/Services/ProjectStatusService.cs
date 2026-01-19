namespace Jira.Api.Services;

internal class ProjectStatusService(JiraClient jira) : IProjectStatusService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<ProjectStatusesByIssueType>> GetProjectStatusesAsync(string projectKey, CancellationToken cancellationToken = default)
	{
		var resource = $"rest/api/2/project/{projectKey}/statuses";
		var remoteStatuses = await _jira.RestClient.ExecuteRequestAsync<RemoteProjectStatusesByIssueType[]>(
			Method.Get,
			resource,
			null,
			cancellationToken).ConfigureAwait(false);

		return remoteStatuses.Select(s => new ProjectStatusesByIssueType(s));
	}
}
