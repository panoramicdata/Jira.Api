using AwesomeAssertions;

namespace Jira.Api.Test;

public class ServiceLocatorTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Fact]
	public async Task UserCanProvideCustomProjectVersionService()
	{
		// Arrange
		var projects = new Mock<IProjectService>();
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteProject = new RemoteProject() { id = "projId", key = "projKey", name = "my project" };
		projects.Setup(s => s.GetProjectsAsync(It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(Enumerable.Repeat(new Project(jira, remoteProject), 1)));
		jira.Services.Register(() => projects.Object);

		var versionResource = new Mock<IProjectVersionService>();
		var remoteVersion = new RemoteVersion() { id = "123", name = "my version" };
		var version = new ProjectVersion(jira, remoteVersion);
		versionResource.Setup(s => s.GetVersionsAsync("projKey", It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(Enumerable.Repeat(version, 1)));

		jira.Services.Register(() => versionResource.Object);

		// Act
		var versions = await (await jira.Projects.GetProjectsAsync(CancellationToken)).First().GetVersionsAsync(CancellationToken);

		// Assert
		versions.First().Name.Should().Be("my version");
	}

	[Fact]
	public async Task UserCanProvideCustomProjectComponentsService()
	{
		// Arrange
		var projects = new Mock<IProjectService>();
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteProject = new RemoteProject() { id = "projId", key = "projKey", name = "my project" };
		projects.Setup(s => s.GetProjectsAsync(It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(Enumerable.Repeat(new Project(jira, remoteProject), 1)));
		jira.Services.Register(() => projects.Object);

		var componentResource = new Mock<IProjectComponentService>();
		var remoteComponent = new RemoteComponent() { id = "123", name = "my component" };
		var component = new ProjectComponent(remoteComponent);
		componentResource.Setup(s => s.GetComponentsAsync("projKey", It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(Enumerable.Repeat(component, 1)));

		jira.Services.Register(() => componentResource.Object);

		// Act
		var components = await (await jira.Projects.GetProjectsAsync(CancellationToken)).First().GetComponentsAsync(CancellationToken);

		// Assert
		components.First().Name.Should().Be("my component");
	}
}

