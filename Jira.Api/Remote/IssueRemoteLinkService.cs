using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class IssueRemoteLinkService(JiraClient jira) : IIssueRemoteLinkService
{
	private readonly JiraClient _jira = jira;

	public Task CreateRemoteLinkAsync(string issueKey, string remoteUrl, string title, string summary, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(title))
		{
			throw new ArgumentNullException(nameof(title), "Title must be supplied.");
		}

		if (string.IsNullOrEmpty(remoteUrl))
		{
			throw new ArgumentNullException(nameof(remoteUrl), "Remote URL must be supplied.");
		}

		var bodyObject = new JObject();
		var bodyObjectContent = new JObject();
		bodyObject.Add("object", bodyObjectContent);

		bodyObjectContent.Add("title", title);
		bodyObjectContent.Add("url", remoteUrl);

		if (!string.IsNullOrEmpty(summary))
		{
			bodyObjectContent.Add("summary", summary);
		}

		return _jira.RestClient.ExecuteRequestAsync(Method.Post, $"rest/api/2/issue/{issueKey}/remotelink", bodyObject, cancellationToken);
	}

	public async Task<IEnumerable<IssueRemoteLink>> GetRemoteLinksForIssueAsync(string issueKey, CancellationToken cancellationToken)
	{
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var resource = $"rest/api/2/issue/{issueKey}/remotelink";
		var remoteLinksJson = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);

		var links = remoteLinksJson.Cast<JObject>();
		var result = links.Select(json =>
		{
			var objJson = json["object"];
			var title = objJson["title"]?.Value<string>();
			var url = objJson["url"].Value<string>();
			var summary = objJson["summary"]?.Value<string>();
			return new IssueRemoteLink(url, title, summary);
		});
		return result;
	}
}
