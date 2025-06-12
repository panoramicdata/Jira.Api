using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Jira.Api.Remote;

internal class ProjectComponentService(Jira jira) : IProjectComponentService
{
	private readonly Jira _jira = jira;

	public async Task<ProjectComponent> CreateComponentAsync(ProjectComponentCreationInfo projectComponent, CancellationToken token = default)
	{
		var serializer = JsonSerializer.Create(_jira.RestClient.Settings.JsonSerializerSettings);
		var resource = "/rest/api/2/component";
		var requestBody = JToken.FromObject(projectComponent, serializer);
		var remoteComponent = await _jira.RestClient.ExecuteRequestAsync<RemoteComponent>(Method.POST, resource, requestBody, token).ConfigureAwait(false);
		remoteComponent.ProjectKey = projectComponent.ProjectKey;
		var component = new ProjectComponent(remoteComponent);

		_jira.Cache.Components.TryAdd(component);

		return component;
	}

	public async Task DeleteComponentAsync(string componentId, string moveIssuesTo = null, CancellationToken token = default)
	{
		var resource = string.Format("/rest/api/2/component/{0}?{1}",
			componentId,
			string.IsNullOrEmpty(moveIssuesTo) ? null : "moveIssuesTo=" + Uri.EscapeDataString(moveIssuesTo));

		await _jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token).ConfigureAwait(false);

		_jira.Cache.Components.TryRemove(componentId);
	}

	public async Task<IEnumerable<ProjectComponent>> GetComponentsAsync(string projectKey, CancellationToken token = default)
	{
		var cache = _jira.Cache;

		if (!cache.Components.Values.Any(c => string.Equals(c.ProjectKey, projectKey)))
		{
			var resource = string.Format("rest/api/2/project/{0}/components", projectKey);
			var remoteComponents = await _jira.RestClient.ExecuteRequestAsync<RemoteComponent[]>(Method.GET, resource).ConfigureAwait(false);
			var components = remoteComponents.Select(remoteComponent =>
			{
				remoteComponent.ProjectKey = projectKey;
				return new ProjectComponent(remoteComponent);
			});
			cache.Components.TryAdd(components);
			return components;
		}
		else
		{
			return cache.Components.Values.Where(c => string.Equals(c.ProjectKey, projectKey));
		}
	}
}
