using Jira.Api.Remote;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test;

public class ServiceLocatorTest
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
		jira.Services.Register<IProjectService>(() => projects.Object);

		var versionResource = new Mock<IProjectVersionService>();
		var remoteVersion = new RemoteVersion() { id = "123", name = "my version" };
		var version = new ProjectVersion(jira, remoteVersion);
		versionResource.Setup(s => s.GetVersionsAsync("projKey", CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat(version, 1)));

		jira.Services.Register(() => versionResource.Object);

		// Act
		var versions = await (await jira.Projects.GetProjectsAsync(default)).First().GetVersionsAsync(default);

		// Assert
		Assert.Equal("my version", versions.First().Name);
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
		componentResource.Setup(s => s.GetComponentsAsync("projKey", CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat(component, 1)));

		jira.Services.Register(() => componentResource.Object);

		// Act
		var components = await (await jira.Projects.GetProjectsAsync(default)).First().GetComponentsAsync(default);

		// Assert
		Assert.Equal("my component", components.First().Name);
	}
}
