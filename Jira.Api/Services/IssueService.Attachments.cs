using Jira.Api.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

namespace Jira.Api.Services;

internal partial class IssueService
{
	public Task<IPagedQueryResult<Issue>> GetSubTasksAsync(string issueKey, int skip, int? take, CancellationToken cancellationToken)
	{
		var jql = $"parent = {issueKey}";
		return GetIssuesFromJqlAsync(jql, skip, take, cancellationToken);
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

	public async Task<Attachment> GetAttachmentAsync(string attachmentId, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/attachment/{attachmentId}";
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var result = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var remoteAttachment = JsonConvert.DeserializeObject<RemoteAttachment>(result.ToString(), serializerSettings);

		return new Attachment(_jira, remoteAttachment);
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
			return result.ToDictionary(i => i.Key.Value);
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
}
