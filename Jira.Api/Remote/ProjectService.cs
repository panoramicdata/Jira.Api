using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class ProjectService(Jira jira) : IProjectService
{
	private readonly Jira _jira = jira;

	public async Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;
		if (!cache.Projects.Any())
		{
			var remoteProjects = await _jira.RestClient.ExecuteRequestAsync<RemoteProject[]>(Method.Get, "rest/api/2/project?expand=lead,url", null, cancellationToken).ConfigureAwait(false);
			cache.Projects.TryAdd(remoteProjects.Select(p => new Project(_jira, p)));
		}

		return cache.Projects.Values;
	}

	public async Task<Project> GetProjectAsync(string projectKey, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/project/{projectKey}?expand=lead,url";
		var remoteProject = await _jira.RestClient.ExecuteRequestAsync<RemoteProject>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		return new Project(_jira, remoteProject);
	}
}
