using AwesomeAssertions;

namespace Jira.Api.Test.Integration;

public class ServerInfoTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetServerInfoWithoutHealthCheck(JiraClient jira)
	{
		var serverInfo = await jira.ServerInfo.GetServerInfoAsync(false, CancellationToken);

		serverInfo.DeploymentType.Should().Be("Server");
		serverInfo.ServerTitle.Should().Be("Your Company JiraClient");
		serverInfo.HealthChecks.Should().BeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetServerInfoWithHealthCheck(JiraClient jira)
	{
		var serverInfo = await jira.ServerInfo.GetServerInfoAsync(true, CancellationToken);

		serverInfo.DeploymentType.Should().Be("Server");
		serverInfo.ServerTitle.Should().Be("Your Company JiraClient");
		serverInfo.HealthChecks.Should().NotBeNull();
		Assert.NotEmpty(serverInfo.HealthChecks);
	}
}



