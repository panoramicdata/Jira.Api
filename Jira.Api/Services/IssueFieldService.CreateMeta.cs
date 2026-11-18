using Jira.Api.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jira.Api.Services;

internal partial class IssueFieldService
{
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
				var customField = ToCustomField(field, serializerSettings);
				if (customField is not null)
				{
					fields.Add(customField);
				}
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

	/// <summary>
	/// Converts a createmeta field entry into a <see cref="CustomField"/>, or returns
	/// <see langword="null"/> if the entry is not a custom field or cannot be deserialized.
	/// </summary>
	private static CustomField? ToCustomField(JToken field, JsonSerializerSettings serializerSettings)
	{
		var fieldId = field.Value<string>("fieldId") ?? string.Empty;
		if (!fieldId.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		var remoteField = JsonConvert.DeserializeObject<RemoteField>(field.ToString(), serializerSettings);
		if (remoteField is null)
		{
			return null;
		}

		if (string.IsNullOrEmpty(remoteField.id))
		{
			remoteField.id = fieldId;
		}

		return new CustomField(remoteField);
	}
}
