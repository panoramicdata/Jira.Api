namespace Jira.Api.Test;

public class ServerInfoServiceTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Fact]
	public async Task GetServerInfoAsync_WithoutHealthCheck_UsesBaseResourceAndMapsResponse()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);
		var remoteServerInfo = new RemoteServerInfo
		{
			baseUrl = "https://jira.example.com",
			version = "10.0.0",
			versionNumbers = [10, 0, 0],
			deploymentType = "Server",
			buildNumber = 10001,
			buildDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
			serverTime = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero),
			scmInfo = "git",
			buildPartnerName = "Panoramic Data Limited",
			serverTitle = "Jira Test",
			healthChecks = null!
		};

		client.Setup(c => c.ExecuteRequestAsync<RemoteServerInfo>(
				Method.Get,
				"rest/api/2/serverInfo",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteServerInfo);

		var serverInfo = await jira.ServerInfo.GetServerInfoAsync(false, CancellationToken);

		serverInfo.BaseUrl.Should().Be("https://jira.example.com");
		serverInfo.Version.Should().Be("10.0.0");
		serverInfo.VersionNumbers.Should().Equal(10, 0, 0);
		serverInfo.DeploymentType.Should().Be("Server");
		serverInfo.BuildNumber.Should().Be(10001);
		serverInfo.BuildDate.Should().Be(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
		serverInfo.ServerTime.Should().Be(new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero));
		serverInfo.ScmInfo.Should().Be("git");
		serverInfo.BuildPartnerName.Should().Be("Panoramic Data Limited");
		serverInfo.ServerTitle.Should().Be("Jira Test");
		serverInfo.HealthChecks.Should().BeNull();
	}

	[Fact]
	public async Task GetServerInfoAsync_WithHealthCheck_AppendsQueryStringAndMapsChecks()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);
		var remoteServerInfo = new RemoteServerInfo
		{
			baseUrl = "https://jira.example.com",
			version = "10.0.0",
			versionNumbers = [10, 0, 0],
			deploymentType = "Server",
			buildNumber = 10001,
			scmInfo = "git",
			buildPartnerName = "Panoramic Data Limited",
			serverTitle = "Jira Test",
			healthChecks =
			[
				new RemoteHealthCheck { name = "Database", description = "Database connectivity", passed = true },
				new RemoteHealthCheck { name = "Indexing", description = "Indexing backlog", passed = false }
			]
		};

		client.Setup(c => c.ExecuteRequestAsync<RemoteServerInfo>(
				Method.Get,
				"rest/api/2/serverInfo?doHealthCheck=true",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteServerInfo);

		var serverInfo = await jira.ServerInfo.GetServerInfoAsync(true, CancellationToken);
		var healthChecks = serverInfo.HealthChecks!.ToArray();

		healthChecks.Should().HaveCount(2);
		healthChecks[0].Name.Should().Be("Database");
		healthChecks[0].Description.Should().Be("Database connectivity");
		healthChecks[0].Passed.Should().BeTrue();
		healthChecks[1].Name.Should().Be("Indexing");
		healthChecks[1].Passed.Should().BeFalse();
	}
}
