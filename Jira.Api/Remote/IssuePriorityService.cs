using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class IssuePriorityService(JiraClient jira) : IIssuePriorityService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<IssuePriority>> GetPrioritiesAsync(CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (!cache.Priorities.Any())
		{
			var priorities = await _jira.RestClient.ExecuteRequestAsync<RemotePriority[]>(Method.Get, "rest/api/2/priority", null, cancellationToken).ConfigureAwait(false);
			cache.Priorities.TryAdd(priorities.Select(p => new IssuePriority(p)));
		}

		return cache.Priorities.Values;
	}
}
