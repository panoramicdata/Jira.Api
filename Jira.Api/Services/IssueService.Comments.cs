using Jira.Api.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

namespace Jira.Api.Services;

internal partial class IssueService
{
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

	public Task DeleteWatcherAsync(string issueKey, string usernameOrAccountId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(issueKey))
		{
			throw new InvalidOperationException("Unable to interact with the watchers resource, make sure the issue has been created.");
		}

		var queryString = _jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username";
		var resourceUrl = string.Format($"rest/api/2/issue/{issueKey}/watchers?{queryString}={Uri.EscapeDataString(usernameOrAccountId)}");
		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resourceUrl, null, cancellationToken);
	}

	public Task AddWatcherAsync(string issueKey, string usernameOrAccountId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(issueKey))
		{
			throw new InvalidOperationException("Unable to interact with the watchers resource, make sure the issue has been created.");
		}

		var requestBody = $"\"{usernameOrAccountId}\"";
		var resourceUrl = $"rest/api/2/issue/{issueKey}/watchers";
		return _jira.RestClient.ExecuteRequestAsync(Method.Post, resourceUrl, requestBody, cancellationToken);
	}
}
