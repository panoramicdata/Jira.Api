using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class IssueFieldService(JiraClient jira) : IIssueFieldService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (!cache.CustomFields.Any())
		{
			var remoteFields = await _jira.RestClient.ExecuteRequestAsync<RemoteField[]>(Method.Get, "rest/api/2/field", null, cancellationToken).ConfigureAwait(false);
			var results = remoteFields.Where(f => f.IsCustomField).Select(f => new CustomField(f));
			cache.CustomFields.TryAdd(results);
		}

		return cache.CustomFields.Values;
	}

	public async Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CustomFieldFetchOptions options, CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;
		var projectKey = options.ProjectKeys.FirstOrDefault();
		var issueTypeId = options.IssueTypeIds.FirstOrDefault();
		var issueTypeName = options.IssueTypeNames.FirstOrDefault();

		if (!string.IsNullOrEmpty(issueTypeId) || !string.IsNullOrEmpty(issueTypeName))
		{
			projectKey = $"{projectKey}::{issueTypeId}::{issueTypeName}";
		}
		else if (string.IsNullOrEmpty(projectKey))
		{
			return await GetCustomFieldsAsync(cancellationToken);
		}

		if (!cache.ProjectCustomFields.TryGetValue(projectKey, out JiraEntityDictionary<CustomField> fields))
		{
			var resource = $"rest/api/2/issue/createmeta?expand=projects.issuetypes.fields";

			if (options.ProjectKeys.Any())
			{
				resource += $"&projectKeys={string.Join(",", options.ProjectKeys)}";
			}

			if (options.IssueTypeIds.Any())
			{
				resource += $"&issuetypeIds={string.Join(",", options.IssueTypeIds)}";
			}

			if (options.IssueTypeNames.Any())
			{
				resource += $"&issuetypeNames={string.Join(",", options.IssueTypeNames)}";
			}

			var jObject = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
			var jProject = jObject["projects"].FirstOrDefault() ?? throw new InvalidOperationException($"Project with key '{projectKey}' was not found on the JiraClient server.");
			var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
			var customFields = jProject["issuetypes"].SelectMany(issueType => GetCustomFieldsFromIssueType(issueType, serializerSettings));
			var distinctFields = customFields.GroupBy(c => c.Id).Select(g => g.First());

			cache.ProjectCustomFields.TryAdd(projectKey, new JiraEntityDictionary<CustomField>(distinctFields));
		}

		return cache.ProjectCustomFields[projectKey].Values;
	}

	public Task<IEnumerable<CustomField>> GetCustomFieldsForProjectAsync(string projectKey, CancellationToken cancellationToken)
	{
		var options = new CustomFieldFetchOptions();
		options.ProjectKeys.Add(projectKey);

		return GetCustomFieldsAsync(options, cancellationToken);
	}

	private static IEnumerable<CustomField> GetCustomFieldsFromIssueType(JToken issueType, JsonSerializerSettings serializerSettings)
	{
		return ((JObject)issueType["fields"]).Properties()
			.Where(f => f.Name.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase))
			.Select(f => JsonConvert.DeserializeObject<RemoteField>(f.Value.ToString(), serializerSettings))
			.Select(remoteField => new CustomField(remoteField));
	}
}
