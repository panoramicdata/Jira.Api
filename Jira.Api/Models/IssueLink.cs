namespace Jira.Api.Models;

/// <summary>
/// Represents a link between two issues.
/// </summary>
/// <remarks>
/// Creates a new IssueLink instance.
/// </remarks>
public class IssueLink(IssueLinkType linkType, Issue outwardIssue, Issue inwardIssue)
{
	/// <summary>
	/// The inward issue of the link relationship.
	/// </summary>
	public Issue InwardIssue { get; } = inwardIssue;

	/// <summary>
	/// The type of the link relationship.
	/// </summary>
	public IssueLinkType LinkType { get; } = linkType;

	/// <summary>
	/// The outward issue of the link relationship.
	/// </summary>
	public Issue OutwardIssue { get; } = outwardIssue;
}
