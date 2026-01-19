namespace Jira.Api.Models;

/// <summary>
/// Represents a link between an issue and a remote link.
/// </summary>
/// <remarks>
/// Creates a new IssueRemoteLink instance.
/// </remarks>
public class IssueRemoteLink(string remoteUrl, string title, string summary)
{
	/// <summary>
	/// The remote url of the link relationship.
	/// </summary>
	public string RemoteUrl { get; } = remoteUrl;

	/// <summary>
	/// The title / link text.
	/// </summary>
	public string Title { get; } = title;

	/// <summary>
	/// The summary / comment.
	/// </summary>
	public string Summary { get; } = summary;

}
