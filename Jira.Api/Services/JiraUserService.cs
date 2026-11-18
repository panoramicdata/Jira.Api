using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Jira.Api.Services;

internal class JiraUserService(JiraClient jira) : IJiraUserService
{
	private readonly JiraClient _jira = jira;

	public async Task<JiraUser> CreateUserAsync(JiraUserCreationInfo user, CancellationToken cancellationToken)
	{
		var resource = "rest/api/2/user";
		var requestBody = JToken.FromObject(user);

		return await _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.Post, resource, requestBody, cancellationToken).ConfigureAwait(false);
	}

	public Task DeleteUserAsync(string usernameOrAccountId, CancellationToken cancellationToken)
		=> _jira.RestClient.ExecuteRequestAsync(Method.Delete, UserResource(usernameOrAccountId), null, cancellationToken);

	public Task<JiraUser> GetUserAsync(string usernameOrAccountId, CancellationToken cancellationToken)
		=> _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.Get, UserResource(usernameOrAccountId), null, cancellationToken);

	private string UserResource(string usernameOrAccountId)
	{
		var queryString = _jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username";
		return $"rest/api/2/user?{queryString}={WebUtility.UrlEncode(usernameOrAccountId)}";
	}

	private Task<IEnumerable<JiraUser>> GetUsersAsync(string resource, CancellationToken cancellationToken)
		=> _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraUser>>(Method.Get, resource, null, cancellationToken);
	public Task<IEnumerable<JiraUser>> SearchUsersAsync(
		string query,
		JiraUserStatus userStatus,
		int skip,
		int take,
		CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/user/search?{(_jira.RestClient.Settings.EnableUserPrivacyMode ? "query" : "username")}={WebUtility.UrlEncode(query)}&includeActive={userStatus.HasFlag(JiraUserStatus.Active)}&includeInactive={userStatus.HasFlag(JiraUserStatus.Inactive)}&startAt={skip}&maxResults={take}";

		return GetUsersAsync(resource, cancellationToken);
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

		return GetUsersAsync(resourceSb.ToString(), cancellationToken);
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

		return GetUsersAsync(resourceSb.ToString(), cancellationToken);
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

		return GetUsersAsync(resourceSb.ToString(), cancellationToken);
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
