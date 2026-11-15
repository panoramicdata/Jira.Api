using Jira.Api.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

namespace Jira.Api.Services;

internal partial class IssueService(JiraClient jira, JiraRestClientSettings restSettings) : IIssueService
{
	private const int DEFAULT_MAX_ISSUES_PER_REQUEST = 20;
	private const string ALL_FIELDS_QUERY_STRING = "*all";

	private readonly JiraClient _jira = jira;
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
}
