using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class ServerInfoTest
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetServerInfoWithoutHealthCheck(JiraClient jira)
	{
		var serverInfo = await jira.ServerInfo.GetServerInfoAsync(false, default);

		Assert.Equal("Server", serverInfo.DeploymentType);
		Assert.Equal("Your Company JiraClient", serverInfo.ServerTitle);
		Assert.Null(serverInfo.HealthChecks);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetServerInfoWithHealthCheck(JiraClient jira)
	{
		var serverInfo = await jira.ServerInfo.GetServerInfoAsync(true, default);

		Assert.Equal("Server", serverInfo.DeploymentType);
		Assert.Equal("Your Company JiraClient", serverInfo.ServerTitle);
		Assert.NotNull(serverInfo.HealthChecks);
		Assert.NotEmpty(serverInfo.HealthChecks);
	}
}
