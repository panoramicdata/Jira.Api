using Jira.Api.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// A JIRA project
/// </summary>
/// <remarks>
/// Creates a new Project instance using a remote project.
/// </remarks>
/// <param name="jira">Instance of the Jira client.</param>
/// <param name="remoteProject">Remote project.</param>
public class Project(Jira jira, RemoteProject remoteProject) : JiraNamedEntity(remoteProject)
{
	private readonly Jira _jira = jira;
	private readonly RemoteProject _remoteProject = remoteProject;

	internal RemoteProject RemoteProject
	{
		get
		{
			return _remoteProject;
		}
	}

	/// <summary>
	/// The unique identifier of the project.
	/// </summary>
	public string Key
	{
		get
		{
			return _remoteProject.key;
		}
	}

	/// <summary>
	/// The category set on this project.
	/// </summary>
	public ProjectCategory Category
	{
		get
		{
			return _remoteProject.projectCategory;
		}
	}

	/// <summary>
	/// Username of the project lead.
	/// </summary>
	public string Lead
	{
		get
		{
			return _remoteProject.leadUser?.InternalIdentifier;
		}
	}

	/// <summary>
	/// User object of the project lead.
	/// </summary>
	public JiraUser LeadUser
	{
		get
		{
			return _remoteProject.leadUser;
		}
	}

	/// <summary>
	/// The URL set on the project.
	/// </summary>
	public string Url
	{
		get
		{
			return _remoteProject.url;
		}
	}

	/// <summary>
	/// The list of the Avatar URL's
	/// </summary>
	public AvatarUrls AvatarUrls
	{
		get
		{
			return _remoteProject.avatarUrls;
		}
	}

	/// <summary>
	/// Gets the issue types for the current project.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken cancellationToken)
	{
		return _jira.IssueTypes.GetIssueTypesForProjectAsync(Key, cancellationToken);
	}

	/// <summary>
	/// Creates a new project component.
	/// </summary>
	/// <param name="projectComponent">Information of the new component.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<ProjectComponent> AddComponentAsync(ProjectComponentCreationInfo projectComponent, CancellationToken cancellationToken)
	{
		projectComponent.ProjectKey = Key;
		return _jira.Components.CreateComponentAsync(projectComponent, cancellationToken);
	}

	/// <summary>
	/// Gets the components for the current project.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<ProjectComponent>> GetComponentsAsync(CancellationToken cancellationToken)
	{
		return _jira.Components.GetComponentsAsync(Key, cancellationToken);
	}

	/// <summary>
	/// Deletes a project component.
	/// </summary>
	/// <param name="componentName">Name of the component to remove.</param>
	/// <param name="moveIssuesTo">The component to set on issues where the deleted component is the component, If null then the component is removed.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task DeleteComponentAsync(
		string componentName,
		string? moveIssuesTo,
		CancellationToken cancellationToken)
	{
		var components = await GetComponentsAsync(cancellationToken).ConfigureAwait(false);
		var component = components.First(c => string.Equals(c.Name, componentName)) ?? throw new InvalidOperationException($"Unable to locate a component with name '{componentName}'");
		await _jira.Components.DeleteComponentAsync(component.Id, moveIssuesTo, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Creates a new project version.
	/// </summary>
	/// <param name="projectVersion">Information of the new project version.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<ProjectVersion> AddVersionAsync(ProjectVersionCreationInfo projectVersion, CancellationToken cancellationToken)
	{
		projectVersion.ProjectKey = Key;
		return _jira.Versions.CreateVersionAsync(projectVersion, cancellationToken);
	}

	/// <summary>
	/// Gets the versions for this project.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<ProjectVersion>> GetVersionsAsync(CancellationToken cancellationToken)
	{
		return _jira.Versions.GetVersionsAsync(Key, cancellationToken);
	}

	/// <summary>
	/// Gets the paged versions for this project (not-cached).
	/// </summary>
	/// <param name="skip">The page offset, if not specified then defaults to 0.</param>
	/// <param name="take">How many results on the page should be included. Suggest 50.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IPagedQueryResult<ProjectVersion>> GetPagedVersionsAsync(
		int skip,
		int take,
		CancellationToken cancellationToken)
	{
		return _jira.Versions.GetPagedVersionsAsync(Key, skip, take, cancellationToken);
	}

	/// <summary>
	/// Deletes a project version.
	/// </summary>
	/// <param name="versionName">Name of the version to delete.</param>
	/// <param name="moveFixIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
	/// <param name="moveAffectedIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task DeleteVersionAsync(
		string versionName,
		string? moveFixIssuesTo,
		string? moveAffectedIssuesTo,
		CancellationToken cancellationToken)
	{
		var versions = await GetVersionsAsync(cancellationToken).ConfigureAwait(false);
		var version = versions.FirstOrDefault(v => string.Equals(v.Name, versionName, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Unable to locate a version with name '{versionName}'");
		await _jira.Versions.DeleteVersionAsync(version.Id, moveFixIssuesTo, moveAffectedIssuesTo, cancellationToken).ConfigureAwait(false);
	}
}
