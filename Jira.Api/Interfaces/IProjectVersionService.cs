namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations for the project versions.
/// </summary>
public interface IProjectVersionService
{
	/// <summary>
	/// Creates a new project version.
	/// </summary>
	Task<ProjectVersion> CreateVersionAsync(ProjectVersionCreationInfo projectVersion, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a project version.
	/// </summary>
	Task DeleteVersionAsync(string versionId, string? moveFixIssuesTo = null, string? moveAffectedIssuesTo = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the versions for a given project.
	/// </summary>
	Task<IEnumerable<ProjectVersion>> GetVersionsAsync(string projectKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the version specified.
	/// </summary>
	Task<ProjectVersion> GetVersionAsync(string versionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates a version and returns a new instance populated from server.
	/// </summary>
	Task<ProjectVersion> UpdateVersionAsync(ProjectVersion version, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the paged versions for a given project (not-cached).
	/// </summary>
	Task<IPagedQueryResult<ProjectVersion>> GetPagedVersionsAsync(string projectKey, int skip, int take, CancellationToken cancellationToken = default);
}
