namespace Jira.Api;

/// <summary>
/// Represents a link between an issue and a remote link.
/// </summary>
/// <remarks>
/// Creates a new IssueRemoteLink instance.
/// </remarks>
public class IssueRemoteLink(string remoteUrl, string title, string summary)
{
	private readonly string _remoteUrl = remoteUrl;
	private readonly string _title = title;
	private readonly string _summary = summary;

	/// <summary>
	/// The remote url of the link relationship.
	/// </summary>
	public string RemoteUrl
	{
		get { return _remoteUrl; }
	}

	/// <summary>
	/// The title / link text.
	/// </summary>
	public string Title
	{
		get { return _title; }
	}

	/// <summary>
	/// The summary / comment.
	/// </summary>
	public string Summary
	{
		get { return _summary; }
	}

}
