namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue resolutions of jira.
/// </summary>
public interface IIssueResolutionService
{
	/// <summary>
	/// Returns all the issue resolutions within JIRA.
	/// </summary>
	Task<IEnumerable<IssueResolution>> GetResolutionsAsync(CancellationToken cancellationToken = default);
}
