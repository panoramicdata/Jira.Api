namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class ProjectTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueTypes(JiraClient jira)
	{
		var project = await jira.Projects.GetProjectAsync("TST", CancellationToken);
		var issueTypes = await project.GetIssueTypesAsync(CancellationToken);

		issueTypes.Should().NotBeEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
  [Trait("Category", "WritesToApi")]
	public async Task AddAndRemoveProjectComponent(JiraClient jira)
	{
		var componentName = "New Component " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var projectInfo = new ProjectComponentCreationInfo(componentName);
		var project = (await jira.Projects.GetProjectsAsync(CancellationToken)).First();

		// Add a project component.
		var component = await project.AddComponentAsync(projectInfo, CancellationToken);
		component.Name.Should().Be(componentName);

		// Retrieve project components.
		var components = await project.GetComponentsAsync(CancellationToken);
		components.Should().Contain(p => p.Name == componentName);

		// Delete project component
		await project.DeleteComponentAsync(component.Name, null, CancellationToken);
		components = await project.GetComponentsAsync(CancellationToken);
		components.Should().NotContain(p => p.Name == componentName);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetProjectComponents(JiraClient jira)
	{
		var components = await jira.Components.GetComponentsAsync("TST", CancellationToken);
		components.Should().HaveCount(2);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
 [Trait("Category", "WritesToApi")]
	public async Task GetAndUpdateProjectVersions(JiraClient jira)
	{
		var startDate = new DateTime(2000, 11, 1);
		var versions = await jira.Versions.GetVersionsAsync("TST", CancellationToken);
		(versions.Count() >= 3).Should().BeTrue();

		var version = versions.First(v => v.Name == "1.0");
		var newDescription = "1.0 Release " + RandomNumberGenerator.GetInt32(int.MaxValue);
		version.Description = newDescription;
		version.StartDate = startDate;
		await version.SaveChangesAsync(CancellationToken);

		version.Description.Should().Be(newDescription);
		version = (await jira.Versions.GetVersionsAsync("TST", CancellationToken)).First(v => v.Name == "1.0");
		version.Description.Should().Be(newDescription);
		version.StartDate.Should().Be(startDate);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
 [Trait("Category", "WritesToApi")]
	public async Task AddAndRemoveProjectVersions(JiraClient jira)
	{
		var versionName = "New Version " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var project = (await jira.Projects.GetProjectsAsync(CancellationToken)).First();
		var projectInfo = new ProjectVersionCreationInfo(versionName)
		{
			StartDate = new DateTime(2000, 11, 1)
		};

		// Add a project version.
		var version = await project.AddVersionAsync(projectInfo, CancellationToken);
		version.Name.Should().Be(versionName);
		version.StartDate.Should().Be(projectInfo.StartDate);

		// Retrieve project versions.
		var pagedVersions = await project.GetPagedVersionsAsync(0, 50, CancellationToken);
		pagedVersions.Should().Contain(p => p.Name == versionName);

		// Delete project version
		await project.DeleteVersionAsync(version.Name, null, null, CancellationToken);
		pagedVersions = await project.GetPagedVersionsAsync(0, 50, CancellationToken);
		pagedVersions.Should().NotContain(p => p.Name == versionName);
	}
}



