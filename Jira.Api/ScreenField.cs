using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// A screen field.
/// </summary>
/// <seealso cref="Atlassian.Jira.JiraNamedEntity" />
/// <remarks>
/// Initializes a new instance of the <see cref="ScreenField"/> class.
/// </remarks>
/// <param name="remoteScreenField">The remote screen field.</param>
public class ScreenField(RemoteScreenField remoteScreenField) : JiraNamedEntity(remoteScreenField)
{

	/// <summary>
	/// Gets the type.
	/// </summary>
	public string Type { get; } = remoteScreenField.type;
}
