using Jira.Api.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

namespace Jira.Api.Services;

internal partial class IssueService
{
	public async Task UpdateIssueAsync(Issue issue, IssueUpdateOptions options, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issue.Key.Value}";
		if (options.SuppressEmailNotification)
		{
			resource += "?notifyUsers=false";
		}

		var fieldProvider = issue as IRemoteIssueFieldProvider;
		var remoteFields = await fieldProvider.GetRemoteFieldValuesAsync(cancellationToken).ConfigureAwait(false);
		var remoteIssue = await issue.ToRemoteIssueAsync(cancellationToken).ConfigureAwait(false);
		var fields = await BuildFieldsObjectFromIssueAsync(remoteIssue, remoteFields, cancellationToken).ConfigureAwait(false);

		await _jira.RestClient.ExecuteRequestAsync(Method.Put, resource, new { fields }, cancellationToken).ConfigureAwait(false);
	}

	public Task UpdateIssueAsync(Issue issue, CancellationToken cancellationToken)
	{
		var options = new IssueUpdateOptions();
		return UpdateIssueAsync(issue, options, cancellationToken);
	}

	public async Task<string> CreateIssueAsync(Issue issue, CancellationToken cancellationToken)
	{
		var remoteIssue = await issue.ToRemoteIssueAsync(cancellationToken).ConfigureAwait(false);
		var remoteIssueWrapper = new RemoteIssueWrapper(remoteIssue, issue.ParentIssueKey);
		var serializerSettings = await GetIssueSerializerSettingsAsync(cancellationToken).ConfigureAwait(false);
		var requestBody = JsonConvert.SerializeObject(remoteIssueWrapper, serializerSettings);

		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Post, "rest/api/2/issue", requestBody, cancellationToken).ConfigureAwait(false);
		return (string)result["key"];
	}

	private async Task<JObject> BuildFieldsObjectFromIssueAsync(RemoteIssue remoteIssue, RemoteFieldValue[] remoteFields, CancellationToken cancellationToken)
	{
		var issueWrapper = new RemoteIssueWrapper(remoteIssue);
		var serializerSettings = await GetIssueSerializerSettingsAsync(cancellationToken).ConfigureAwait(false);
		var issueJson = JsonConvert.SerializeObject(issueWrapper, serializerSettings);

		var fieldsJsonSerializerSettings = new JsonSerializerSettings
		{
			DateParseHandling = DateParseHandling.None
		};

		var issueFields = JsonConvert.DeserializeObject<JObject>(issueJson, fieldsJsonSerializerSettings)["fields"] as JObject;
		var updateFields = new JObject();

		foreach (var field in remoteFields)
		{
			var issueFieldName = field.id;
			var issueFieldValue = issueFields[issueFieldName];

			if (issueFieldValue == null && issueFieldName.Equals("components", StringComparison.OrdinalIgnoreCase))
			{
				// JIRA does not accept 'null' as a valid value for the 'components' field.
				//   So if the components field has been cleared it must be set to empty array instead.
				issueFieldValue = new JArray();
			}

			updateFields.Add(issueFieldName, issueFieldValue);
		}

		return updateFields;
	}

	public async Task ExecuteWorkflowActionAsync(
		Issue issue,
		string actionNameOrId,
		WorkflowTransitionUpdates? updates,
		CancellationToken cancellationToken)
	{
		string actionId;
		if (int.TryParse(actionNameOrId, out _))
		{
			actionId = actionNameOrId;
		}
		else
		{
			var actions = await GetActionsAsync(issue.Key.Value, cancellationToken).ConfigureAwait(false);
			var action = actions.FirstOrDefault(a => a.Name.Equals(actionNameOrId, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Workflow action with name '{actionNameOrId}' not found.");
			actionId = action.Id;
		}

		updates ??= new WorkflowTransitionUpdates();

		var resource = $"rest/api/2/issue/{issue.Key.Value}/transitions";
		var fieldProvider = issue as IRemoteIssueFieldProvider;
		var remoteFields = await fieldProvider.GetRemoteFieldValuesAsync(cancellationToken).ConfigureAwait(false);
		var remoteIssue = await issue.ToRemoteIssueAsync(cancellationToken).ConfigureAwait(false);
		var fields = await BuildFieldsObjectFromIssueAsync(remoteIssue, remoteFields, cancellationToken).ConfigureAwait(false);
		var updatesObject = new JObject();

		if (!string.IsNullOrEmpty(updates.Comment))
		{
			updatesObject.Add("comment", new JArray(new JObject(new JProperty("add", new JObject(new JProperty("body", updates.Comment))))));
		}

		var requestBody = new
		{
			transition = new
			{
				id = actionId
			},
			update = updatesObject,
			fields
		};

		await _jira.RestClient.ExecuteRequestAsync(Method.Post, resource, requestBody, cancellationToken).ConfigureAwait(false);
	}

	public async Task<IssueTimeTrackingData> GetTimeTrackingDataAsync(string issueKey, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(issueKey))
		{
			throw new InvalidOperationException("Unable to retrieve time tracking data, make sure the issue has been created.");
		}

		var resource = $"rest/api/2/issue/{issueKey}?fields=timetracking";
		var response = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);

		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var timeTrackingJson = response["fields"]?["timetracking"];

		if (timeTrackingJson != null)
		{
			return JsonConvert.DeserializeObject<IssueTimeTrackingData>(timeTrackingJson.ToString(), serializerSettings);
		}
		else
		{
			return null;
		}
	}

	public async Task<IDictionary<string, IssueFieldEditMetadata>> GetFieldsEditMetadataAsync(string issueKey, CancellationToken cancellationToken)
	{
		var dict = new Dictionary<string, IssueFieldEditMetadata>();
		var resource = $"rest/api/2/issue/{issueKey}/editmeta";
		var serializer = JsonSerializer.Create(_jira.RestClient.Settings.JsonSerializerSettings);
		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		JObject fields = result["fields"].Value<JObject>();

		foreach (var prop in fields.Properties())
		{
			var fieldName = (prop.Value["name"] ?? prop.Name).ToString();
			dict.Add(fieldName, new IssueFieldEditMetadata(prop.Value.ToObject<RemoteIssueFieldMetadata>(serializer)));
		}

		return dict;
	}
}
