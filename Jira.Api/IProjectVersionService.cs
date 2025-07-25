﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations for the project versions.
/// Maps to https://docs.atlassian.com/jira/REST/latest/#api/2/version
/// </summary>
public interface IProjectVersionService
{
	/// <summary>
	/// Creates a new project version.
	/// </summary>
	/// <param name="projectVersion">Information of the new project version.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<ProjectVersion> CreateVersionAsync(ProjectVersionCreationInfo projectVersion, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a project version.
	/// </summary>
	/// <param name="versionId">Identifier of the version to delete.</param>
	/// <param name="moveFixIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
	/// <param name="moveAffectedIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteVersionAsync(string versionId, string? moveFixIssuesTo, string? moveAffectedIssuesTo, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the versions for a given project.
	/// </summary>
	/// <param name="projectKey">Key of the project to retrieve versions from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<ProjectVersion>> GetVersionsAsync(string projectKey, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the version specified.
	/// </summary>
	/// <param name="versionId">Identifier of the version.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<ProjectVersion> GetVersionAsync(string versionId, CancellationToken cancellationToken);

	/// <summary>
	/// Updates a version and returns a new instance populated from server.
	/// </summary>
	/// <param name="version">Version to update.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<ProjectVersion> UpdateVersionAsync(ProjectVersion version, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the paged versions for a given project (not-cached).
	/// </summary>
	/// <param name="projectKey">Key of the project to retrieve versions from.</param>
	/// <param name="skip">The page offset, if not specified then defaults to 0.</param>
	/// <param name="take">How many results on the page should be included. Defaults to 50.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IPagedQueryResult<ProjectVersion>> GetPagedVersionsAsync(
		string projectKey,
		int skip,
		int take,
		CancellationToken cancellationToken);
}
