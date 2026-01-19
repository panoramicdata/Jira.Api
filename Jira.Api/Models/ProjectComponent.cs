namespace Jira.Api.Models;

/// <summary>
/// A component associated with a project
/// </summary>
/// <remarks>
/// Creates a new instance of ProjectComponent.
/// </remarks>
/// <param name="remoteComponent">The remote component.</param>
public class ProjectComponent(RemoteComponent remoteComponent) : JiraNamedEntity(remoteComponent)
{
	internal RemoteComponent RemoteComponent { get; } = remoteComponent;

	/// <summary>
	/// Gets the project key associated with this component.
	/// </summary>
	public string ProjectKey
	{
		get
		{
			return RemoteComponent.ProjectKey;
		}
	}
}
