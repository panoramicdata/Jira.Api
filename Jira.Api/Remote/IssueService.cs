using Jira.Api.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class IssueService(Jira jira, JiraRestClientSettings restSettings) : IIssueService
{
	private const int DEFAULT_MAX_ISSUES_PER_REQUEST = 20;
	private const string ALL_FIELDS_QUERY_STRING = "*all";

	private readonly Jira _jira = jira;
	private readonly JiraRestClientSettings _restSettings = restSettings;
	private readonly string[] _excludedFields = ["comment", "attachment", "issuelinks", "subtasks", "watches", "worklog"];

	private JsonSerializerSettings _serializerSettings;

	public JiraQueryable<Issue> Queryable
	{
		get
		{
			var translator = _jira.Services.Get<IJqlExpressionVisitor>();
			var provider = new JiraQueryProvider(translator, this);
			return new JiraQueryable<Issue>(provider);
		}
	}

	public bool ValidateQuery { get; set; } = true;

	public int MaxIssuesPerRequest { get; set; } = DEFAULT_MAX_ISSUES_PER_REQUEST;

	private async Task<JsonSerializerSettings> GetIssueSerializerSettingsAsync(CancellationToken cancellationToken)
	{
		if (_serializerSettings == null)
		{
			var fieldService = _jira.Services.Get<IIssueFieldService>();
			var customFields = await fieldService.GetCustomFieldsAsync(cancellationToken).ConfigureAwait(false);
			var remoteFields = customFields.Select(f => f.RemoteField);

			var customFieldSerializers = new Dictionary<string, ICustomFieldValueSerializer>(_restSettings.CustomFieldSerializers, StringComparer.InvariantCultureIgnoreCase);

			_serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
			_serializerSettings.Converters.Add(new RemoteIssueJsonConverter(remoteFields, customFieldSerializers));
		}

		return _serializerSettings;
	}

	public async Task<Issue> GetIssueAsync(string issueKey, CancellationToken cancellationToken)
	{
		var excludedFields = string.Join(",", _excludedFields.Select(field => $"-{field}"));
		var fields = $"{ALL_FIELDS_QUERY_STRING},{excludedFields}";
		var resource = $"rest/api/2/issue/{issueKey}?fields={fields}";
		var response = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var serializerSettings = await GetIssueSerializerSettingsAsync(cancellationToken).ConfigureAwait(false);
		var issue = JsonConvert.DeserializeObject<RemoteIssueWrapper>(response.ToString(), serializerSettings);

		return new Issue(_jira, issue.RemoteIssue);
	}

	public Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(
		string jql,
		int skip,
		int? maxIssues,
		CancellationToken cancellationToken)
	{
		var options = new IssueSearchOptions(jql)
		{
			StartAt = skip,
			MaxIssuesPerRequest = maxIssues,
			ValidateQuery = ValidateQuery
		};

		return GetIssuesFromJqlAsync(options, cancellationToken);
	}

	public async Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(IssueSearchOptions options, CancellationToken cancellationToken)
	{
		if (_jira.Debug)
		{
			Trace.WriteLine("[GetFromJqlAsync] JQL: " + options.Jql);
		}

		var fields = new List<string>();
		if (options.AdditionalFields == null || !options.AdditionalFields.Any())
		{
			fields.Add(ALL_FIELDS_QUERY_STRING);
			fields.AddRange(_excludedFields.Select(field => $"-{field}"));
		}
		else if (options.FetchBasicFields)
		{
			var excludedFields = _excludedFields.Where(excludedField => !options.AdditionalFields.Contains(excludedField, StringComparer.OrdinalIgnoreCase)).ToArray();
			fields.Add(ALL_FIELDS_QUERY_STRING);
			fields.AddRange(excludedFields.Select(field => $"-{field}"));
		}
		else
		{
			fields.AddRange(options.AdditionalFields.Select(field => field.Trim()));
		}

		var parameters = new
		{
			jql = options.Jql,
			startAt = options.StartAt,
			maxResults = options.MaxIssuesPerRequest ?? MaxIssuesPerRequest,
			validateQuery = options.ValidateQuery,
			fields
		};

		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Post, "rest/api/2/search", parameters, cancellationToken).ConfigureAwait(false);
		var serializerSettings = await GetIssueSerializerSettingsAsync(cancellationToken).ConfigureAwait(false);
		var issues = result["issues"]
			.Cast<JObject>()
			.Select(issueJson =>
			{
				var remoteIssue = JsonConvert.DeserializeObject<RemoteIssueWrapper>(issueJson.ToString(), serializerSettings).RemoteIssue;
				return new Issue(_jira, remoteIssue);
			});

		return PagedQueryResult<Issue>.FromJson((JObject)result, issues);
	}

	public async Task UpdateIssueAsync(Issue issue, IssueUpdateOptions options, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issue.Key.Value}";
		if (options.SuppressEmailNotification)
		{
			resource += "?notifyUsers=false";
		}

		var fieldProvider = issue as IRemoteIssueFieldProvider;
		var remoteFields = await fieldProvider.GetRemoteFieldValuesAsync(cancellationToken).ConfigureAwait(false);
		var remoteIssue = await issue.ToRemoteAsync(cancellationToken).ConfigureAwait(false);
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
		var remoteIssue = await issue.ToRemoteAsync(cancellationToken).ConfigureAwait(false);
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

		var fieldsJsonSerializerSettings = new JsonSerializerSettings()
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
		WorkflowTransitionUpdates updates,
		CancellationToken cancellationToken)
	{
		string actionId;
		if (int.TryParse(actionNameOrId, out int actionIdInt))
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
		var remoteIssue = await issue.ToRemoteAsync(cancellationToken).ConfigureAwait(false);
		var fields = await BuildFieldsObjectFromIssueAsync(remoteIssue, remoteFields, cancellationToken).ConfigureAwait(false);
		var updatesObject = new JObject();

		if (!string.IsNullOrEmpty(updates.Comment))
		{
			updatesObject.Add("comment", new JArray(new JObject[]
			{
					new(new JProperty("add",
						new JObject(new JProperty("body", updates.Comment))))
			}));
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

	public async Task<Comment> AddCommentAsync(string issueKey, Comment comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(comment.Author))
		{
			throw new InvalidOperationException("Unable to add comment due to missing author field.");
		}

		var resource = $"rest/api/2/issue/{issueKey}/comment";
		var remoteComment = await _jira.RestClient.ExecuteRequestAsync<RemoteComment>(Method.Post, resource, comment.ToRemote(), cancellationToken).ConfigureAwait(false);
		return new Comment(remoteComment);
	}

	public async Task<Comment> UpdateCommentAsync(string issueKey, Comment comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(comment.Id))
		{
			throw new InvalidOperationException("Unable to update comment due to missing id field.");
		}

		var resource = $"rest/api/2/issue/{issueKey}/comment/{comment.Id}";
		var remoteComment = await _jira.RestClient.ExecuteRequestAsync<RemoteComment>(Method.Put, resource, comment.ToRemote(), cancellationToken).ConfigureAwait(false);
		return new Comment(remoteComment);
	}

	public async Task<IPagedQueryResult<Comment>> GetPagedCommentsAsync(string issueKey, int skip, int? take, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}/comment?startAt={skip}";

		if (take.HasValue)
		{
			resource += $"&maxResults={take.Value}";
		}

		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var comments = result["comments"]
			.Cast<JObject>()
			.Select(commentJson =>
			{
				var remoteComment = JsonConvert.DeserializeObject<RemoteComment>(commentJson.ToString(), serializerSettings);
				return new Comment(remoteComment);
			});

		return PagedQueryResult<Comment>.FromJson((JObject)result, comments);
	}

	public Task<IEnumerable<IssueTransition>> GetActionsAsync(string issueKey, CancellationToken cancellationToken)
	{
		return GetActionsAsync(issueKey, false, cancellationToken);
	}

	public async Task<IEnumerable<IssueTransition>> GetActionsAsync(string issueKey, bool expandTransitionFields, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}/transitions";
		if (expandTransitionFields)
		{
			resource += "?expand=transitions.fields";
		}

		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var remoteTransitions = JsonConvert.DeserializeObject<RemoteTransition[]>(result["transitions"].ToString(), serializerSettings);

		return remoteTransitions.Select(transition => new IssueTransition(transition));
	}

	public async Task<IEnumerable<Attachment>> GetAttachmentsAsync(string issueKey, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}?fields=attachment";
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var attachmentsJson = result["fields"]["attachment"];
		var attachments = JsonConvert.DeserializeObject<RemoteAttachment[]>(attachmentsJson.ToString(), serializerSettings);

		return attachments.Select(remoteAttachment => new Attachment(_jira, remoteAttachment));
	}

	public async Task<string[]> GetLabelsAsync(string issueKey, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}?fields=labels";
		var serializerSettings = await GetIssueSerializerSettingsAsync(cancellationToken).ConfigureAwait(false);
		var response = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var issue = JsonConvert.DeserializeObject<RemoteIssueWrapper>(response.ToString(), serializerSettings);
		return issue.RemoteIssue.labels ?? [];
	}

	public Task SetLabelsAsync(string issueKey, string[] labels, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}";
		return _jira.RestClient.ExecuteRequestAsync(Method.Put, resource, new
		{
			fields = new
			{
				labels
			}

		}, cancellationToken);
	}

	public async Task<IEnumerable<JiraUser>> GetWatchersAsync(string issueKey, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(issueKey))
		{
			throw new InvalidOperationException("Unable to interact with the watchers resource, make sure the issue has been created.");
		}

		var resourceUrl = $"rest/api/2/issue/{issueKey}/watchers";
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resourceUrl, null, cancellationToken).ConfigureAwait(false);
		var watchersJson = result["watchers"];
		return watchersJson.Select(watcherJson => JsonConvert.DeserializeObject<JiraUser>(watcherJson.ToString(), serializerSettings));
	}

	public async Task<IEnumerable<IssueChangeLog>> GetChangeLogsAsync(string issueKey, CancellationToken cancellationToken)
	{
		var resourceUrl = $"rest/api/2/issue/{issueKey}?fields=created&expand=changelog";
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var response = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resourceUrl, null, cancellationToken).ConfigureAwait(false);
		var result = Enumerable.Empty<IssueChangeLog>();
		var changeLogs = response["changelog"];
		if (changeLogs != null)
		{
			var histories = changeLogs["histories"];
			if (histories != null)
			{
				result = histories.Select(history => JsonConvert.DeserializeObject<IssueChangeLog>(history.ToString(), serializerSettings));
			}
		}

		return result;
	}

	public Task DeleteWatcherAsync(string issueKey, string username, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(issueKey))
		{
			throw new InvalidOperationException("Unable to interact with the watchers resource, make sure the issue has been created.");
		}

		var queryString = _jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username";
		var resourceUrl = string.Format($"rest/api/2/issue/{issueKey}/watchers?{queryString}={Uri.EscapeUriString(username)}");
		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resourceUrl, null, cancellationToken);
	}

	public Task AddWatcherAsync(string issueKey, string username, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(issueKey))
		{
			throw new InvalidOperationException("Unable to interact with the watchers resource, make sure the issue has been created.");
		}

		var requestBody = $"\"{username}\"";
		var resourceUrl = $"rest/api/2/issue/{issueKey}/watchers";
		return _jira.RestClient.ExecuteRequestAsync(Method.Post, resourceUrl, requestBody, cancellationToken);
	}

	public Task<IPagedQueryResult<Issue>> GetSubTasksAsync(string issueKey, int skip, int? maxIssues, CancellationToken cancellationToken)
	{
		var jql = $"parent = {issueKey}";
		return GetIssuesFromJqlAsync(jql, skip, maxIssues, cancellationToken);
	}

	public Task AddAttachmentsAsync(string issueKey, UploadAttachmentInfo[] attachments, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}/attachments";
		var request = new RestRequest
		{
			Method = Method.Post,
			Resource = resource
		};
		request.AddHeader("X-Atlassian-Token", "nocheck");
		request.AlwaysMultipartFormData = true;

		foreach (var attachment in attachments)
		{
			request.AddFile("file", attachment.Data, attachment.Name);
		}

		return _jira.RestClient.ExecuteRequestAsync(request, cancellationToken);
	}

	public Task DeleteAttachmentAsync(string issueKey, string attachmentId, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/attachment/{attachmentId}";

		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken);
	}

	public async Task<IDictionary<string, Issue>> GetIssuesAsync(IEnumerable<string> issueKeys, CancellationToken cancellationToken)
	{
		if (issueKeys.Any())
		{
			var distinctKeys = issueKeys.Distinct();
			var jql = $"key in ({string.Join(",", distinctKeys)})";
			var options = new IssueSearchOptions(jql)
			{
				MaxIssuesPerRequest = distinctKeys.Count(),
				ValidateQuery = false
			};

			var result = await GetIssuesFromJqlAsync(options, cancellationToken).ConfigureAwait(false);
			return result.ToDictionary<Issue, string>(i => i.Key.Value);
		}
		else
		{
			return new Dictionary<string, Issue>();
		}
	}

	public Task<IDictionary<string, Issue>> GetIssuesAsync(params string[] issueKeys)
	{
		return GetIssuesAsync(issueKeys, default);
	}

	public Task<IEnumerable<Comment>> GetCommentsAsync(string issueKey, CancellationToken cancellationToken)
	{
		var options = new CommentQueryOptions();
		options.Expand.Add("properties");

		return GetCommentsAsync(issueKey, options, cancellationToken);
	}

	public async Task<IEnumerable<Comment>> GetCommentsAsync(string issueKey, CommentQueryOptions options, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}/comment";

		if (options.Expand.Any())
		{
			resource += $"?expand={string.Join(",", options.Expand)}";
		}

		var issueJson = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var commentJson = issueJson["comments"];

		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var remoteComments = JsonConvert.DeserializeObject<RemoteComment[]>(commentJson.ToString(), serializerSettings);

		return remoteComments.Select(c => new Comment(c));
	}

	public Task DeleteCommentAsync(string issueKey, string commentId, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}/comment/{commentId}";

		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken);
	}

	public async Task<Worklog> AddWorklogAsync(
		string issueKey,
		Worklog worklog,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken)
	{
		var remoteWorklog = worklog.ToRemote();
		string? queryString = null;

		if (worklogStrategy == WorklogStrategy.RetainRemainingEstimate)
		{
			queryString = "adjustEstimate=leave";
		}
		else if (worklogStrategy == WorklogStrategy.NewRemainingEstimate)
		{
			queryString = "adjustEstimate=new&newEstimate=" + Uri.EscapeDataString(newEstimate);
		}

		var resource = $"rest/api/2/issue/{issueKey}/worklog?{queryString}";
		var serverWorklog = await _jira.RestClient.ExecuteRequestAsync<RemoteWorklog>(Method.Post, resource, remoteWorklog, cancellationToken).ConfigureAwait(false);
		return new Worklog(serverWorklog);
	}

	public Task DeleteWorklogAsync(
		string issueKey,
		string worklogId,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken)
	{
		string? queryString = null;

		if (worklogStrategy == WorklogStrategy.RetainRemainingEstimate)
		{
			queryString = "adjustEstimate=leave";
		}
		else if (worklogStrategy == WorklogStrategy.NewRemainingEstimate)
		{
			queryString = "adjustEstimate=new&newEstimate=" + Uri.EscapeDataString(newEstimate);
		}

		var resource = $"rest/api/2/issue/{issueKey}/worklog/{worklogId}?{queryString}";
		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken);
	}

	public async Task<IEnumerable<Worklog>> GetWorklogsAsync(string issueKey, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}/worklog";
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var response = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var worklogsJson = response["worklogs"];
		var remoteWorklogs = JsonConvert.DeserializeObject<RemoteWorklog[]>(worklogsJson.ToString(), serializerSettings);

		return remoteWorklogs.Select(w => new Worklog(w));
	}

	public async Task<Worklog> GetWorklogAsync(string issueKey, string worklogId, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}/worklog/{worklogId}";
		var remoteWorklog = await _jira.RestClient.ExecuteRequestAsync<RemoteWorklog>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		return new Worklog(remoteWorklog);
	}

	public Task DeleteIssueAsync(string issueKey, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/issue/{issueKey}";
		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken);
	}

	public Task AssignIssueAsync(string issueKey, string assignee, CancellationToken cancellationToken)
	{
		var resource = $"/rest/api/2/issue/{issueKey}/assignee";

		object body = new { name = assignee };
		if (_jira.RestClient.Settings.EnableUserPrivacyMode)
		{
			body = new { accountId = assignee };
		}

		return _jira.RestClient.ExecuteRequestAsync(Method.Put, resource, body, cancellationToken);
	}

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

			return _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ContinueWith<JToken>(t =>
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
					 throw t.Exception;
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
}
