namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations for the project components.
/// </summary>
public interface IProjectComponentService
{
	/// <summary>
	/// Creates a new project component.
	/// </summary>
	Task<ProjectComponent> CreateComponentAsync(ProjectComponentCreationInfo projectComponent, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a project component.
	/// </summary>
	Task DeleteComponentAsync(string componentId, string? moveIssuesTo = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the components for a given project.
	/// </summary>
	Task<IEnumerable<ProjectComponent>> GetComponentsAsync(string projectKey, CancellationToken cancellationToken = default);
}
