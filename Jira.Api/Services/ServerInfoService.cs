namespace Jira.Api.Services;

internal class ServerInfoService(JiraClient jira) : IServerInfoService
{
	private readonly JiraClient _jira = jira;

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
