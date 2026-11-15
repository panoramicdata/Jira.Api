using Jira.Api.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jira.Api.Services;

internal partial class IssueFieldService(JiraClient jira) : IIssueFieldService
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
}
