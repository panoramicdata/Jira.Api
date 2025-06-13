using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class JiraUserService(Jira jira) : IJiraUserService
{
	private readonly Jira _jira = jira;

	public async Task<JiraUser> CreateUserAsync(JiraUserCreationInfo user, CancellationToken cancellationToken)
	{
		var resource = "rest/api/2/user";
		var requestBody = JToken.FromObject(user);

		return await _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.Post, resource, requestBody, cancellationToken).ConfigureAwait(false);
	}

	public Task DeleteUserAsync(string usernameOrAccountId, CancellationToken cancellationToken)
	{
		var queryString = _jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username";
		var resource = $"rest/api/2/user?{queryString}={WebUtility.UrlEncode(usernameOrAccountId)}";
		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken);
	}

	public Task<JiraUser> GetUserAsync(string usernameOrAccountId, CancellationToken cancellationToken)
	{
		var queryString = _jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username";
		var resource = $"rest/api/2/user?{queryString}={WebUtility.UrlEncode(usernameOrAccountId)}";
		return _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.Get, resource, null, cancellationToken);
	}

	public Task<IEnumerable<JiraUser>> SearchUsersAsync(
		string query,
		JiraUserStatus userStatus,
		int skip,
		int take,
		CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/user/search?{(_jira.RestClient.Settings.EnableUserPrivacyMode ? "query" : "username")}={WebUtility.UrlEncode(query)}&includeActive={userStatus.HasFlag(JiraUserStatus.Active)}&includeInactive={userStatus.HasFlag(JiraUserStatus.Inactive)}&startAt={skip}&maxResults={take}";

		return _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraUser>>(Method.Get, resource, null, cancellationToken);
	}

	public Task<IEnumerable<JiraUser>> SearchAssignableUsersForIssueAsync(
		string username,
		string issueKey,
		int skip,
		int take,
		CancellationToken cancellationToken)
	{
		var resourceSb = new StringBuilder($"rest/api/2/user/assignable/search", 200);
		resourceSb.Append($"?username={Uri.EscapeDataString(username)}&issueKey={Uri.EscapeDataString(issueKey)}");
		resourceSb.Append($"&startAt={skip}&maxResults={take}");

		return _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraUser>>(Method.Get, resourceSb.ToString(), null, cancellationToken);
	}

	public Task<IEnumerable<JiraUser>> SearchAssignableUsersForProjectAsync(
		string username,
		string projectKey,
		int skip,
		int take,
		CancellationToken cancellationToken)
	{
		var resourceSb = new StringBuilder($"rest/api/2/user/assignable/search", 200);
		resourceSb.Append($"?username={Uri.EscapeDataString(username)}&project={Uri.EscapeDataString(projectKey)}");
		resourceSb.Append($"&startAt={skip}&maxResults={take}");

		return _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraUser>>(Method.Get, resourceSb.ToString(), null, cancellationToken);
	}

	public Task<IEnumerable<JiraUser>> SearchAssignableUsersForProjectsAsync(
		string username,
		IEnumerable<string> projectKeys,
		int skip,
		int take,
		CancellationToken cancellationToken)
	{
		var resourceSb = new StringBuilder("rest/api/2/user/assignable/multiProjectSearch", 200);
		resourceSb.Append($"?username={username}&projectKeys={string.Join(",", projectKeys)}&startAt={skip}&maxResults={take}");

		return _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraUser>>(Method.Get, resourceSb.ToString(), null, cancellationToken);
	}

	public async Task<JiraUser> GetMyselfAsync(CancellationToken cancellationToken)
	{
		var cache = _jira.Cache;

		if (cache.CurrentUser == null)
		{
			var resource = "rest/api/2/myself";
			var jiraUser = await _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.Get, resource, null, cancellationToken);
			cache.CurrentUser = jiraUser;
		}

		return cache.CurrentUser;
	}
}
