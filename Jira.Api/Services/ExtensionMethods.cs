namespace Jira.Api.Services;

/// <summary>
/// Extension methods for converting between local and remote JIRA types
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public static class ExtensionMethods
{
	/// <summary>
	/// Create a new RemoteIssue based on the information in a given issue.
	/// </summary>
	public static RemoteIssue ToRemote(this Issue issue)
	{
		return issue.ToRemoteAsync(CancellationToken.None).Result;
	}

	/// <summary>
	/// Create a new RemoteIssue based on the information in a given issue.
	/// </summary>
	public static Task<RemoteIssue> ToRemoteAsync(this Issue issue, CancellationToken cancellationToken)
	{
		return issue.ToRemoteIssueAsync(cancellationToken);
	}

	/// <summary>
	/// Create a new Issue from a RemoteIssue
	/// </summary>
	public static Issue ToLocal(this RemoteIssue remoteIssue, JiraClient jira = null)
	{
		return new Issue(jira, remoteIssue);
	}

	/// <summary>
	/// Create a new Attachment from a RemoteAttachment
	/// </summary>
	public static Attachment ToLocal(this RemoteAttachment remoteAttachment, JiraClient jira)
	{
		return new Attachment(jira, remoteAttachment);
	}

	/// <summary>
	/// Creates a new Version from RemoteVersion
	/// </summary>
	public static ProjectVersion ToLocal(this RemoteVersion remoteVersion, JiraClient jira)
	{
		return new ProjectVersion(jira, remoteVersion);
	}

	/// <summary>
	/// Creates a new Component from RemoteComponent
	/// </summary>
	public static ProjectComponent ToLocal(this RemoteComponent remoteComponent)
	{
		return new ProjectComponent(remoteComponent);
	}
}
