using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace Jira.Api.Remote;

internal class IssueResolutionService(Jira jira) : IIssueResolutionService
{
	private readonly Jira _jira = jira;

	public async Task<IEnumerable<IssueResolution>> GetResolutionsAsync(CancellationToken token)
	{
		var cache = _jira.Cache;

		if (!cache.Resolutions.Any())
		{
			var resolutions = await _jira.RestClient.ExecuteRequestAsync<RemoteResolution[]>(Method.GET, "rest/api/2/resolution", null, token).ConfigureAwait(false);
			cache.Resolutions.TryAdd(resolutions.Select(r => new IssueResolution(r)));
		}

		return cache.Resolutions.Values;
	}
}
