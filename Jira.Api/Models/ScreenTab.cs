namespace Jira.Api.Models;

/// <summary>
/// A screen tab.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ScreenTab"/> class.
/// </remarks>
/// <param name="remoteScreenTab">The remote screen tab.</param>
public class ScreenTab(RemoteScreenTab remoteScreenTab) : JiraNamedEntity(remoteScreenTab)
{
}
