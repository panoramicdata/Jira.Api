using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class IssueFilterService(Jira jira) : IIssueFilterService
{
	private readonly Jira _jira = jira;

	public Task<IEnumerable<JiraFilter>> GetFavouritesAsync(CancellationToken cancellationToken)
	{
		return _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraFilter>>(Method.Get, "rest/api/2/filter/favourite", null, cancellationToken);
	}

	public Task<JiraFilter> GetFilterAsync(string filterId, CancellationToken cancellationToken)
	{
		return _jira.RestClient.ExecuteRequestAsync<JiraFilter>(Method.Get, $"rest/api/2/filter/{filterId}", null, cancellationToken);
	}

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteAsync(string filterName, int skip, int? maxIssues, CancellationToken cancellationToken)
	{
		var jql = await GetFilterJqlByNameAsync(filterName, cancellationToken).ConfigureAwait(false);

		return await _jira.Issues.GetIssuesFromJqlAsync(jql, skip, maxIssues, cancellationToken).ConfigureAwait(false);
	}

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteWithFieldsAsync(
		string filterName,
		int skip,
		int? take,
		IList<string> fields,
		CancellationToken cancellationToken)
	{
		var jql = await GetFilterJqlByNameAsync(filterName, cancellationToken).ConfigureAwait(false);

		var searchOptions = new IssueSearchOptions(jql)
		{
			StartAt = skip,
			MaxIssuesPerRequest = take,
			AdditionalFields = fields,
			FetchBasicFields = false
		};
		return await _jira.Issues.GetIssuesFromJqlAsync(searchOptions, cancellationToken).ConfigureAwait(false);
	}

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromFilterAsync(
		string filterId,
		int skip,
		int? take,
		CancellationToken cancellationToken)
	{
		var jql = await GetFilterJqlByIdAsync(filterId, cancellationToken).ConfigureAwait(false);

		return await _jira.Issues.GetIssuesFromJqlAsync(jql, skip, take, cancellationToken).ConfigureAwait(false);
	}

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromFilterWithFieldsAsync(
		string filterId,
		int skip,
		int? take,
		IList<string> fields,
		CancellationToken cancellationToken)
	{
		var jql = await GetFilterJqlByIdAsync(filterId, cancellationToken).ConfigureAwait(false);

		var searchOptions = new IssueSearchOptions(jql)
		{
			StartAt = skip,
			MaxIssuesPerRequest = take,
			AdditionalFields = fields,
			FetchBasicFields = false
		};
		return await _jira.Issues.GetIssuesFromJqlAsync(searchOptions, cancellationToken).ConfigureAwait(false);
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
