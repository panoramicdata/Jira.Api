namespace Jira.Api.Services;

internal class IssueResolutionService(JiraClient jira) : IIssueResolutionService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<IssueResolution>> GetResolutionsAsync(CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (!cache.Resolutions.Any())
		{
			var resolutions = await _jira.RestClient.ExecuteRequestAsync<RemoteResolution[]>(Method.Get, "rest/api/2/resolution", null, cancellationToken).ConfigureAwait(false);
			cache.Resolutions.TryAdd(resolutions.Select(r => new IssueResolution(r)));
		}

		return cache.Resolutions.Values;
	}
}
