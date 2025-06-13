using RestSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

internal class ServerInfoService(Jira jira) : IServerInfoService
{
	private readonly Jira _jira = jira;

	public async Task<ServerInfo> GetServerInfoAsync(bool doHealthCheck, CancellationToken cancellationToken)
	{
		var resource = "rest/api/2/serverInfo";
		if (doHealthCheck)
		{
			resource += "?doHealthCheck=true";
		}

		var remoteServerInfo = await _jira.RestClient.ExecuteRequestAsync<RemoteServerInfo>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);

		return new ServerInfo(remoteServerInfo);
	}
}
