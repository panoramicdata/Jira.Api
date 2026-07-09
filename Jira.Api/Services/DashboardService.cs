namespace Jira.Api.Services;

internal class DashboardService(JiraClient jira) : IDashboardService
{
	private readonly JiraClient _jira = jira;

	public Task<JiraDashboardPage> GetDashboardsAsync(int skip, int? take, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/dashboard?startAt={skip}";
		if (take is not null)
		{
			resource += $"&maxResults={take}";
		}

		return _jira.RestClient.ExecuteRequestAsync<JiraDashboardPage>(Method.Get, resource, null, cancellationToken);
	}

	public Task<JiraDashboard> GetDashboardAsync(string dashboardId, CancellationToken cancellationToken)
	{
		return _jira.RestClient.ExecuteRequestAsync<JiraDashboard>(Method.Get, $"rest/api/2/dashboard/{dashboardId}", null, cancellationToken);
	}

	public Task<JiraDashboardDetail> GetDashboardDetailAsync(string dashboardId, CancellationToken cancellationToken)
	{
		return _jira.RestClient.ExecuteRequestAsync<JiraDashboardDetail>(Method.Get, $"rest/dashboards/1.0/{dashboardId}", null, cancellationToken);
	}
}
