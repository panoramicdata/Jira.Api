namespace Jira.Api.Services;

internal class IndexingService(JiraClient jira) : IIndexingService
{
	private readonly JiraClient _jira = jira;

	public async Task<IndexSummary> GetIndexSummaryAsync(CancellationToken cancellationToken)
	{
		var remote = await _jira.RestClient
			.ExecuteRequestAsync<RemoteIndexSummary>(Method.Get, "rest/api/2/index/summary", null, cancellationToken)
			.ConfigureAwait(false);
		return new IndexSummary(remote);
	}

	public async Task<ReindexStatus> GetReindexStatusAsync(CancellationToken cancellationToken)
	{
		var remote = await _jira.RestClient
			.ExecuteRequestAsync<RemoteReindexStatus>(Method.Get, "rest/api/2/reindex", null, cancellationToken)
			.ConfigureAwait(false);
		return new ReindexStatus(remote);
	}

	public async Task<ReindexStatus> TriggerReindexAsync(CancellationToken cancellationToken)
	{
		var remote = await _jira.RestClient
			.ExecuteRequestAsync<RemoteReindexStatus>(Method.Post, "rest/api/2/reindex", null, cancellationToken)
			.ConfigureAwait(false);
		return new ReindexStatus(remote);
	}
}
