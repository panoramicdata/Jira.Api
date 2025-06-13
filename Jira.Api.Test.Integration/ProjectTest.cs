using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class ProjectTest
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueTypes(Jira jira)
	{
		var project = await jira.Projects.GetProjectAsync("TST", default);
		var issueTypes = await project.GetIssueTypesAsync(default);

		Assert.True(issueTypes.Any());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveProjectComponent(Jira jira)
	{
		var componentName = "New Component " + _random.Next(int.MaxValue);
		var projectInfo = new ProjectComponentCreationInfo(componentName);
		var project = (await jira.Projects.GetProjectsAsync(default)).First();

		// Add a project component.
		var component = await project.AddComponentAsync(projectInfo, default);
		Assert.Equal(componentName, component.Name);

		// Retrieve project components.
		Assert.Contains(await project.GetComponentsAsync(default), p => p.Name == componentName);

		// Delete project component
		await project.DeleteComponentAsync(component.Name, null, default);
		Assert.DoesNotContain(await project.GetComponentsAsync(default), p => p.Name == componentName);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetProjectComponents(Jira jira)
	{
		var components = await jira.Components.GetComponentsAsync("TST", default);
		Assert.Equal(2, components.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetAndUpdateProjectVersions(Jira jira)
	{
		var startDate = new DateTime(2000, 11, 1);
		var versions = await jira.Versions.GetVersionsAsync("TST", default);
		Assert.True(versions.Count() >= 3);

		var version = versions.First(v => v.Name == "1.0");
		var newDescription = "1.0 Release " + _random.Next(int.MaxValue);
		version.Description = newDescription;
		version.StartDate = startDate;
		await version.SaveChangesAsync(default);

		Assert.Equal(newDescription, version.Description);
		version = (await jira.Versions.GetVersionsAsync("TST", default)).First(v => v.Name == "1.0");
		Assert.Equal(newDescription, version.Description);
		Assert.Equal(version.StartDate, startDate);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndRemoveProjectVersions(Jira jira)
	{
		var versionName = "New Version " + _random.Next(int.MaxValue);
		var project = (await jira.Projects.GetProjectsAsync(default)).First();
		var projectInfo = new ProjectVersionCreationInfo(versionName)
		{
			StartDate = new DateTime(2000, 11, 1)
		};

		// Add a project version.
		var version = await project.AddVersionAsync(projectInfo, default);
		Assert.Equal(versionName, version.Name);
		Assert.Equal(version.StartDate, projectInfo.StartDate);

		// Retrieve project versions.
		Assert.Contains(await project.GetPagedVersionsAsync(0, 50, default), p => p.Name == versionName);

		// Delete project version
		await project.DeleteVersionAsync(version.Name, null, null, default);
		Assert.DoesNotContain(await project.GetPagedVersionsAsync(0, 50, default), p => p.Name == versionName);
	}
}
