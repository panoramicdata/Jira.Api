using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class IssueLinkService(Jira jira) : IIssueLinkService
{
	private readonly Jira _jira = jira;

	public Task CreateLinkAsync(
		string outwardIssueKey,
		string inwardIssueKey,
		string linkName,
		string comment,
		CancellationToken cancellationToken)
	{
		var bodyObject = new JObject
		{
			{ "type", new JObject(new JProperty("name", linkName)) },
			{ "inwardIssue", new JObject(new JProperty("key", inwardIssueKey)) },
			{ "outwardIssue", new JObject(new JProperty("key", outwardIssueKey)) }
		};

		if (!string.IsNullOrEmpty(comment))
		{
			bodyObject.Add("comment", new JObject(new JProperty("body", comment)));
		}

		return _jira.RestClient.ExecuteRequestAsync(Method.Post, "rest/api/2/issueLink", bodyObject, cancellationToken);
	}

	public async Task<IEnumerable<IssueLink>> GetLinksForIssueAsync(string issueKey, CancellationToken cancellationToken)
	{
		var issue = await _jira.Issues.GetIssueAsync(issueKey, cancellationToken);
		return await GetLinksForIssueAsync(issue, null, cancellationToken);
	}

	public async Task<IEnumerable<IssueLink>> GetLinksForIssueAsync(
		Issue issue,
		IEnumerable<string>? linkTypeNames,
		CancellationToken cancellationToken)
	{
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
		var resource = $"rest/api/2/issue/{issue.Key.Value}?fields=issuelinks,created";
		var issueLinksResult = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var issueLinksJson = issueLinksResult["fields"]["issuelinks"] ?? throw new InvalidOperationException("There is no 'issueLinks' field on the issue data, make sure issue linking is turned on in JIRA.");
		var issueLinks = issueLinksJson.Cast<JObject>();
		var filteredIssueLinks = issueLinks;

		if (linkTypeNames != null)
		{
			filteredIssueLinks = issueLinks.Where(link => linkTypeNames.Contains(link["type"]["name"].ToString(), StringComparer.InvariantCultureIgnoreCase));
		}

		var issuesToGet = filteredIssueLinks.Select(issueLink =>
		{
			var issueJson = issueLink["outwardIssue"] ?? issueLink["inwardIssue"];
			return issueJson["key"].Value<string>();
		}).ToList();

		var issuesMap = await _jira.Issues.GetIssuesAsync(issuesToGet, cancellationToken).ConfigureAwait(false);
		if (!issuesMap.ContainsKey(issue.Key.ToString()))
		{
			issuesMap.Add(issue.Key.ToString(), issue);
		}


		return filteredIssueLinks.Select(issueLink =>
		{
			var linkType = JsonConvert.DeserializeObject<IssueLinkType>(issueLink["type"].ToString(), serializerSettings);
			var outwardIssue = issueLink["outwardIssue"];
			var inwardIssue = issueLink["inwardIssue"];
			var outwardIssueKey = outwardIssue != null ? (string)outwardIssue["key"] : null;
			var inwardIssueKey = inwardIssue != null ? (string)inwardIssue["key"] : null;
			return new IssueLink(
				linkType,
				outwardIssueKey == null ? issue : issuesMap[outwardIssueKey],
				inwardIssueKey == null ? issue : issuesMap[inwardIssueKey]);
		});
	}

	public async Task<IEnumerable<IssueLinkType>> GetLinkTypesAsync(CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;
		var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;

		if (!cache.LinkTypes.Any())
		{
			var results = await _jira.RestClient.ExecuteRequestAsync(Method.Get, "rest/api/2/issueLinkType", null, cancellationToken).ConfigureAwait(false);
			var linkTypes = results["issueLinkTypes"]
				.Cast<JObject>()
				.Select(issueLinkJson => JsonConvert.DeserializeObject<IssueLinkType>(issueLinkJson.ToString(), serializerSettings));

			cache.LinkTypes.TryAdd(linkTypes);
		}

		return cache.LinkTypes.Values;
	}
}
