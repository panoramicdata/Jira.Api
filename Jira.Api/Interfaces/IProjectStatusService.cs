namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on project statuses in Jira.
/// </summary>
public interface IProjectStatusService
{
	/// <summary>
	/// Returns all valid statuses for a project. The statuses are grouped by issue type.
	/// </summary>
	/// <param name="projectKey">The project key or ID.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<ProjectStatusesByIssueType>> GetProjectStatusesAsync(string projectKey, CancellationToken cancellationToken = default);
}
