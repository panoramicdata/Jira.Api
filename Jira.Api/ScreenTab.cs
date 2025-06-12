using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// A screen tab.
/// </summary>
/// <seealso cref="Atlassian.Jira.JiraNamedEntity" />
/// <remarks>
/// Initializes a new instance of the <see cref="ScreenTab"/> class.
/// </remarks>
/// <param name="remoteScreenTab">The remote screen tab.</param>
public class ScreenTab(RemoteScreenTab remoteScreenTab) : JiraNamedEntity(remoteScreenTab)
{
}
