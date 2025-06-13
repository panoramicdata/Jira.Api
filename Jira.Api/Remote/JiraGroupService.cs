using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class JiraGroupService(JiraClient jira) : IJiraGroupService
{
	private readonly JiraClient _jira = jira;

	public Task AddUserAsync(
		string groupName,
		string username,
		CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/group/user?groupName={WebUtility.UrlEncode(groupName)}";
		object body = new { name = username };
		if (_jira.RestClient.Settings.EnableUserPrivacyMode)
		{
			body = new { accountId = username };
		}

		var requestBody = JToken.FromObject(body);
		return _jira.RestClient.ExecuteRequestAsync(Method.Post, resource, requestBody, cancellationToken);
	}

	public Task CreateGroupAsync(string groupName, CancellationToken cancellationToken)
	{
		var resource = "rest/api/2/group";
		var requestBody = JToken.FromObject(new { name = groupName });

		return _jira.RestClient.ExecuteRequestAsync(Method.Post, resource, requestBody, cancellationToken);
	}

	public Task DeleteGroupAsync(string groupName, string? swapGroupName, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/group?groupName={WebUtility.UrlEncode(groupName)}";

		if (!string.IsNullOrEmpty(swapGroupName))
		{
			resource += $"&swapGroup={WebUtility.UrlEncode(swapGroupName)}";
		}

		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken);
	}

	/// <summary>
	/// Get users
	/// </summary>
	/// <param name="groupName"></param>
	/// <param name="includeInactiveUsers"></param>
	/// <param name="skip"></param>
	/// <param name="take">Suggest 50</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async Task<IPagedQueryResult<JiraUser>> GetUsersAsync(
		string groupName,
		bool includeInactiveUsers,
		int skip,
		int take,
		CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/group/member?groupName={WebUtility.UrlEncode(groupName)}&includeInactiveUsers={includeInactiveUsers}&startAt={skip}&maxResults={take}";

		var response = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		var serializerSetting = _jira.RestClient.Settings.JsonSerializerSettings;
		var users = response["values"]
			.Cast<JObject>()
			.Select(valuesJson => JsonConvert.DeserializeObject<JiraUser>(valuesJson.ToString(), serializerSetting));

		return PagedQueryResult<JiraUser>.FromJson((JObject)response, users);
	}

	public Task RemoveUserAsync(string groupName, string username, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/group/user?groupName={WebUtility.UrlEncode(groupName)}&{(_jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username")}={WebUtility.UrlEncode(username)}";

		return _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, cancellationToken);

	}
}
