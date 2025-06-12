namespace Jira.Api;

/// <summary>
/// Represents a link between two issues.
/// </summary>
/// <remarks>
/// Creates a new IssueLink instance.
/// </remarks>
public class IssueLink(IssueLinkType linkType, Issue outwardIssue, Issue inwardIssue)
{
	private readonly IssueLinkType _linkType = linkType;
	private readonly Issue _outwardIssue = outwardIssue;
	private readonly Issue _inwardIssue = inwardIssue;

	/// <summary>
	/// The inward issue of the link relationship.
	/// </summary>
	public Issue InwardIssue
	{
		get { return _inwardIssue; }
	}

	/// <summary>
	/// The type of the link relationship.
	/// </summary>
	public IssueLinkType LinkType
	{
		get { return _linkType; }
	}

	/// <summary>
	/// The outward issue of the link relationship.
	/// </summary>
	public Issue OutwardIssue
	{
		get { return _outwardIssue; }
	}
}
