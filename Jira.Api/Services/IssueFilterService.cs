namespace Jira.Api.Services;

internal class IssueFilterService(JiraClient jira) : IIssueFilterService
{
	private readonly JiraClient _jira = jira;

	public Task<IEnumerable<JiraFilter>> GetFavouritesAsync(CancellationToken cancellationToken)
	{
		return _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraFilter>>(Method.Get, "rest/api/2/filter/favourite", null, cancellationToken);
	}

	public Task<JiraFilter> GetFilterAsync(string filterId, CancellationToken cancellationToken)
	{
		return _jira.RestClient.ExecuteRequestAsync<JiraFilter>(Method.Get, $"rest/api/2/filter/{filterId}", null, cancellationToken);
	}

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteAsync(string filterName, int skip, int? take, CancellationToken cancellationToken)
		=> await _jira.Issues.GetIssuesFromJqlAsync(await GetFilterJqlByNameAsync(filterName, cancellationToken).ConfigureAwait(false), skip, take, cancellationToken).ConfigureAwait(false);

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteWithFieldsAsync(string filterName, int skip, int? take, IList<string> fields, CancellationToken cancellationToken)
		=> await GetIssuesWithFieldsAsync(await GetFilterJqlByNameAsync(filterName, cancellationToken).ConfigureAwait(false), skip, take, fields, cancellationToken).ConfigureAwait(false);

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromFilterAsync(string filterId, int skip, int? take, CancellationToken cancellationToken)
		=> await _jira.Issues.GetIssuesFromJqlAsync(await GetFilterJqlByIdAsync(filterId, cancellationToken).ConfigureAwait(false), skip, take, cancellationToken).ConfigureAwait(false);

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromFilterWithFieldsAsync(string filterId, int skip, int? take, IList<string> fields, CancellationToken cancellationToken)
		=> await GetIssuesWithFieldsAsync(await GetFilterJqlByIdAsync(filterId, cancellationToken).ConfigureAwait(false), skip, take, fields, cancellationToken).ConfigureAwait(false);

	private Task<IPagedQueryResult<Issue>> GetIssuesWithFieldsAsync(string jql, int skip, int? take, IList<string> fields, CancellationToken cancellationToken)
	{
		var searchOptions = new IssueSearchOptions(jql)
		{
			StartAt = skip,
			MaxIssuesPerRequest = take,
			AdditionalFields = fields,
			FetchBasicFields = false
		};
		return _jira.Issues.GetIssuesFromJqlAsync(searchOptions, cancellationToken);
	}
	private async Task<string> GetFilterJqlByNameAsync(string filterName, CancellationToken cancellationToken)
	{
		var filters = await GetFavouritesAsync(cancellationToken).ConfigureAwait(false);
		var filter = filters.FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Filter with name '{filterName}' was not found.");
		return filter.Jql;
	}

	private async Task<string> GetFilterJqlByIdAsync(string filterId, CancellationToken cancellationToken)
	{
		var filter = await GetFilterAsync(filterId, cancellationToken) ?? throw new InvalidOperationException($"Filter with ID '{filterId}' was not found.");
		return filter.Jql;
	}
}
