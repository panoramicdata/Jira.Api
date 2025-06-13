using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class IssueStatusService(Jira jira) : IIssueStatusService
{
	private readonly Jira _jira = jira;

	public async Task<IEnumerable<IssueStatus>> GetStatusesAsync(CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (!cache.Statuses.Any())
		{
			var results = await _jira.RestClient.ExecuteRequestAsync<RemoteStatus[]>(Method.Get, "rest/api/2/status", null, cancellationToken).ConfigureAwait(false);
			cache.Statuses.TryAdd(results.Select(s => new IssueStatus(s)));
		}

		return cache.Statuses.Values;
	}

	public async Task<IssueStatus> GetStatusAsync(string idOrName, CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		var status = cache.Statuses
			.FirstOrDefault(s => idOrName.Equals(s.Value.Id, StringComparison.InvariantCulture) || idOrName.Equals(s.Value.Name, StringComparison.InvariantCulture))
			.Value;

		if (status == null)
		{
			var resource = $"rest/api/2/status/{idOrName}";
			var result = await _jira.RestClient.ExecuteRequestAsync<RemoteStatus>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
			status = new IssueStatus(result);
		}

		return status;
	}
}
