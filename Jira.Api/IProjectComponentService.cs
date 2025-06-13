using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations for the project components.
/// Maps to https://docs.atlassian.com/jira/REST/latest/#api/2/component
/// </summary>
public interface IProjectComponentService
{
	/// <summary>
	/// Creates a new project component.
	/// </summary>
	/// <param name="projectComponent">Information of the new component.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<ProjectComponent> CreateComponentAsync(ProjectComponentCreationInfo projectComponent, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a project component.
	/// </summary>
	/// <param name="componentId">Identifier of the component to delete.</param>
	/// <param name="moveIssuesTo">The component to set on issues where the deleted component is the component, If null then the component is removed.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteComponentAsync(string componentId, string? moveIssuesTo, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the components for a given project.
	/// </summary>
	/// <param name="projectKey">Key of the project to retrieve the components from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<ProjectComponent>> GetComponentsAsync(string projectKey, CancellationToken cancellationToken);
}
