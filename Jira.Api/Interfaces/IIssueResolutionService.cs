namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue resolutions of jira.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IIssueResolutionService
{
	/// <summary>
	/// Returns all the issue resolutions within JIRA.
	/// </summary>
	Task<IEnumerable<IssueResolution>> GetResolutionsAsync(CancellationToken cancellationToken = default);
}
