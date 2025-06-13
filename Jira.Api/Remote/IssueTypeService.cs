using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class IssueTypeService(JiraClient jira) : IIssueTypeService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (!cache.IssueTypes.Any())
		{
			var remoteIssueTypes = await _jira.RestClient.ExecuteRequestAsync<RemoteIssueType[]>(Method.Get, "rest/api/2/issuetype", null, cancellationToken).ConfigureAwait(false);
			var issueTypes = remoteIssueTypes.Select(t => new IssueType(t));
			cache.IssueTypes.TryAdd(issueTypes);
		}

		return cache.IssueTypes.Values;
	}

	public async Task<IEnumerable<IssueType>> GetIssueTypesForProjectAsync(string projectKey, CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (!cache.ProjectIssueTypes.TryGetValue(projectKey, out JiraEntityDictionary<IssueType> _))
		{
			var resource = $"rest/api/2/project/{projectKey}/statuses";
			var results = await _jira.RestClient.ExecuteRequestAsync<RemoteIssueType[]>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
			var issueTypes = results.Select(x => new IssueType(x));

			cache.ProjectIssueTypes.TryAdd(projectKey, new JiraEntityDictionary<IssueType>(issueTypes));
		}

		return cache.ProjectIssueTypes[projectKey].Values;
	}
}
