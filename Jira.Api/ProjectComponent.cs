using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// A component associated with a project
/// </summary>
/// <remarks>
/// Creates a new instance of ProjectComponent.
/// </remarks>
/// <param name="remoteComponent">The remote component.</param>
public class ProjectComponent(RemoteComponent remoteComponent) : JiraNamedEntity(remoteComponent)
{
	private readonly RemoteComponent _remoteComponent = remoteComponent;

	internal RemoteComponent RemoteComponent
	{
		get
		{
			return _remoteComponent;
		}
	}

	/// <summary>
	/// Gets the project key associated with this component.
	/// </summary>
	public string ProjectKey
	{
		get
		{
			return _remoteComponent.ProjectKey;
		}
	}
}
