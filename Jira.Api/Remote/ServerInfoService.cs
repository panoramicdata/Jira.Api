using RestSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class ServerInfoService(Jira jira) : IServerInfoService
{
	private readonly Jira _jira = jira;

	public async Task<ServerInfo> GetServerInfoAsync(bool doHealthCheck = false, CancellationToken token = default)
	{
		var resource = "rest/api/2/serverInfo";
		if (doHealthCheck)
		{
			resource += "?doHealthCheck=true";
		}

		var remoteServerInfo = await _jira.RestClient.ExecuteRequestAsync<RemoteServerInfo>(Method.GET, resource, null, token).ConfigureAwait(false);

		return new ServerInfo(remoteServerInfo);
	}
}
