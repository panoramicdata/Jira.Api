using Jira.Api.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jira.Api.Services;

internal class IssueFieldService(JiraClient jira) : IIssueFieldService
{
	private readonly JiraClient _jira = jira;

	private const int CreateMetaPageSize = 50;

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
		var projectKey = GetCacheKey(options);

		if (string.IsNullOrEmpty(projectKey))
		{
			return await GetCustomFieldsAsync(cancellationToken);
		}

		if (!cache.ProjectCustomFields.TryGetValue(projectKey, out JiraEntityDictionary<CustomField> _))
		{
			IEnumerable<CustomField> distinctFields;
			try
			{
				// Jira 8.4+ / 9+: per-project, per-issue-type create metadata endpoints.
				distinctFields = await GetProjectCustomFieldsViaCreateMetaAsync(options, cancellationToken).ConfigureAwait(false);
			}
			catch (ResourceNotFoundException)
			{
				// Older Jira (< 8.4) without the per-issue-type endpoints: fall back to the legacy
				// expand-based createmeta (which was removed in Jira 9.0).
				distinctFields = await GetProjectCustomFieldsViaLegacyCreateMetaAsync(options, cancellationToken).ConfigureAwait(false);
			}

			cache.ProjectCustomFields.TryAdd(projectKey, new JiraEntityDictionary<CustomField>(distinctFields));
		}

		return cache.ProjectCustomFields[projectKey].Values;
	}

	/// <summary>
	/// Fetches a project's custom fields using the Jira 8.4+/9+ create-metadata endpoints
	/// (<c>issue/createmeta/{projectKey}/issuetypes</c> and <c>.../issuetypes/{issueTypeId}</c>), which
	/// replaced the expand-based <c>issue/createmeta</c> endpoint removed in Jira 9.0.
	/// </summary>
	private async Task<IEnumerable<CustomField>> GetProjectCustomFieldsViaCreateMetaAsync(CustomFieldFetchOptions options, CancellationToken cancellationToken)
	{
		var projectKey = options.ProjectKeys.First();
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;

		var issueTypes = await GetCreateMetaIssueTypesAsync(projectKey, cancellationToken).ConfigureAwait(false);

		var selectedIssueTypes = issueTypes.Where(it =>
			(!options.IssueTypeIds.Any() || options.IssueTypeIds.Contains(it.Id)) &&
			(!options.IssueTypeNames.Any() || options.IssueTypeNames.Contains(it.Name, StringComparer.OrdinalIgnoreCase)));

		var customFields = new List<CustomField>();
		foreach (var issueType in selectedIssueTypes)
		{
			customFields.AddRange(await GetCreateMetaCustomFieldsForIssueTypeAsync(projectKey, issueType.Id, serializerSettings, cancellationToken).ConfigureAwait(false));
		}

		return customFields.GroupBy(c => c.Id).Select(g => g.First());
	}

	private async Task<IReadOnlyList<(string Id, string Name)>> GetCreateMetaIssueTypesAsync(string projectKey, CancellationToken cancellationToken)
	{
		var issueTypes = new List<(string, string)>();
		var startAt = 0;

		while (true)
		{
			var resource = $"rest/api/2/issue/createmeta/{projectKey}/issuetypes?startAt={startAt}&maxResults={CreateMetaPageSize}";
			var page = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
			var values = page["values"] as JArray ?? [];

			foreach (var value in values)
			{
				var id = value.Value<string>("id");
				if (!string.IsNullOrEmpty(id))
				{
					issueTypes.Add((id, value.Value<string>("name") ?? string.Empty));
				}
			}

			startAt += values.Count;
			var total = page.Value<int?>("total") ?? issueTypes.Count;
			if (values.Count == 0 || startAt >= total)
			{
				break;
			}
		}

		return issueTypes;
	}

	private async Task<IEnumerable<CustomField>> GetCreateMetaCustomFieldsForIssueTypeAsync(string projectKey, string issueTypeId, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken)
	{
		var fields = new List<CustomField>();
		var startAt = 0;

		while (true)
		{
			var resource = $"rest/api/2/issue/createmeta/{projectKey}/issuetypes/{issueTypeId}?startAt={startAt}&maxResults={CreateMetaPageSize}";
			var page = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
			var values = page["values"] as JArray ?? [];

			foreach (var field in values)
			{
				var fieldId = field.Value<string>("fieldId") ?? string.Empty;
				if (!fieldId.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				var remoteField = JsonConvert.DeserializeObject<RemoteField>(field.ToString(), serializerSettings);
				if (remoteField is null)
				{
					continue;
				}

				if (string.IsNullOrEmpty(remoteField.id))
				{
					remoteField.id = fieldId;
				}

				fields.Add(new CustomField(remoteField));
			}

			startAt += values.Count;
			var total = page.Value<int?>("total") ?? fields.Count;
			if (values.Count == 0 || startAt >= total)
			{
				break;
			}
		}

		return fields;
	}

	private async Task<IEnumerable<CustomField>> GetProjectCustomFieldsViaLegacyCreateMetaAsync(CustomFieldFetchOptions options, CancellationToken cancellationToken)
	{
		var resource = BuildCreateMetaResource(options);
		var jObject = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var jProject = jObject["projects"].FirstOrDefault()
			?? throw new InvalidOperationException($"Project with key '{options.ProjectKeys.FirstOrDefault()}' was not found on the JiraClient server.");
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var customFields = jProject["issuetypes"].SelectMany(issueType => GetCustomFieldsFromIssueType(issueType, serializerSettings));
		return customFields.GroupBy(c => c.Id).Select(g => g.First());
	}

	private static string? GetCacheKey(CustomFieldFetchOptions options)
	{
		var projectKey = options.ProjectKeys.FirstOrDefault();
		var issueTypeId = options.IssueTypeIds.FirstOrDefault();
		var issueTypeName = options.IssueTypeNames.FirstOrDefault();

		if (!string.IsNullOrEmpty(issueTypeId) || !string.IsNullOrEmpty(issueTypeName))
		{
			return $"{projectKey}::{issueTypeId}::{issueTypeName}";
		}

		return projectKey;
	}

	private static string BuildCreateMetaResource(CustomFieldFetchOptions options)
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

		return resource;
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
