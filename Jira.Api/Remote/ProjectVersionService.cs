using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class ProjectVersionService(JiraClient jira) : IProjectVersionService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<ProjectVersion>> GetVersionsAsync(string projectKey, CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (!cache.Versions.Values.Any(v => string.Equals(v.ProjectKey, projectKey)))
		{
			var resource = $"rest/api/2/project/{projectKey}/versions";
			var remoteVersions = await _jira.RestClient.ExecuteRequestAsync<RemoteVersion[]>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
			var versions = remoteVersions.Select(remoteVersion =>
			{
				remoteVersion.ProjectKey = projectKey;
				return new ProjectVersion(_jira, remoteVersion);
			});
			cache.Versions.TryAdd(versions);
			return versions;
		}
		else
		{
			return cache.Versions.Values.Where(v => string.Equals(v.ProjectKey, projectKey));
		}
	}

	public async Task<IPagedQueryResult<ProjectVersion>> GetPagedVersionsAsync(
		string projectKey,
		int skip,
		int take,
		CancellationToken cancellationToken)
	{
		var settings = _jira.RestClient.Settings.JsonSerializerSettings;
		var resource = $"rest/api/2/project/{projectKey}/version?startAt={skip}&maxResults={take}";

		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var versions = result["values"]
			.Cast<JObject>()
			.Select(versionJson =>
			{
				var remoteVersion = JsonConvert.DeserializeObject<RemoteVersion>(versionJson.ToString(), settings);
				remoteVersion.ProjectKey = projectKey;
				return new ProjectVersion(_jira, remoteVersion);
			});

		return PagedQueryResult<ProjectVersion>.FromJson((JObject)result, versions);
	}

	public async Task<ProjectVersion> CreateVersionAsync(ProjectVersionCreationInfo projectVersion, CancellationToken cancellationToken)
	{
		var settings = _jira.RestClient.Settings.JsonSerializerSettings;
		var serializer = JsonSerializer.Create(settings);
		var resource = "/rest/api/2/version";
		var requestBody = JToken.FromObject(projectVersion, serializer);
		var remoteVersion = await _jira.RestClient.ExecuteRequestAsync<RemoteVersion>(Method.Post, resource, requestBody, cancellationToken).ConfigureAwait(false);
		remoteVersion.ProjectKey = projectVersion.ProjectKey;
		var version = new ProjectVersion(_jira, remoteVersion);

		// invalidate the cache
		_jira.Cache.Versions.Clear();

		return version;
	}

	public async Task DeleteVersionAsync(
		string versionId,
		string? moveFixIssuesTo,
		string? moveAffectedIssuesTo,
		CancellationToken cancellationToken)
	{
		var resource = $"/rest/api/2/version/{versionId}?{(string.IsNullOrEmpty(moveFixIssuesTo) ? null : "moveFixIssuesTo=" + Uri.EscapeDataString(moveFixIssuesTo))}&{(string.IsNullOrEmpty(moveAffectedIssuesTo) ? null : "moveAffectedIssuesTo=" + Uri.EscapeDataString(moveAffectedIssuesTo))}";

		await _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken).ConfigureAwait(false);

		_jira.Cache.Versions.TryRemove(versionId);
	}

	public async Task<ProjectVersion> GetVersionAsync(string versionId, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/version/{versionId}";
		var remoteVersion = await _jira.RestClient.ExecuteRequestAsync<RemoteVersion>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);

		return new ProjectVersion(_jira, remoteVersion);
	}

	public async Task<ProjectVersion> UpdateVersionAsync(ProjectVersion version, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/version/{version.Id}";
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var versionJson = JsonConvert.SerializeObject(version.RemoteVersion, serializerSettings);
		var remoteVersion = await _jira.RestClient.ExecuteRequestAsync<RemoteVersion>(Method.Put, resource, versionJson, cancellationToken).ConfigureAwait(false);

		// invalidate the cache
		_jira.Cache.Versions.Clear();

		return new ProjectVersion(_jira, remoteVersion);
	}
}
