namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the projects of jira.
/// </summary>
public interface IProjectService
{
	/// <summary>
	/// Returns all projects defined in JIRA.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns a single project in JIRA.
	/// </summary>
	/// <param name="projectKey">Project key for the single project to load</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<Project> GetProjectAsync(string projectKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all statuses for a project, grouped by issue type.
	/// This gives workflow-specific statuses rather than all global statuses.
	/// </summary>
	/// <param name="projectKeyOrId">The project key (e.g., "PRJ") or numeric ID.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <returns>Statuses grouped by issue type.</returns>
	Task<IEnumerable<IssueTypeWithStatuses>> GetProjectStatusesAsync(string projectKeyOrId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the workflow scheme for a project.
	/// </summary>
	/// <param name="projectKeyOrId">The project key (e.g., "PRJ") or numeric ID.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <returns>The project's workflow scheme.</returns>
	Task<ProjectWorkflowScheme?> GetProjectWorkflowSchemeAsync(string projectKeyOrId, CancellationToken cancellationToken = default);
}
