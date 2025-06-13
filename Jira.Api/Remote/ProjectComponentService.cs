using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class ProjectComponentService(Jira jira) : IProjectComponentService
{
	private readonly Jira _jira = jira;

	public async Task<ProjectComponent> CreateComponentAsync(ProjectComponentCreationInfo projectComponent, CancellationToken cancellationToken)
	{
		var serializer = JsonSerializer.Create(_jira.RestClient.Settings.JsonSerializerSettings);
		var resource = "/rest/api/2/component";
		var requestBody = JToken.FromObject(projectComponent, serializer);
		var remoteComponent = await _jira.RestClient.ExecuteRequestAsync<RemoteComponent>(Method.Post, resource, requestBody, cancellationToken).ConfigureAwait(false);
		remoteComponent.ProjectKey = projectComponent.ProjectKey;
		var component = new ProjectComponent(remoteComponent);

		_jira.Cache.Components.TryAdd(component);

		return component;
	}

	public async Task DeleteComponentAsync(
		string componentId,
		string? moveIssuesTo,
		CancellationToken cancellationToken)
	{
		var resource = $"/rest/api/2/component/{componentId}?{(string.IsNullOrEmpty(moveIssuesTo) ? null : "moveIssuesTo=" + Uri.EscapeDataString(moveIssuesTo))}";

		await _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken).ConfigureAwait(false);

		_jira.Cache.Components.TryRemove(componentId);
	}

	public async Task<IEnumerable<ProjectComponent>> GetComponentsAsync(string projectKey, CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (!cache.Components.Values.Any(c => string.Equals(c.ProjectKey, projectKey)))
		{
			var resource = $"rest/api/2/project/{projectKey}/components";
			var remoteComponents = await _jira.RestClient.ExecuteRequestAsync<RemoteComponent[]>(Method.Get, resource, null, default).ConfigureAwait(false);
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
