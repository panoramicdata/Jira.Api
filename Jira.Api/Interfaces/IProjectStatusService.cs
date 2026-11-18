namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on project statuses in Jira.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IProjectStatusService
{
	/// <summary>
	/// Returns all valid statuses for a project. The statuses are grouped by issue type.
	/// </summary>
	/// <param name="projectKey">The project key or ID.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<ProjectStatusesByIssueType>> GetProjectStatusesAsync(string projectKey, CancellationToken cancellationToken = default);
}
