using Jira.Api.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

namespace Jira.Api.Services;

internal partial class IssueService
{
	public async Task<IEnumerable<string>> GetPropertyKeysAsync(string issueKey, CancellationToken cancellationToken)
	{
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var serializer = JsonSerializer.Create(serializerSettings);

		var resource = $"rest/api/2/issue/{issueKey}/properties";
		var response = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var propertyRefsJson = response["keys"];
		var propertyRefs = propertyRefsJson.ToObject<IEnumerable<RemoteEntityPropertyReference>>(serializer);
		return propertyRefs.Select(x => x.key);
	}

	public async Task<ReadOnlyDictionary<string, JToken>> GetPropertiesAsync(
		string issueKey,
		IEnumerable<string> propertyKeys,
		CancellationToken cancellationToken)
	{
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var serializer = JsonSerializer.Create(serializerSettings);

		var requestTasks = propertyKeys.Select((propertyKey) =>
		{
			// NOTE; There are no character limits on property keys
			var urlEncodedKey = WebUtility.UrlEncode(propertyKey);
			var resource = $"rest/api/2/issue/{issueKey}/properties/{urlEncodedKey}";

			return _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ContinueWith<JToken?>(t =>
			 {
				 if (!t.IsFaulted)
				 {
					 return t.Result;
				 }
				 else if (t.Exception != null && t.Exception.InnerException is ResourceNotFoundException)
				 {
					 // WARN; Null result needs to be filtered out during processing!
					 return null;
				 }
				 else
				 {
					 throw t.Exception!;
				 }
			 });
		});

		var responses = await Task.WhenAll(requestTasks).ConfigureAwait(false);

		// NOTE; Response includes the key and value
		var transformedResponses = responses
			.Where(x => x != null)
			.Select(x => x.ToObject<RemoteEntityProperty>(serializer))
			.ToDictionary(x => x.key, x => x.value);

		return new ReadOnlyDictionary<string, JToken>(transformedResponses);
	}

	public Task SetPropertyAsync(string issueKey, string propertyKey, JToken obj, CancellationToken cancellationToken)
	{
		if (propertyKey.Length <= 0 || propertyKey.Length >= 256)
		{
			throw new ArgumentOutOfRangeException(nameof(propertyKey), "PropertyKey length must be between 0 and 256 (both exclusive).");
		}

		var urlEncodedKey = WebUtility.UrlEncode(propertyKey);
		var resource = $"rest/api/2/issue/{issueKey}/properties/{urlEncodedKey}";
		return _jira.RestClient.ExecuteRequestAsync(Method.Put, resource, obj, cancellationToken);
	}

	public async Task DeletePropertyAsync(string issueKey, string propertyKey, CancellationToken cancellationToken)
	{
		var urlEncodedKey = WebUtility.UrlEncode(propertyKey);
		var resource = $"rest/api/2/issue/{issueKey}/properties/{urlEncodedKey}";

		try
		{
			await _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken).ConfigureAwait(false);
		}
		catch (ResourceNotFoundException)
		{
			// No-op. The resource that we are trying to delete doesn't exist anyway.
		}
	}

	/// <summary>
	/// Re-ranks one or more issues within a sprint or backlog using the Jira Agile API.
	/// </summary>
	/// <param name="issueKeys">The keys of the issues to reorder. Must not be null or empty.</param>
	/// <param name="rankBeforeIssue">Optional. The key of the issue to rank these issues before.</param>
	/// <param name="rankAfterIssue">Optional. The key of the issue to rank these issues after.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <remarks>
	/// Invokes Jira Agile REST API (PUT rest/agile/1.0/issue/rank).
	/// Exactly one of rankBeforeIssue or rankAfterIssue must be provided.
	/// </remarks>
	public Task ReRankAsync(IEnumerable<string> issueKeys, string? rankBeforeIssue = null, string? rankAfterIssue = null, CancellationToken cancellationToken = default)
	{
		if (issueKeys == null || !issueKeys.Any())
		{
			throw new ArgumentNullException(nameof(issueKeys), "At least one issue key must be provided.");
		}

		if (!string.IsNullOrEmpty(rankBeforeIssue) && !string.IsNullOrEmpty(rankAfterIssue))
		{
			throw new InvalidOperationException("Only one of rankBeforeIssue or rankAfterIssue can be specified, not both.");
		}

		if (string.IsNullOrEmpty(rankBeforeIssue) && string.IsNullOrEmpty(rankAfterIssue))
		{
			throw new InvalidOperationException("Either rankBeforeIssue or rankAfterIssue must be specified.");
		}

		var resource = "rest/agile/1.0/issue/rank";
		var requestBody = new
		{
			issues = issueKeys.ToArray(),
			rankBeforeIssue,
			rankAfterIssue
		};

		return _jira.RestClient.ExecuteRequestAsync(Method.Put, resource, requestBody, cancellationToken);
	}
}
